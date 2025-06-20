
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Identifiers;
using CompilerLib.Parser.Nodes.Literals;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Punctuation;

namespace Compiler.Parser
{
    public class RecursiveDescentParser : IParser
    {
        public static RecursiveDescentParser Instance { get; private set; } = new();

        private RecursiveDescentParser() { }

        public SyntaxNode? ParseTokens(List<LeafNode> tokens)
        {
            if (tokens.Count == 0) return null;

            int position = 0;
            tokens = HangWhitespace(tokens);
            return ParseMulDivExpression(tokens, ref position);
        }

        public SyntaxNode ToAST(SyntaxNode root) => throw new NotImplementedException();

        private static List<LeafNode> HangWhitespace(List<LeafNode> tokens)
        {
            List<LeafNode> trimmedTokens = new(capacity: tokens.Count);

            if (tokens[0] is Whitespace whitespaceToken)
            {
                whitespaceToken.IsLeading = true;
                tokens[1].Children.Add(tokens[0]);
            }

            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] is not Whitespace)
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
            int start = position;
        }

        private static SyntaxNode? ParseMulDivExpression(List<LeafNode> tokens, ref int position)
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

                var rest = ParseMulDivExpression(tokens, ref position);
                if (rest is null)
                {
                    position = start;
                    return null;
                }

                op.Children[^1] = rest;
                return op;

                SyntaxNode? ParseOp(ref int position)
                {
                    if (tokens[position] is MultiplyOperator) return new MultiplyExpressionNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is DivideOperator) return new DivideExpressionNode(children: [null, tokens[position++], null]);
                    else return null;
                }
            }
            SyntaxNode? ParseIntTerm(ref int position)
            {
                if (tokens[position] is IdentifierToken idToken)
                {
                    position++;
                    return new IdentifierNode(idToken);
                }
                else if (tokens[position] is IntLiteralToken litToken)
                {
                    position++;
                    return new IntLiteralNode(litToken);
                }
                else return null;
            }
        }

        public class Program() : SyntaxNode;
        public class VariableDefinition() : SyntaxNode;
        public class EqualsValue() : SyntaxNode;
        public class IntExpression() : SyntaxNode;

        public abstract class IntExpressionNode : SyntaxNode
        {
            public IntExpressionNode() { }
            public IntExpressionNode(List<SyntaxNode> children) : base(children) { }
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

        public class IntLiteralNode : SyntaxNode
        {
            public IntLiteralNode(IntLiteralToken token) : base(children: [token])
                => UpdateRange();
        }

        public class IdentifierNode : SyntaxNode
        {
            public IdentifierNode(IdentifierToken token) : base(children: [token])
                => UpdateRange();
        }

        public class Collapsible() : SyntaxNode;
        public class Epsilon() : Collapsible;
    }
}
