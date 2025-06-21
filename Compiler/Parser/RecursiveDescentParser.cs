using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Punctuation;

namespace Compiler.Parser
{
    public partial class RecursiveDescentParser : IParser
    {
        public static RecursiveDescentParser Instance { get; private set; } = new();

        private RecursiveDescentParser() { }

        public SyntaxNode? ParseTokens(List<LeafNode> tokens)
        {
            if (tokens.Count == 0) return null;

            int position = 0;
            tokens = HangWhitespace(tokens);
            return ParseValueExpression(tokens, ref position);
        }

        public SyntaxNode ToAST(SyntaxNode root) => throw new NotImplementedException();

        private static List<LeafNode> HangWhitespace(List<LeafNode> tokens)
        {
            List<LeafNode> trimmedTokens = new(capacity: tokens.Count);

            if (tokens[0] is WhitespaceLeaf whitespaceToken)
            {
                whitespaceToken.IsLeading = true;
                tokens[1].Children.Add(tokens[0]);
            }
            else
            {
                trimmedTokens.Add(tokens[0]);
            }

            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] is not WhitespaceLeaf)
                {
                    trimmedTokens.Add(tokens[i]);
                    continue;
                }

                tokens[i - 1].Children.Add(tokens[i]);
            }

            return trimmedTokens;
        }

        private Program ParseProgram(List<LeafNode> tokens, ref int position)
        {
            throw new NotImplementedException();
        }

        private VariableDefinition ParseVariableDefinition(List<LeafNode> tokens, ref int position)
        {
            throw new NotImplementedException();
        }
        private EqualsValue ParseEqualsValue(List<LeafNode> tokens, ref int position)
        {
            throw new NotImplementedException();
        }

        static SyntaxNode? ParseValueExpression(List<LeafNode> tokens, ref int position)
        {
            var highExpr = ParseHighValueExpr(ref position);
            if (highExpr is null) return null;

            var rest = ParseLowValueExprRest(ref position)
                ?? throw new NotImplementedException("'ParseLowValueExprRest' failed to parse!");

            if (rest is Epsilon) return highExpr;

            rest.Children[0] = highExpr;
            rest.UpdateRange();
            return rest;

            SyntaxNode? ParseHighValueExpr(ref int position)
            {
                var lhs = ParseValueTerm(ref position);
                if (lhs is null) return null;

                var rest = ParseHighValueExprRest(ref position)
                    ?? throw new NotImplementedException("'ParseHighValueExprRest' failed to parse!");

                if (rest is Epsilon) return lhs;

                rest.Children[0] = lhs;
                rest.UpdateRange();
                return rest;

                SyntaxNode? ParseHighValueExprRest(ref int position)
                {
                    if (position >= tokens.Count) return Epsilon.Instance;

                    var op = ParseHighOp(ref position);
                    if (op is null) return Epsilon.Instance;

                    var rhs = ParseHighValueExpr(ref position)
                        ?? throw new ArgumentException($"Could not parse the expression after the operator {op.Children[1]}!");

                    op.Children[^1] = rhs;
                    return op;

                    SyntaxNode? ParseHighOp(ref int position)
                    {
                        if (tokens[position] is MultiplyOperatorLeaf) return new MultiplyExpressionNode(children: [null, tokens[position++], null]);
                        else if (tokens[position] is DivideOperatorLeaf) return new DivideExpressionNode(children: [null, tokens[position++], null]);
                        else return null;
                    }
                }
            }
            SyntaxNode? ParseLowValueExprRest(ref int position)
            {
                if (position >= tokens.Count) return Epsilon.Instance;

                var op = ParseLowOp(ref position);
                if (op is null) return Epsilon.Instance;

                var rhs = ParseValueExpression(tokens, ref position)
                    ?? throw new ArgumentException($"Could not parse the expression after the operator {op.Children[1]}!");

                op.Children[^1] = rhs;
                return op;

                SyntaxNode? ParseLowOp(ref int position)
                {
                    if (tokens[position] is AddOperatorLeaf) return new AddExpressionNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is NegateOperatorLeaf) return new SubtractExpressionNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is OrOperatorLeaf) return new OrExpressionNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is AndOperatorLeaf) return new AndExpressionNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is EqualityOperatorLeaf) return new EqualityExpressionNode(children: [null, tokens[position++], null]);
                    else return null;
                }
            }
            SyntaxNode? ParseValueTerm(ref int position)
            {
                switch (tokens[position])
                {
                    case NotOperatorLeaf notToken:
                        {
                            position++;
                            var notValueNode = ParseValueTerm(ref position)
                                ?? throw new ArgumentException("Could not parse the expression after the '!' token!");

                            var notOpNode = new NotExpressionNode([notToken, null]);
                            notOpNode.Children[1] = notValueNode;
                            notOpNode.UpdateRange();
                            return notOpNode;
                        }

                    case OpenParenthesisLeaf openToken:
                        {
                            position++;
                            var exprNode = ParseValueExpression(tokens, ref position)
                                ?? throw new ArgumentException("Could not parse the expression after the '(' token!");

                            if (tokens[position++] is not CloseParenthesisLeaf closeToken)
                                throw new ArgumentException("Expected ')' token!");

                            var parenthesizedExpr = new ParenthesizedExpression(openToken, exprNode, closeToken);
                            parenthesizedExpr.UpdateRange();
                            return parenthesizedExpr;
                        }

                    case IdentifierLeaf idToken:
                        position++;
                        return new IdentifierNode(idToken);

                    case IntLiteralLeaf litToken:
                        position++;
                        return new IntLiteralNode(litToken);

                    case BoolLiteralLeaf strLitToken:
                        position++;
                        return new BoolLiteralNode(strLitToken);

                    default:
                        return null;
                }
            }
        }

        public class Program : SyntaxNode;
        public class VariableDefinition : SyntaxNode;
        public class EqualsValue : SyntaxNode;
        public class Collapsible : SyntaxNode;
        public class Epsilon : Collapsible
        {
            public static Epsilon Instance { get; } = new();
            private Epsilon() { }
        }
    }
}
