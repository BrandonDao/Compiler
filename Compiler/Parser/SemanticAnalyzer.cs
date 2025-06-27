using System.Text;
using CompilerLib.Parser.Nodes;
using static Compiler.Parser.RecursiveDescentParser;
using static Compiler.Parser.SymbolTable;

namespace Compiler.Parser
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
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypes.Int8, PrimitiveTypes.Int8);
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypes.Int16, PrimitiveTypes.Int16);
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypes.Int32, PrimitiveTypes.Int32);
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypes.Int64, PrimitiveTypes.Int64);
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, PrimitiveTypes.Bool, PrimitiveTypes.Bool);

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

                        string name = scopeContainer.Name;
                        scopeContainer.Block.ID = scopeID;
                        symbolTable.AddScope(scopeID, name, isLocal: true, currentScopeID);
                        symbolPosition = BuildSymbolTable(scopeContainer.Block, scopeID++, symbolPosition, isLocal: true);
                    }
                    else if (child is VariableDefinitionNode varDefNode)
                    {
                        if (symbolTable.TryGetSymbolInfo(currentScopeID, varDefNode.NameTypeNode.Name, out var symbolInfo))
                        {
                            if (!isLocal || (isLocal && symbolInfo.EnclosingScope.IsLocal))
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
                    var isScopeContainerLocal = scopeContainer is FunctionDefinitionNode || isLocal;

                    string name = scopeContainer.Name;
                    scopeContainer.Block.ID = scopeID;
                    symbolTable.AddScope(scopeID, name, isScopeContainerLocal, currentScopeID);
                    BuildSymbolTable(scopeContainer.Block, scopeID++, symbolPosition: 0, isScopeContainerLocal);
                }
                return symbolPosition;
            }
        }
        private bool TryValidateScopes(SyntaxNode rootNode, StringBuilder messageSB)
        {
            bool hasFailed = false;
            messageSB.AppendLine("Scope validation failed:");

            ValidateScopesAndTypes(rootNode, 0);
            if (hasFailed)
            {
                return false;
            }

            messageSB.AppendLine("\tScope validation completed successfully.");
            return true;

            void ValidateScopesAndTypes(IHasChildren node, uint currentScopeID)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    SyntaxNode? child = node.Children[i];

                    if (child is VariableDefinitionNode varDefNode)
                    {
                        IdentifierLeaf varID = varDefNode.NameTypeNode.Identifier;

                        if (!symbolTable.TryGetSymbolInfo(currentScopeID, name: varID.Value, out SymbolInfo? nameInfo))
                            throw new InvalidOperationException($"Symbol table failed to build variable definitions correctly!"
                                + "Could not find symbol: ({varID})");

                        if (!symbolTable.TryGetSymbolInfo(currentScopeID, varDefNode.NameTypeNode.Type, out SymbolInfo? typeInfo))
                        {
                            hasFailed = true;
                            messageSB.AppendLine($"\t[{varDefNode.StartLine}.{varDefNode.StartChar}-{varDefNode.EndLine}.{varDefNode.EndChar}] "
                                + $"The type '{varDefNode.NameTypeNode.Type}' is not defined in the current context!");
                        }

                        if (TryValidateVarDefScope(varDefNode.AssignedValue, currentScopeID, nameInfo))
                        {
                            ValidateVarDefType(varDefNode, currentScopeID); // Will not validate type if scope check fails
                        }

                    }
                    else if (child is IContainsScopeNode scopeContainer)
                    {
                        uint scopeID = scopeContainer.Block.ID;
                        ValidateScopesAndTypes(scopeContainer.Block, scopeID);
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
                        TryValidateVarDefScope(child, currentScopeID, definedVarInfo);
                    }
                }
                return !hasThisFailed;
            }
            void ValidateVarDefType(VariableDefinitionNode node, uint currentScopeID)
            {
                string assignmentRHS = ResolveType(node.AssignedValue);

                if (node.NameTypeNode.Type != assignmentRHS)
                {
                    messageSB.AppendLine($"\t[{node.StartLine}.{node.StartChar}-{node.EndLine}.{node.EndChar}] "
                        + $"Cannot assign type of '{assignmentRHS}' to variable {node.NameTypeNode.Name} of type '{node.NameTypeNode.Type}'");
                }

                string ResolveType(IHasChildren node)
                {
                    if (node is ValueOperationNode)
                    {
                        if (node is UnaryOperationNode)
                        {
                            if (node is NotOperationNode notOp)
                            {
                                string type = ResolveType(notOp.Operand);

                                if (type != PrimitiveTypes.Bool)
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
                                string lhsType = ResolveType(highOp.LeftOperand);
                                string rhsType = ResolveType(highOp.RightOperand);

                                if (lhsType is not (PrimitiveTypes.Int8 or PrimitiveTypes.Int16 or PrimitiveTypes.Int32 or PrimitiveTypes.Int64)
                                || rhsType is not (PrimitiveTypes.Int8 or PrimitiveTypes.Int16 or PrimitiveTypes.Int32 or PrimitiveTypes.Int64))
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
                                    string lhsType = ResolveType(lowOp.LeftOperand);
                                    string rhsType = ResolveType(lowOp.RightOperand);

                                    if (lhsType is not (PrimitiveTypes.Int8 or PrimitiveTypes.Int16 or PrimitiveTypes.Int32 or PrimitiveTypes.Int64)
                                    || rhsType is not (PrimitiveTypes.Int8 or PrimitiveTypes.Int16 or PrimitiveTypes.Int32 or PrimitiveTypes.Int64))
                                    {
                                        messageSB.AppendLine($"\t[{lowOp.StartLine}.{lowOp.StartChar}-{lowOp.EndLine}.{lowOp.EndChar}] "
                                            + $"Operator '{lowOp.Operator}' cannot be applied to operands of type {lhsType} and {rhsType}");
                                    }
                                    return lhsType;
                                }
                                else if (lowOp is OrOperationNode or AndOperationNode)
                                {
                                    string lhsType = ResolveType(lowOp.LeftOperand);
                                    string rhsType = ResolveType(lowOp.RightOperand);

                                    if (lhsType != PrimitiveTypes.Bool || rhsType != PrimitiveTypes.Bool)
                                    {
                                        messageSB.AppendLine($"\t[{lowOp.StartLine}.{lowOp.StartChar}-{lowOp.EndLine}.{lowOp.EndChar}] "
                                            + $"Operator '{lowOp.Operator}' cannot be applied to operands of type {lhsType} and {rhsType}");
                                    }
                                    return lhsType;
                                }
                                else if (lowOp is EqualityOperationNode)
                                {
                                    ResolveType(lowOp.LeftOperand);
                                    ResolveType(lowOp.RightOperand);
                                    return PrimitiveTypes.Bool;
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
                        if (node is IntLiteralLeaf) return PrimitiveTypes.Int32;

                        if (node is BoolLiteralLeaf) return PrimitiveTypes.Bool;

                        throw new NotImplementedException("Node type not supported for type resolution: " + node.GetType().Name);
                    }
                    else throw new NotImplementedException("Node type not supported for type resolution: " + node.GetType().Name);
                }
            }
        }

        public string GetPrintable() => symbolTable.GetPrintable();
    }
}