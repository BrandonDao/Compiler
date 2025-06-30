using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes.Statements
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