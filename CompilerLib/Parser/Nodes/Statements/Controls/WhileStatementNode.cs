using CompilerLib.Parser.Nodes.Scopes;

namespace CompilerLib.Parser.Nodes.Statements.Controls
{
        public class WhileStatementNode : SyntaxNode, IContainsScopeNode
        {
            public string Name => "While Loop";
            public SyntaxNode Condition => Children[0];
            public BlockNode Block { get; }
            public WhileStatementNode(WhileKeywordLeaf whileKeyword, SyntaxNode condition, BlockNode body)
                : base([whileKeyword, condition, body])
            {
                Block = body;
                UpdateRange();
            }

            public override SyntaxNode ToAST()
            {
                Children.RemoveAt(0); // Remove the while keyword
                Children[0] = Children[0].ToAST(); // Convert the condition to AST
                Children[1] = Children[1].ToAST(); // Convert the body
                return this;
            }
    }
}