using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Operators;

namespace Compiler.Parser
{
    public partial class RecursiveDescentParser
    {
        public abstract class IntExpressionNode : SyntaxNode
        {
            public IntExpressionNode() { }
            public IntExpressionNode(List<SyntaxNode> children) : base(children) { }

            public static SyntaxNode? ParseMulDiv(List<LeafNode> tokens, ref int position)
            {
                var lhsTerm = ParseIntTerm(ref position);
                if (lhsTerm is null) return null;

                var rest = ParseMulDivRest(ref position);
                if (rest is null) return null;

                if (rest is Epsilon) return lhsTerm;

                rest.Children[0] = lhsTerm;
                rest.UpdateRange();
                return rest;

                SyntaxNode? ParseMulDivRest(ref int position)
                {
                    if (position >= tokens.Count) return new Epsilon();

                    int start = position;

                    var op = ParseOp(ref position);
                    if (op is null) return new Epsilon();

                    var rest = ParseMulDiv(tokens, ref position);
                    if (rest is null)
                    {
                        position = start;
                        return null;
                    }

                    op.Children[^1] = rest;
                    return op;

                    SyntaxNode? ParseOp(ref int position)
                    {
                        if (tokens[position] is MultiplyOperatorLeaf) return new MultiplyExpressionNode(children: [null, tokens[position++], null]);
                        else if (tokens[position] is DivideOperatorLeaf) return new DivideExpressionNode(children: [null, tokens[position++], null]);
                        else return null;
                    }
                }
                SyntaxNode? ParseIntTerm(ref int position)
                {
                    if (tokens[position] is IdentifierLeaf idToken)
                    {
                        position++;
                        return new IdentifierNode(idToken);
                    }
                    else if (tokens[position] is IntLiteralLeaf litToken)
                    {
                        position++;
                        return new IntLiteralNode(litToken);
                    }
                    else return null;
                }
            }
        }

        public class MultiplyExpressionNode : IntExpressionNode
        {
            public MultiplyExpressionNode() { }
            public MultiplyExpressionNode(List<SyntaxNode> children) : base(children) { }
        }
        public class DivideExpressionNode : IntExpressionNode
        {
            public DivideExpressionNode() { }
            public DivideExpressionNode(List<SyntaxNode> children) : base(children) { }
        }
        public class AddExpressionNode : IntExpressionNode;
        public class SubtractExpressionNode : IntExpressionNode;

    }
}
