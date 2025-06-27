using System.Runtime.CompilerServices;
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
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, "int8", "int8");
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, "int16", "int16");
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, "int32", "int32");
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, "int64", "int64");
            symbolTable.AddSymbol(scopeID: 0, symbolPosition: -1, "bool", "bool");

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
                                messageSB.AppendLine($"\t[{varDefNode.StartLine}.{varDefNode.StartChar}-{varDefNode.EndLine}.{varDefNode.EndChar}] Variable with name '{varDefNode.NameTypeNode.Name}' already exists in scope!");
                            }
                            else
                            {
                                messageSB.AppendLine($"\t[{varDefNode.StartLine}.{varDefNode.StartChar}-{varDefNode.EndLine}.{varDefNode.EndChar}] WARNING: Variable with name '{varDefNode.NameTypeNode.Name}' is being shadowed!");
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

            ValidateScopes(rootNode, 0);
            if (hasFailed)
            {
                return false;
            }

            messageSB.AppendLine("\tScope validation completed successfully.");
            return true;

            void ValidateScopes(IHasChildren node, uint currentScopeID)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    SyntaxNode? child = node.Children[i];

                    if (child is VariableDefinitionNode varDefNode)
                    {
                        IdentifierLeaf varID = varDefNode.NameTypeNode.Identifier;

                        if (!symbolTable.TryGetSymbolInfo(currentScopeID, varDefNode.NameTypeNode.Type, out SymbolInfo? typeInfo))
                        {
                            hasFailed = true;
                            messageSB.AppendLine($"\t[{varDefNode.StartLine}.{varDefNode.StartChar}-{varDefNode.EndLine}.{varDefNode.EndChar}] The type '{varDefNode.NameTypeNode.Type}' is not defined in the current context!");
                        }

                        if (!symbolTable.TryGetSymbolInfo(currentScopeID, name: varID.Value, out SymbolInfo? nameInfo))
                            throw new InvalidOperationException($"Symbol table failed to build variable definitions correctly! Could not find symbol: ({varID})");

                        ValidateVarDefScope(varDefNode.AssignedValue, currentScopeID, nameInfo);

                    }
                    else if (child is IContainsScopeNode scopeContainer)
                    {
                        uint scopeID = scopeContainer.Block.ID;
                        ValidateScopes(scopeContainer.Block, scopeID);
                    }
                    else throw new NotImplementedException();
                }
            }
            void ValidateVarDefScope(IHasChildren node, uint currentScopeID, SymbolInfo definedVarInfo)
            {
                if (node is IdentifierLeaf id)
                {
                    if (!symbolTable.TryGetSymbolInfo(currentScopeID, id.Value, out SymbolInfo? symInfo))
                    {
                        hasFailed = true;
                        messageSB.AppendLine($"\t[{id.StartLine}.{id.StartChar}-{id.EndLine}.{id.EndChar}] The identifier '{id.Value}' does not exist in the current context!");
                    }
                    else
                    {
                        if (symInfo.EnclosingScope.IsLocal)
                        {
                            if (symInfo.SymbolPosition >= definedVarInfo.SymbolPosition)
                            {
                                hasFailed = true;
                                messageSB.AppendLine($"\t[{id.StartLine}.{id.StartChar}-{id.EndLine}.{id.EndChar}] Cannot use identifier '{id.Value}' before is declared!");
                            }
                        }
                        else if (!definedVarInfo.EnclosingScope.IsLocal)
                        {
                            hasFailed = true;
                            messageSB.AppendLine($"\t[{id.StartLine}.{id.StartChar}-{id.EndLine}.{id.EndChar}] A field definition cannot reference another field '{id.Value}'!");
                        }
                    }
                }
                else
                {
                    foreach (SyntaxNode? child in node.Children)
                    {
                        ValidateVarDefScope(child, currentScopeID, definedVarInfo);
                    }
                }
            }
        }

        public string GetPrintable() => symbolTable.GetPrintable();
    }
}