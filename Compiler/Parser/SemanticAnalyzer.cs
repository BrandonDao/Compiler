using System.Text;
using CompilerLib.Parser.Nodes;
using static Compiler.Parser.RecursiveDescentParser;

namespace Compiler.Parser
{
    public class SemanticAnalyzer
    {
        SymbolTable symbolTable = new();

        public bool TryBuildSymbolTable(SyntaxNode rootNode, out string completionMessage)
        {
            uint scopeID = 1;
            bool hasFailed = false;
            StringBuilder errorSB = new("Symbol table build failed:\n");

            BuildSymbolTable(rootNode, 0);

            if (hasFailed)
            {
                completionMessage = errorSB.ToString();
                return false;
            }
            completionMessage = $"Symbol table built successfully.";
            return true;

            void BuildSymbolTable(SyntaxNode node, uint currentScopeID)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    SyntaxNode? child = node.Children[i];

                    BlockNode block;
                    string name;

                    if (child is IContainsScopeNode scopeContainer)
                    {
                        scopeContainer.Block.ID = scopeID++;
                        block = scopeContainer.Block;
                        name = scopeContainer.Name;
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

                    symbolTable.AddScope(block.ID, name, currentScopeID);
                    BuildSymbolTable(block, block.ID);
                }
            }
        }

        public string GetPrintable() => symbolTable.GetPrintable();
    }
}