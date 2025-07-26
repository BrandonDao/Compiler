using CompilerLib.Nodes.Punctuation;

namespace CompilerLib.Nodes.Statements
{
    public class EmptyStatementNode : SyntaxNode
    {
        public EmptyStatementNode(SemicolonLeaf semicolon) : base([semicolon])
            => UpdateRange();

        public override SyntaxNode ToAST()
        {
            Children.Clear();
            return this;
        }
    }
}