using System.Text;
using CompilerLib.Nodes;
using CompilerLib.Nodes.Functions;
using CompilerLib.Nodes.Operators;
using CompilerLib.Nodes.Scopes;
using CompilerLib.Nodes.Statements;
using CompilerLib.Nodes.Statements.Controls;
using CompilerLib.Nodes.Types;
using static CompilerLib.SemanticAnalysis.SymbolTable;

namespace CompilerLib.SemanticAnalysis;

public class SemanticAnalyzer : ISemanticAnalyzer
{
    public SymbolTable SymbolTable { get; } = new();

    delegate bool AnalysisStage(SyntaxNode rootNode, StringBuilder messageStringBuilder);
    readonly AnalysisStage[] analysisStages;

    public SemanticAnalyzer()
    {
        analysisStages =
        [
            RegisterPrimitives,
            TryBuildSymbolTable,
            TryValidateScopesAndTypes,
        ];
    }

    public bool Analyze(SyntaxNode rootNode, out string completionMessage)
    {
        StringBuilder messageSB = new();
        foreach (AnalysisStage stage in analysisStages)
        {
            if (!stage.Invoke(rootNode, messageSB))
            {
                completionMessage = messageSB.ToString();
                return false;
            }
        }
        completionMessage = $"Semantic analysis completed successfully with the following messages:\n{messageSB}";
        return true;
    }

    private static void LogMessage(StringBuilder builder, SyntaxNode node, string message)
        => builder.AppendLine($"\t[{node.StartLine}.{node.StartChar}-{node.EndLine}.{node.EndChar}] {message}");

    private bool RegisterPrimitives(SyntaxNode _, StringBuilder messageSB)
    {
        SymbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, LanguageNames.Primitives.Int8, LanguageNames.Primitives.Int8);
        SymbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, LanguageNames.Primitives.Int16, LanguageNames.Primitives.Int16);
        SymbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, LanguageNames.Primitives.Int32, LanguageNames.Primitives.Int32);
        SymbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, LanguageNames.Primitives.Int64, LanguageNames.Primitives.Int64);
        SymbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, LanguageNames.Primitives.Bool, LanguageNames.Primitives.Bool);

        messageSB.AppendLine("Primitive types registered successfully.");
        return true;
    }
    private bool TryBuildSymbolTable(SyntaxNode rootNode, StringBuilder messageSB)
    {
        uint scopeID = 1;
        bool hasFailed = false;
        messageSB.AppendLine("Symbol table built with the following messages:");

        BuildSymbolTable(rootNode, currentScopeID: 0, symbolPosition: 0, isLocal: false);

        if (hasFailed)
        {
            return false;
        }
        messageSB.AppendLine("\tSymbol table built successfully.");
        return true;

        int BuildSymbolTable(IHasChildren node, uint currentScopeID, int symbolPosition, bool isLocal)
        {
            Queue<IContainsScopeNode> nodesToProcess = new();

            for (int i = 0; i < node.Children.Count; i++)
            {
                symbolPosition++;

                SyntaxNode? child = node.Children[i];
                if (child is IContainsScopeNode scopeContainer)
                {
                    if (!isLocal)
                    {
                        nodesToProcess.Enqueue(scopeContainer);
                        continue;
                    }
                    HandleScopeContainer(scopeContainer);
                }
                else if (child is VariableDefinitionNode varDefNode)
                {
                    if (SymbolTable.TryGetSymbolInfo(currentScopeID, varDefNode.NameTypeNode.Name, out SymbolInfo? symbolInfo))
                    {
                        if (!isLocal || isLocal && symbolInfo.EnclosingScope.IsLocal)
                        {
                            hasFailed = true;
                            messageSB.AppendLine($"\t[{varDefNode.StartLine}.{varDefNode.StartChar}-{varDefNode.EndLine}.{varDefNode.EndChar}] "
                                + $"Variable with name '{varDefNode.NameTypeNode.Name}' already exists in scope!");
                        }
                        else
                        {
                            messageSB.AppendLine($"\t[{varDefNode.StartLine}.{varDefNode.StartChar}-{varDefNode.EndLine}.{varDefNode.EndChar}] "
                                + $"WARNING: Variable with name '{varDefNode.NameTypeNode.Name}' is being shadowed!");
                        }
                    }
                    else
                    {
                        SymbolTable.AddSymbol(currentScopeID, symbolPosition, varDefNode.NameTypeNode.Name, varDefNode.NameTypeNode.Type);
                    }
                }
            }

            while (nodesToProcess.Count > 0)
            {
                IContainsScopeNode scopeContainer = nodesToProcess.Dequeue();
                HandleScopeContainer(scopeContainer);
            }
            return symbolPosition;

            void HandleScopeContainer(IContainsScopeNode scopeContainer)
            {
                bool isScopeContainerLocal = isLocal;
                if (scopeContainer is FunctionDefinitionNode funcDefNode)
                {
                    isScopeContainerLocal = true;

                    FunctionInfo funcInfo = SymbolTable.AddFunction(scopeID, currentScopeID, funcDefNode);
                    funcDefNode.FunctionInfo = funcInfo;
                    funcDefNode.FunctionBlockNode.FunctionInfo = funcInfo;
                    scopeContainer.Block.ID = scopeID;
                    BuildSymbolTable(scopeContainer.Block, scopeID++, symbolPosition: 0, isScopeContainerLocal);
                    return;
                }

                string name = scopeContainer.Name;
                scopeContainer.Block.ID = scopeID;
                SymbolTable.AddScope(scopeID, name, isScopeContainerLocal, currentScopeID);
                BuildSymbolTable(scopeContainer.Block, scopeID++, symbolPosition: 0, isScopeContainerLocal);
            }
        }
    }
    private bool TryValidateScopesAndTypes(SyntaxNode rootNode, StringBuilder messageSB)
    {
        messageSB.AppendLine("Scope completed with the following messages:");

        bool failFlag = false;
        ValidateNonLocal(SymbolTable, ref failFlag, messageSB, rootNode, currentScopeID: 0);

        if (failFlag)
        {
            return false;
        }

        messageSB.AppendLine("\tScope validation completed successfully.");
        return true;

        static void ValidateNonLocal(SymbolTable symbolTable, ref bool failFlag, StringBuilder messageSB, IHasChildren node, uint currentScopeID)
        {
            foreach (SyntaxNode? child in node.Children)
            {
                switch (child)
                {
                    case VariableDefinitionNode varDefNode:
                        IdentifierLeaf varID = varDefNode.NameTypeNode.Identifier;

                        TryValidateVariableDefinition(symbolTable, ref failFlag, messageSB, varDefNode, currentScopeID);
                        break;

                    case IContainsScopeNode scopeContainer:
                        if (child is FunctionDefinitionNode funcDefNode)
                        {
                            if (funcDefNode.FunctionInfo == null)
                            {
                                throw new InvalidOperationException("Function definition node does not have function info set!");
                            }

                            ValidateLocal(symbolTable, ref failFlag, messageSB, funcDefNode.FunctionBlockNode, funcDefNode.FunctionInfo, statementPosition: 0);
                        }
                        else if (child is NamespaceDefinitionNode namespaceDefNode)
                        {
                            ValidateNonLocal(symbolTable, ref failFlag, messageSB, namespaceDefNode.Block, currentScopeID);
                        }
                        else
                        {
                            throw new NotImplementedException("Scope container not implemented: " + child.GetType().Name);
                        }

                        break;

                    default: throw new NotImplementedException("Non-Local node type not implemented: " + child.GetType().Name);
                }
            }
        }

        static void ValidateLocal(SymbolTable symbolTable, ref bool failFlag, StringBuilder messageSB, IHasChildren node, FunctionInfo funcInfo, int statementPosition)
        {
            foreach (SyntaxNode? child in node.Children)
            {
                statementPosition++;

                switch (child)
                {
                    case VariableDefinitionNode varDefNode:
                        IdentifierLeaf varID = varDefNode.NameTypeNode.Identifier;
                        TryValidateVariableDefinition(symbolTable, ref failFlag, messageSB, varDefNode, funcInfo.ChildScopeInfo.ID);
                        break;

                    case AssignmentStatementNode assignmentNode:
                        TryValidateIdentifierScope(symbolTable, ref failFlag, messageSB, assignmentNode.Identifier, funcInfo.ChildScopeInfo.ID, statementPosition);
                        TryValidateIdentifierScope(symbolTable, ref failFlag, messageSB, assignmentNode.AssignedValue, funcInfo.ChildScopeInfo.ID, statementPosition);

                        if (!symbolTable.TryGetSymbolInfo(funcInfo.ChildScopeInfo.ID, assignmentNode.Identifier.Value, out SymbolInfo? info))
                        {
                            throw new InvalidOperationException($"Symbol table failed to build variable definitions correctly! "
                                + $"Could not find symbol: ({assignmentNode.Identifier.Value})");
                        }

                        string assignedType = ResolveType(symbolTable, ref failFlag, messageSB, assignmentNode.AssignedValue, funcInfo.ChildScopeInfo.ID);

                        if (info.Type != assignedType)
                        {
                            failFlag = true;
                            LogMessage(messageSB, assignmentNode.AssignedValue, $"Cannot assign type of '{assignedType}' to variable {info.Name} of type '{info.Type}'");
                        }
                        break;

                    case FunctionCallStatementNode funcCallNode:
                        if (!symbolTable.ContainsFunctionName(funcInfo.ChildScopeInfo.ID, funcCallNode.FunctionCallExpression.Identifier.Value))
                        {
                            failFlag = true;
                            LogMessage(
                                messageSB,
                                funcCallNode.FunctionCallExpression.Identifier,
                                $"The function '{funcCallNode.FunctionCallExpression.Identifier.Value}' does not exist in the current context!");
                            continue;
                        }
                        ResolveType(symbolTable, ref failFlag, messageSB, funcCallNode.FunctionCallExpression, funcInfo.ChildScopeInfo.ID);

                        break;

                    case ReturnStatementNode returnNode:
                        if (returnNode.ReturnValue == null)
                        {
                            continue;
                        }

                        string returnType = ResolveType(symbolTable, ref failFlag, messageSB, returnNode.ReturnValue, funcInfo.ChildScopeInfo.ID);

                        if (funcInfo.SignatureInfo.Type != returnType)
                        {
                            failFlag = true;
                            LogMessage(messageSB, returnNode, $"Return type '{returnType}' does not match function return type '{funcInfo.SignatureInfo.Type}'!");
                        }
                        break;

                    case EmptyStatementNode: break;

                    case IContainsScopeNode scopeContainer:
                        if (scopeContainer is WhileStatementNode whileNode)
                        {
                            string conditionType = ResolveType(symbolTable, ref failFlag, messageSB, whileNode.Condition, funcInfo.ChildScopeInfo.ID);

                            if (conditionType != LanguageNames.Primitives.Bool)
                            {
                                failFlag = true;
                                LogMessage(messageSB, whileNode, $"While loop condition must be of type 'bool', found '{conditionType}' instead!");
                            }
                        }

                        uint scopeID = scopeContainer.Block.ID;
                        ValidateLocal(symbolTable, ref failFlag, messageSB, scopeContainer.Block, funcInfo, statementPosition);
                        break;
                }
            }
        }

        static bool TryValidateVariableDefinition(SymbolTable symbolTable, ref bool failFlag, StringBuilder messageSB, VariableDefinitionNode node, uint scopeID)
        {
            if (!symbolTable.ContainsSymbol(scopeID, node.NameTypeNode.Type))
            {
                LogMessage(messageSB, node.NameTypeNode, $"The type '{node.NameTypeNode.Type}' is not defined in the current context!");
                failFlag = true;
            }

            if (!symbolTable.TryGetSymbolInfo(scopeID, name: node.NameTypeNode.Name, out SymbolInfo? varInfo))
            {
                throw new InvalidOperationException($"Symbol table failed to build variable definitions correctly! "
                    + $"Could not find symbol: ({node.NameTypeNode.Name})");
            }

            if (!TryValidateValueExprScope(symbolTable, ref failFlag, messageSB, node.AssignedValue, scopeID, varInfo))
            {
                return false;
            }

            string rhsType = ResolveType(symbolTable, ref failFlag, messageSB, node.AssignedValue, varInfo.EnclosingScope.ID);

            if (node.NameTypeNode.Type == rhsType)
            {
                return true;
            }

            failFlag = true;
            LogMessage(messageSB, node, $"Cannot assign type of '{rhsType}' to variable {node.NameTypeNode.Name} of type '{node.NameTypeNode.Type}'");
            return false;

            static bool TryValidateValueExprScope(SymbolTable symbolTable, ref bool failFlag, StringBuilder messageSB, IHasChildren node, uint currentScopeID, SymbolInfo definedVarInfo)
            {
                switch (node)
                {
                    case IdentifierLeaf id:
                        {
                            if (!symbolTable.TryGetSymbolInfo(currentScopeID, id.Value, out SymbolInfo? symInfo))
                            {
                                failFlag = true;
                                LogMessage(messageSB, id, $"The identifier '{id.Value}' does not exist in the current context!");
                                return false;
                            }

                            if (symInfo.EnclosingScope.IsLocal)
                            {
                                if (symInfo.SymbolPosition < definedVarInfo.SymbolPosition)
                                {
                                    return true;
                                }

                                failFlag = true;
                                LogMessage(messageSB, id, $"Cannot use identifier '{id.Value}' before is declared!");
                                return false;
                            }
                            else if (!definedVarInfo.EnclosingScope.IsLocal)
                            {
                                failFlag = true;
                                LogMessage(messageSB, id, $"A field definition cannot reference another field '{id.Value}'!");
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }

                    case FunctionCallExpressionNode funcCallExpr:
                        if (symbolTable.ContainsFunction(currentScopeID, funcCallExpr.Identifier.Value))
                        {
                            return true;
                        }

                        string funcSignature = GenerateFunctionSignature(symbolTable, ref failFlag, messageSB, funcCallExpr, currentScopeID);

                        if (!symbolTable.TryGetFunctionInfo(currentScopeID, funcSignature, out FunctionInfo? funcInfo))
                        {
                            failFlag = true;
                            LogMessage(messageSB, funcCallExpr.Identifier, $"No function overload matching signature '{funcSignature}' is defined in the current context!");
                            return false;
                        }
                        return true;

                    default:
                        {
                            bool hasThisFailed = false;

                            foreach (SyntaxNode? child in node.Children)
                            {
                                if (!TryValidateValueExprScope(symbolTable, ref failFlag, messageSB, child, currentScopeID, definedVarInfo))
                                {
                                    hasThisFailed = true;
                                }
                            }

                            return !hasThisFailed;
                        }
                }
            }
        }

        static bool TryValidateIdentifierScope(SymbolTable symbolTable, ref bool failFlag, StringBuilder messageSB, SyntaxNode node, uint currentScopeID, int statementPosition)
        {
            switch (node)
            {
                case IdentifierLeaf id:
                    if (!symbolTable.TryGetSymbolInfo(currentScopeID, id.Value, out SymbolInfo? symInfo))
                    {
                        failFlag = true;
                        LogMessage(messageSB, id, $"The identifier '{id.Value}' does not exist in the current context!");
                        return false;
                    }
                    if (symInfo.EnclosingScope.IsLocal && symInfo.SymbolPosition >= statementPosition)
                    {
                        failFlag = true;
                        LogMessage(messageSB, id, $"Cannot use identifier '{id.Value}' before is declared!");
                        return false;
                    }
                    return true;

                default:
                    bool haveIFailed = false;
                    foreach (SyntaxNode? child in node.Children)
                    {
                        if (!TryValidateIdentifierScope(symbolTable, ref failFlag, messageSB, child, currentScopeID, statementPosition))
                        {
                            haveIFailed = true;
                        }
                    }
                    return !haveIFailed;
            }
        }
    }

    private static string GenerateFunctionSignature(SymbolTable symbolTable, ref bool failFlag, StringBuilder messageSB, FunctionCallExpressionNode funcCallExpr, uint currentScopeID)
    {
        string name = funcCallExpr.Identifier.Value;
        List<SyntaxNode> arguments = funcCallExpr.ArgumentList.Children;

        List<string> argumentTypes = new(arguments.Count);
        foreach (SyntaxNode arg in arguments)
        {
            string argType = ResolveType(symbolTable, ref failFlag, messageSB, arg, currentScopeID);
            argumentTypes.Add(argType);
        }
        return SymbolTable.GetFunctionSignature(name, argumentTypes);
    }
    private static string ResolveType(SymbolTable symbolTable, ref bool failFlag, StringBuilder messageSB, IHasChildren node, uint currentScopeID)
    {
        switch (node)
        {
            case ValueOperationNode:
                {
                    switch (node)
                    {
                        case UnaryOperationNode:
                            if (node is NotOperationNode notOp)
                            {
                                string type = ResolveType(symbolTable, ref failFlag, messageSB, notOp.Operand, currentScopeID);

                                if (type != LanguageNames.Primitives.Bool)
                                {
                                    failFlag = true;
                                    LogMessage(messageSB, notOp, $"Operator '!' cannot be applied to operand of type {type}");
                                }
                                return type;
                            }
                            throw new NotImplementedException("Unary operation not implemented: " + node.GetType().Name);


                        case BinaryOperationNode binaryOp:
                            {
                                string lhsType = ResolveType(symbolTable, ref failFlag, messageSB, binaryOp.LeftOperand, currentScopeID);
                                string rhsType = ResolveType(symbolTable, ref failFlag, messageSB, binaryOp.RightOperand, currentScopeID);

                                switch (binaryOp)
                                {
                                    case AddOperationNode
                                        or SubtractOperationNode
                                        or MultiplyOperationNode
                                        or DivideOperationNode
                                        or ModOperationNode:
                                        if (lhsType is not (LanguageNames.Primitives.Int8 or LanguageNames.Primitives.Int16 or LanguageNames.Primitives.Int32 or LanguageNames.Primitives.Int64)
                                        || rhsType is not (LanguageNames.Primitives.Int8 or LanguageNames.Primitives.Int16 or LanguageNames.Primitives.Int32 or LanguageNames.Primitives.Int64))
                                        {
                                            failFlag = true;
                                            LogMessage(messageSB, binaryOp, $"Operator '{binaryOp.Operator}' cannot be applied to operands of type {lhsType} and {rhsType}");
                                        }
                                        return lhsType;

                                    case OrOperationNode
                                        or AndOperationNode:
                                        if (lhsType != LanguageNames.Primitives.Bool || rhsType != LanguageNames.Primitives.Bool)
                                        {
                                            failFlag = true;
                                            LogMessage(messageSB, binaryOp, $"Operator '{binaryOp}' cannot be applied to operands of type {lhsType} and {rhsType}");
                                        }
                                        return lhsType;

                                    case EqualityOperationNode:
                                        return LanguageNames.Primitives.Bool;
                                }
                                throw new NotImplementedException("Binary operation not implemented: " + node.GetType().Name);
                            }
                    }
                    throw new NotImplementedException("Non-Unary-Or-Binary operation not implemented: " + node.GetType().Name);
                }

            case IdentifierLeaf identifier:
                return symbolTable.TryGetSymbolInfo(currentScopeID, identifier.Value, out SymbolInfo? idInfo)
                    ? idInfo.Type
                    : throw new InvalidOperationException($"Scope check failed! Encountered undefined identifier: {identifier}");

            case LiteralLeaf:
                if (node is IntLiteralLeaf)
                {
                    return LanguageNames.Primitives.Int32;
                }

                if (node is BoolLiteralLeaf)
                {
                    return LanguageNames.Primitives.Bool;
                }

                throw new NotImplementedException("Node type not supported for type resolution: " + node.GetType().Name);

            case FunctionCallExpressionNode funcCallExpr:
                string funcSignature = GenerateFunctionSignature(symbolTable, ref failFlag, messageSB, funcCallExpr, currentScopeID);

                if (!symbolTable.TryGetFunctionInfo(currentScopeID, funcSignature, out FunctionInfo? funcInfo))
                {
                    throw new InvalidOperationException("Scope check failed! Encountered unknown function overload!");
                }

                funcCallExpr.FunctionInfo = funcInfo;
                return funcInfo.SignatureInfo.Type;

            default: throw new NotImplementedException("Node type not supported for type resolution: " + node.GetType().Name);
        }
    }

    public string GetPrintable() => SymbolTable.GetPrintable();
}