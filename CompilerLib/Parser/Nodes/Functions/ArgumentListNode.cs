using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes.Functions
{
    public class ArgumentListNode : SyntaxNode
    {
        private static List<SyntaxNode> EmptyArgs { get; } = [];
        public ArgumentListNode(OpenParenthesisLeaf openParen, List<SyntaxNode> arguments, CloseParenthesisLeaf closeParen)
            : base([openParen, .. arguments, closeParen])
        {
            UpdateRange();
        }
        public ArgumentListNode(OpenParenthesisLeaf openParen, CloseParenthesisLeaf closeParen)
            : this(openParen, EmptyArgs, closeParen) { }

        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(Children.Count - 1); // Remove the close parenthesis
            Children.RemoveAt(0); // Remove the open parenthesis
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is CommaLeaf)
                {
                    Children.RemoveAt(i);
                    i--;
                    continue;
                }
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
}
