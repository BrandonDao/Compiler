using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes.Functions
{
    public class FunctionCallStatementNode : SyntaxNode
    {
        public FunctionCallExpressionNode FunctionCallExpression { get; }
        public FunctionCallStatementNode(FunctionCallExpressionNode funcCallExpr, SemicolonLeaf semicolon)
            : base([funcCallExpr, semicolon])
        {
            FunctionCallExpression = funcCallExpr;
            UpdateRange();
        }
        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(1); // Remove the semicolon
            Children[0] = Children[0].ToAST(); // Convert the function call expression to AST
            return this;
        }
    }
}