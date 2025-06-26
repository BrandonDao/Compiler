using System.Linq.Expressions;
using System.Text;
using CompilerLib.Parser.Nodes;
using static Compiler.Parser.RecursiveDescentParser;
using static Compiler.Parser.SymbolTable;

namespace Compiler.Parser
{
    public class SemanticAnalyzer
    {
        readonly SymbolTable symbolTable = new();

        delegate bool AnalysisStage(SyntaxNode rootNode, out string completionMessage);
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
            foreach (var stage in analysisStages)
            {
                if (!stage(rootNode, out completionMessage)) return false;
            }
            completionMessage = "Semantic analysis completed successfully.";
            return true;
        }


        private bool RegisterPrimitives(SyntaxNode _, out string completionMessage)
        {
            int symbolID = -1;
            symbolTable.AddSymbol(scopeID: 0, symbolID--, "int8", "int8");
            symbolTable.AddSymbol(scopeID: 0, symbolID--, "int16", "int16");
            symbolTable.AddSymbol(scopeID: 0, symbolID--, "int32", "int32");
            symbolTable.AddSymbol(scopeID: 0, symbolID--, "int64", "int64");
            symbolTable.AddSymbol(scopeID: 0, symbolID--, "bool", "bool");

            completionMessage = "Primitive types registered successfully.";
            return true;
        }
        private bool TryBuildSymbolTable(SyntaxNode rootNode, out string completionMessage)
        {
            uint scopeID = 1;
            bool hasFailed = false;
            StringBuilder errorSB = new("Symbol table construction failed:\n");

            BuildSymbolTable(rootNode, 0);

            if (hasFailed)
            {
                completionMessage = errorSB.ToString();
                return false;
            }
            completionMessage = $"Symbol table built successfully.";
            return true;

            void BuildSymbolTable(IHasChildren node, uint currentScopeID)
            {
                Queue<IContainsScopeNode> nodesToProcess = new();

                for (int i = 0; i < node.Children.Count; i++)
                {
                    SyntaxNode? child = node.Children[i];

                    if (child is IContainsScopeNode scopeContainer)
                    {
                        nodesToProcess.Enqueue(scopeContainer);
                    }
                    else if (child is VariableDefinitionNode varDefNode)
                    {
                        if (symbolTable.ContainsSymbol(currentScopeID, varDefNode.Name))
                        {
                            hasFailed = true;
                            errorSB.AppendLine($"\t[{varDefNode.StartLine}.{varDefNode.StartChar}-{varDefNode.EndLine}.{varDefNode.EndChar}] Variable with name '{varDefNode.Name}' already exists in scope!");
                        }
                        else
                        {
                            symbolTable.AddSymbol(currentScopeID, i, varDefNode.Name, varDefNode.Type);
                        }
                        continue;
                    }
                    else continue;
                }

                while (nodesToProcess.Count > 0)
                {
                    var scopeContainer = nodesToProcess.Dequeue();

                    string name = scopeContainer.Name;
                    scopeContainer.Block.ID = scopeID;
                    symbolTable.AddScope(scopeID, name, currentScopeID);
                    BuildSymbolTable(scopeContainer.Block, scopeID++);
                }
            }
        }
        private bool TryValidateScopes(SyntaxNode rootNode, out string completionMessage)
        {
            bool hasFailed = false;
            StringBuilder errorSB = new("Scope validation failed:\n");

            ValidateScopes(rootNode, 0);
            if (hasFailed)
            {
                completionMessage = errorSB.ToString();
                return false;
            }

            completionMessage = "Scope validation completed successfully.";
            return true;

            void ValidateScopes(IHasChildren node, uint currentScopeID)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    SyntaxNode? child = node.Children[i];

                    if (child is IdentifierLeaf idNode)
                    {
                        if (!symbolTable.TryGetSymbolInfo(currentScopeID, idNode.Value, out SymbolInfo? info))
                        {
                            hasFailed = true;
                            errorSB.AppendLine($"\t[{idNode.StartLine}.{idNode.StartChar}-{idNode.EndLine}.{idNode.EndChar}] The name '{idNode.Value}' does not exist in the current context!");
                        }
                        else
                        {

                        }
                    }
                    else if (child is IContainsScopeNode scopeContainer)
                    {
                        uint scopeID = scopeContainer.Block.ID;
                        ValidateScopes(scopeContainer.Block, scopeID);
                    }
                    else
                    {
                        ValidateScopes(child, currentScopeID);
                    }
                }
            }
        }

        public string GetPrintable() => symbolTable.GetPrintable();
    }
}