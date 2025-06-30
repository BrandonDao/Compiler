using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes.Functions
{
    public class ParameterListNode : SyntaxNode
    {
        private static List<SyntaxNode> EmptyParams { get; } = [];

        public ParameterListNode(OpenParenthesisLeaf openParen, List<SyntaxNode> parameters, CloseParenthesisLeaf closeParen)
            : base([openParen, .. parameters, closeParen])
            => UpdateRange();
        public ParameterListNode(OpenParenthesisLeaf openParen, CloseParenthesisLeaf closeParen)
            : this(openParen, EmptyParams, closeParen) { }

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