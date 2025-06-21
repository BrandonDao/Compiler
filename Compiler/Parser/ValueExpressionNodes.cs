using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Punctuation;

namespace Compiler.Parser
{
    public partial class RecursiveDescentParser
    {
        public abstract class ValueExpressionNode : SyntaxNode
        {
            public ValueExpressionNode() { }
            public ValueExpressionNode(List<SyntaxNode> children) : base(children) { }
        }
        public class ParenthesizedExpression(OpenParenthesisLeaf open, SyntaxNode expr, CloseParenthesisLeaf close)
            : ValueExpressionNode([open, expr, close]);


        public class MultiplyExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);
        public class DivideExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);
        public class ModExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);
        public class AddExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);
        public class SubtractExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);


        public class NotExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);
        public class OrExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);
        public class AndExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);
        public class EqualityExpressionNode(List<SyntaxNode> children) : ValueExpressionNode(children);

    }
}
