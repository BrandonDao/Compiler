namespace CompilerLib.Parser.Nodes.Scopes
{
    public class NamespaceDefinitionNode(NamespaceKeywordLeaf namespaceKeyword, QualifiedNameNode name, BlockNode block)
        : SyntaxNode([namespaceKeyword, name, block]), IContainsScopeNode
    {
        public string Name => name.Name;
        public BlockNode Block => block;
        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(0); // Remove the namespace keyword
            Children[0] = Children[0].ToAST(); // Convert the qualified name to AST
            Children[1] = Children[1].ToAST(); // Convert the block to AST
            return this;
        }
    }
}