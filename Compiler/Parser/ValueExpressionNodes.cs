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


        public abstract class IntExpressionNode : ValueExpressionNode
        {
            public IntExpressionNode() { }
            public IntExpressionNode(List<SyntaxNode> children) : base(children) { }
        }
        public class MultiplyExpressionNode(List<SyntaxNode> children) : IntExpressionNode(children);
        public class DivideExpressionNode(List<SyntaxNode> children) : IntExpressionNode(children);
        public class ModExpressionNode(List<SyntaxNode> children) : IntExpressionNode(children);
        public class AddExpressionNode(List<SyntaxNode> children) : IntExpressionNode(children);
        public class SubtractExpressionNode(List<SyntaxNode> children) : IntExpressionNode(children);



        public abstract class BoolExpressionNode : ValueExpressionNode
        {
            public BoolExpressionNode() { }
            public BoolExpressionNode(List<SyntaxNode> children) : base(children) { }
        }
        public class NotExpressionNode(List<SyntaxNode> children) : BoolExpressionNode(children);
        public class OrExpressionNode(List<SyntaxNode> children) : BoolExpressionNode(children);
        public class AndExpressionNode(List<SyntaxNode> children) : BoolExpressionNode(children);
        public class EqualityExpressionNode(List<SyntaxNode> children) : BoolExpressionNode(children);

    }
}
