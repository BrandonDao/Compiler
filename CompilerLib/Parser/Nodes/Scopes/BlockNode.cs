using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes.Scopes
{
    public class BlockNode : SyntaxNode, IContainsScopeNode
    {
        public string Name => "Anonymous Block";
        public BlockNode Block => this;
        public uint ID { get; set; }

        public BlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
            : base([openBrace, .. statements, closeBrace])
            => UpdateRange();

        public override SyntaxNode ToAST()
        {
            if (Children.Count == 3 && Children[1] is BlockNode blockNode)
            {
                return blockNode.ToAST();
            }

            Children.RemoveAt(Children.Count - 1); // Remove the close brace
            Children.RemoveAt(0); // Remove the open brace
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
}