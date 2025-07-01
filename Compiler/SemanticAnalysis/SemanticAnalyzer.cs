using System.Text;
using CompilerLib;
using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Functions;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Scopes;
using CompilerLib.Parser.Nodes.Statements;
using CompilerLib.Parser.Nodes.Statements.Controls;
using CompilerLib.Parser.Nodes.Types;
using static CompilerLib.SymbolTable;

namespace Compiler.SemanticAnalysis
{
    public class SemanticAnalyzer
    {
        readonly SymbolTable symbolTable = new();

        delegate bool AnalysisStage(SyntaxNode rootNode, StringBuilder messageStringBuilder);
        readonly AnalysisStage[] analysisStages;

        public SemanticAnalyzer()
        {
            analysisStages =
            [
                RegisterPrimitives,
                TryBuildSymbolTable,
                TryValidateScopes,
            ];
        }

        public bool Analyze(SyntaxNode rootNode, out string completionMessage)
        {
            StringBuilder messageSB = new();
            foreach (var stage in analysisStages)
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

        private bool RegisterPrimitives(SyntaxNode _, StringBuilder messageSB)
        {
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypeNames.Int8, PrimitiveTypeNames.Int8);
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypeNames.Int16, PrimitiveTypeNames.Int16);
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypeNames.Int32, PrimitiveTypeNames.Int32);
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypeNames.Int64, PrimitiveTypeNames.Int64);
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypeNames.Bool, PrimitiveTypeNames.Bool);

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
                        if (symbolTable.TryGetSymbolInfo(currentScopeID, varDefNode.NameTypeNode.Name, out var symbolInfo))
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
                            symbolTable.AddSymbol(currentScopeID, symbolPosition, varDefNode.NameTypeNode.Name, varDefNode.NameTypeNode.Type);
                        }
                    }
                }

                while (nodesToProcess.Count > 0)
                {
                    var scopeContainer = nodesToProcess.Dequeue();
                    HandleScopeContainer(scopeContainer);
                }
                return symbolPosition;

                void HandleScopeContainer(IContainsScopeNode scopeContainer)
                {
                    var isScopeContainerLocal = isLocal;
                    if (scopeContainer is FunctionDefinitionNode funcDefNode)
                    {
                        isScopeContainerLocal = true;
                        if (symbolTable.TryGetSymbolInfo(currentScopeID, funcDefNode.Name, out SymbolInfo? info))
                        {
                            hasFailed = true;
                            messageSB.AppendLine($"\t[{funcDefNode.StartLine}.{funcDefNode.StartChar}-{funcDefNode.EndLine}.{funcDefNode.EndChar}] "
                                    + $"Function with name '{funcDefNode.Name}' already exists in scope!");
                        }
                        else
                        {
                            symbolTable.AddSymbol(currentScopeID, symbolPosition, funcDefNode.Name, funcDefNode.ReturnTypeName);
                        }

                        string funcName = scopeContainer.Name;
                        scopeContainer.Block.ID = scopeID;
                        funcDefNode.FunctionBlockNode.ScopeInfo = symbolTable.AddScope(scopeID, funcName, isScopeContainerLocal, currentScopeID);
                        BuildSymbolTable(scopeContainer.Block, scopeID++, symbolPosition: 0, isScopeContainerLocal);
                        return;
                    }

                    string name = scopeContainer.Name;
                    scopeContainer.Block.ID = scopeID;
                    symbolTable.AddScope(scopeID, name, isScopeContainerLocal, currentScopeID);
                    BuildSymbolTable(scopeContainer.Block, scopeID++, symbolPosition: 0, isScopeContainerLocal);
                }
            }
        }
        private bool TryValidateScopes(SyntaxNode rootNode, StringBuilder messageSB)
        {
            bool hasFailed = false;
            messageSB.AppendLine("Scope validation failed:");

            ValidateScopesAndTypes(rootNode, 0, 0, false);
            if (hasFailed)
            {
                return false;
            }

            messageSB.AppendLine("\tScope validation completed successfully.");
            return true;

            void ValidateScopesAndTypes(IHasChildren node, uint currentScopeID, int statementPosition, bool isLocal)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    statementPosition++;
                    SyntaxNode? child = node.Children[i];

                    if (child is VariableDefinitionNode varDefNode)
                    {
                        IdentifierLeaf varID = varDefNode.NameTypeNode.Identifier;

                        if (!symbolTable.TryGetSymbolInfo(currentScopeID, name: varID.Value, out SymbolInfo? nameInfo))
                            throw new InvalidOperationException($"Symbol table failed to build variable definitions correctly!"
                                + "Could not find symbol: ({varID})");

                        if (!symbolTable.ContainsSymbol(currentScopeID, varDefNode.NameTypeNode.Type))
                        {
                            hasFailed = true;
                            messageSB.AppendLine($"\t[{varDefNode.NameTypeNode.StartLine}.{varDefNode.NameTypeNode.StartChar}-{varDefNode.NameTypeNode.EndLine}.{varDefNode.NameTypeNode.EndChar}] "
                                + $"The type '{varDefNode.NameTypeNode.Type}' is not defined in the current context!");
                        }

                        if (TryValidateVarDefScope(varDefNode.AssignedValue, currentScopeID, nameInfo)) // Will not validate type if scope check fails
                        {
                            ValidateVarDefType(varDefNode, currentScopeID, messageSB);
                        }
                    }
                    else if (child is IContainsScopeNode scopeContainer)
                    {
                        if (scopeContainer is WhileStatementNode whileNode)
                        {
                            string conditionType = ResolveType(whileNode.Condition, currentScopeID, messageSB);

                            if (conditionType != PrimitiveTypeNames.Bool)
                            {
                                hasFailed = true;
                                messageSB.AppendLine($"\t[{whileNode.StartLine}.{whileNode.StartChar}-{whileNode.EndLine}.{whileNode.EndChar}] "
                                    + $"While loop condition must be of type 'bool', found '{conditionType}' instead!");
                            }
                        }

                        if (child is FunctionDefinitionNode funcDefNode)
                        {
                            isLocal = true;
                            statementPosition = 0;
                        }

                        uint scopeID = scopeContainer.Block.ID;
                        ValidateScopesAndTypes(scopeContainer.Block, scopeID, statementPosition, isLocal);
                    }
                    else if (child is AssignmentStatementNode assignment)
                    {
                        if (TryValidateAssignmentOrFuncCallScope(assignment, currentScopeID, statementPosition))
                        {
                            ValidateAssignment(assignment, currentScopeID, messageSB);
                        }
                    }
                    else if (child is FunctionCallStatementNode funcCall)
                    {
                        if (TryValidateAssignmentOrFuncCallScope(funcCall, currentScopeID, statementPosition))
                        {

                            ValidateFunctionCall(funcCall, currentScopeID, messageSB);
                        }
                    }
                    else throw new NotImplementedException();
                }
            }
            bool TryValidateVarDefScope(IHasChildren node, uint currentScopeID, SymbolInfo definedVarInfo)
            {
                bool hasThisFailed = false;
                if (node is IdentifierLeaf id)
                {
                    if (!symbolTable.TryGetSymbolInfo(currentScopeID, id.Value, out SymbolInfo? symInfo))
                    {
                        hasThisFailed = true;
                        hasFailed = true;
                        messageSB.AppendLine($"\t[{id.StartLine}.{id.StartChar}-{id.EndLine}.{id.EndChar}] The identifier '{id.Value}' does not exist in the current context!");
                    }
                    else
                    {
                        if (symInfo.EnclosingScope.IsLocal)
                        {
                            if (symInfo.SymbolPosition >= definedVarInfo.SymbolPosition)
                            {
                                hasThisFailed = true;
                                hasFailed = true;
                                messageSB.AppendLine($"\t[{id.StartLine}.{id.StartChar}-{id.EndLine}.{id.EndChar}] Cannot use identifier '{id.Value}' before is declared!");
                            }
                        }
                        else if (!definedVarInfo.EnclosingScope.IsLocal)
                        {
                            hasThisFailed = true;
                            hasFailed = true;
                            messageSB.AppendLine($"\t[{id.StartLine}.{id.StartChar}-{id.EndLine}.{id.EndChar}] A field definition cannot reference another field '{id.Value}'!");
                        }
                    }
                }
                else
                {
                    foreach (SyntaxNode? child in node.Children)
                    {
                        if (!TryValidateVarDefScope(child, currentScopeID, definedVarInfo))
                        {
                            hasThisFailed = true;
                        }
                    }
                }
                return !hasThisFailed;
            }
            bool TryValidateAssignmentOrFuncCallScope(IHasChildren node, uint currentScopeID, int statementPosition)
            {
                bool hasThisFailed = false;
                if (node is IdentifierLeaf id)
                {
                    if (!symbolTable.TryGetSymbolInfo(currentScopeID, id.Value, out SymbolInfo? symInfo))
                    {
                        hasThisFailed = true;
                        hasFailed = true;
                        messageSB.AppendLine($"\t[{id.StartLine}.{id.StartChar}-{id.EndLine}.{id.EndChar}] The identifier '{id.Value}' does not exist in the current context!");
                    }
                    else if (symInfo.EnclosingScope.IsLocal && symInfo.SymbolPosition >= statementPosition)
                    {
                        hasThisFailed = true;
                        hasFailed = true;
                        messageSB.AppendLine($"\t[{id.StartLine}.{id.StartChar}-{id.EndLine}.{id.EndChar}] Cannot use identifier '{id.Value}' before is declared!");
                    }
                }
                else
                {
                    foreach (SyntaxNode? child in node.Children)
                    {
                        if (!TryValidateAssignmentOrFuncCallScope(child, currentScopeID, statementPosition))
                        {
                            hasThisFailed = true;
                        }
                    }
                }
                return !hasThisFailed;
            }
            void ValidateVarDefType(VariableDefinitionNode node, uint currentScopeID, StringBuilder messageSB)
            {
                string assignmentRHS = ResolveType(node.AssignedValue, currentScopeID, messageSB);

                if (node.NameTypeNode.Type != assignmentRHS)
                {
                    messageSB.AppendLine($"\t[{node.StartLine}.{node.StartChar}-{node.EndLine}.{node.EndChar}] "
                        + $"Cannot assign type of '{assignmentRHS}' to variable {node.NameTypeNode.Name} of type '{node.NameTypeNode.Type}'");
                }
            }
            void ValidateAssignment(AssignmentStatementNode node, uint currentScopeID, StringBuilder messageSB)
            {
                if (!symbolTable.TryGetSymbolInfo(currentScopeID, node.Identifier.Value, out SymbolInfo? info))
                {
                    hasFailed = true;
                    messageSB.AppendLine($"\t[{node.StartLine}.{node.StartChar}-{node.EndLine}.{node.EndChar}] "
                        + $"Cannot assign to undefined identifier '{node.Identifier.Value}'");
                    return;
                }

                string assignedType = ResolveType(node.AssignedValue, currentScopeID, messageSB);

                if (info.Type != assignedType)
                {
                    hasFailed = true;
                    messageSB.AppendLine($"\t[{node.StartLine}.{node.StartChar}-{node.EndLine}.{node.EndChar}] "
                        + $"Cannot assign type of '{assignedType}' to variable {info.Name} of type '{info.Type}'");
                }
            }
            void ValidateFunctionCall(FunctionCallStatementNode node, uint currentScopeID, StringBuilder messageSB)
            {
                if (!symbolTable.TryGetSymbolInfo(currentScopeID, node.FunctionCallExpression.Identifier.Value, out SymbolInfo? info))
                {
                    hasFailed = true;
                    messageSB.AppendLine($"\t[{node.StartLine}.{node.StartChar}-{node.EndLine}.{node.EndChar}] "
                        + $"Function '{node.FunctionCallExpression.Identifier.Value}' is not defined in the current context!");
                    return;
                }
            }
        }

        string ResolveType(IHasChildren node, uint currentScopeID, StringBuilder messageSB)
        {
            if (node is ValueOperationNode)
            {
                if (node is UnaryOperationNode)
                {
                    if (node is NotOperationNode notOp)
                    {
                        string type = ResolveType(notOp.Operand, currentScopeID, messageSB);

                        if (type != PrimitiveTypeNames.Bool)
                        {
                            messageSB.AppendLine($"\t[{notOp.StartLine}.{notOp.StartChar}-{notOp.EndLine}.{notOp.EndChar}] "
                                + $"Operator '!' cannot be applied to operand of type {type}");
                        }
                        return type;
                    }
                    throw new NotImplementedException("Unary operation not implemented: " + node.GetType().Name);
                }
                else if (node is BinaryOperationNode)
                {
                    if (node is HighPrecedenceOperationNode highOp)
                    {
                        string lhsType = ResolveType(highOp.LeftOperand, currentScopeID, messageSB);
                        string rhsType = ResolveType(highOp.RightOperand, currentScopeID, messageSB);

                        if (lhsType is not (PrimitiveTypeNames.Int8 or PrimitiveTypeNames.Int16 or PrimitiveTypeNames.Int32 or PrimitiveTypeNames.Int64)
                        || rhsType is not (PrimitiveTypeNames.Int8 or PrimitiveTypeNames.Int16 or PrimitiveTypeNames.Int32 or PrimitiveTypeNames.Int64))
                        {
                            messageSB.AppendLine($"\t[{highOp.StartLine}.{highOp.StartChar}-{highOp.EndLine}.{highOp.EndChar}] "
                                + $"Operator '{highOp.Operator}' cannot be applied to operands of type {lhsType} and {rhsType}");
                        }
                        return lhsType;
                    }
                    else if (node is LowPrecedenceOperationNode lowOp)
                    {
                        if (lowOp is AddOperationNode or SubtractOperationNode)
                        {
                            string lhsType = ResolveType(lowOp.LeftOperand, currentScopeID, messageSB);
                            string rhsType = ResolveType(lowOp.RightOperand, currentScopeID, messageSB);

                            if (lhsType is not (PrimitiveTypeNames.Int8 or PrimitiveTypeNames.Int16 or PrimitiveTypeNames.Int32 or PrimitiveTypeNames.Int64)
                            || rhsType is not (PrimitiveTypeNames.Int8 or PrimitiveTypeNames.Int16 or PrimitiveTypeNames.Int32 or PrimitiveTypeNames.Int64))
                            {
                                messageSB.AppendLine($"\t[{lowOp.StartLine}.{lowOp.StartChar}-{lowOp.EndLine}.{lowOp.EndChar}] "
                                    + $"Operator '{lowOp.Operator}' cannot be applied to operands of type {lhsType} and {rhsType}");
                            }
                            return lhsType;
                        }
                        else if (lowOp is OrOperationNode or AndOperationNode)
                        {
                            string lhsType = ResolveType(lowOp.LeftOperand, currentScopeID, messageSB);
                            string rhsType = ResolveType(lowOp.RightOperand, currentScopeID, messageSB);

                            if (lhsType != PrimitiveTypeNames.Bool || rhsType != PrimitiveTypeNames.Bool)
                            {
                                messageSB.AppendLine($"\t[{lowOp.StartLine}.{lowOp.StartChar}-{lowOp.EndLine}.{lowOp.EndChar}] "
                                    + $"Operator '{lowOp.Operator}' cannot be applied to operands of type {lhsType} and {rhsType}");
                            }
                            return lhsType;
                        }
                        else if (lowOp is EqualityOperationNode)
                        {
                            ResolveType(lowOp.LeftOperand, currentScopeID, messageSB);
                            ResolveType(lowOp.RightOperand, currentScopeID, messageSB);
                            return PrimitiveTypeNames.Bool;
                        }
                        throw new NotImplementedException("Low precedence binary operation not implemented: " + node.GetType().Name);
                    }
                    throw new NotImplementedException("Binary operation not implemented: " + node.GetType().Name);
                }
                throw new NotImplementedException("Non-Unary-Or-Binary operation not implemented: " + node.GetType().Name);
            }
            else if (node is IdentifierLeaf identifier)
            {
                if (symbolTable.TryGetSymbolInfo(currentScopeID, identifier.Value, out SymbolInfo? info)) return info.Type;

                throw new InvalidOperationException($"Scope check failed! Encountered undefined identifier: {identifier}");
            }
            else if (node is LiteralLeaf)
            {
                if (node is IntLiteralLeaf) return PrimitiveTypeNames.Int32;

                if (node is BoolLiteralLeaf) return PrimitiveTypeNames.Bool;

                throw new NotImplementedException("Node type not supported for type resolution: " + node.GetType().Name);
            }
            else if (node is FunctionCallExpressionNode funcCallExpr)
            {
                if (!symbolTable.TryGetSymbolInfo(currentScopeID, funcCallExpr.Identifier.Value, out SymbolInfo? info))
                    throw new InvalidOperationException($"Scope check pass failed! Encountered undefined function identifier: {funcCallExpr.Identifier.Value}");

                return info.Type;
            }
            else throw new NotImplementedException("Node type not supported for type resolution: " + node.GetType().Name);
        }

        public string GetPrintable() => symbolTable.GetPrintable();
    }
}