using System.Text;

namespace CompilerLib.Parser.Nodes.Scopes
{
    public class NamespaceDefinitionNode(NamespaceKeywordLeaf namespaceKeyword, QualifiedNameNode name, NonLocalBlockNode block)
        : SyntaxNode([namespaceKeyword, name, block]),
        IContainsScopeNode, IGeneratesCode
    {
        public string Name => name.Name;
        public BlockNode Block => block;

        public List<IGeneratesCode> CodeGenChildren => [block];

        public void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel)
        {
            codeBuilder.AppendIndentedLine($"//// Namespace '{Name}' Visited ----------------\n", indentLevel);
            block.GenerateILCode(ilGen, symbolTable, codeBuilder, indentLevel);
        }

        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(0); // Remove the namespace keyword
            Children[0] = Children[0].ToAST(); // Convert the qualified name to AST
            Children[1] = Children[1].ToAST(); // Convert the block to AST
            return this;
        }
    }
}