using CompilerLib.Parser.Nodes;
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
            return ParseVariableDefinition(tokens, ref position);
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

                trimmedTokens[^1].Children.Add(tokens[i]);
            }

            return trimmedTokens;
        }



        private static VariableDefinitionNode ParseVariableDefinition(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not LetKeywordLeaf letKeywordLeaf)
                throw new ArgumentException($"Expected 'let' keyword at start of variable definition, not {tokens[position]}!");

            position++;
            var varNameTypeNode = ParseVariableNameType(tokens, ref position);

            if (tokens[position] is not AssignmentOperatorLeaf assignmentOpLeaf)
                throw new ArgumentException($"Expected '=' token after variable name and type, not {tokens[position]}!");

            position++;
            var valueExpr = ParseValueExpression(tokens, ref position);

            if (tokens[position] is not SemicolonLeaf semicolonLeaf)
                throw new ArgumentException($"Expected ';' token at the end of variable definition, not {tokens[position]}!");

            return new VariableDefinitionNode(letKeywordLeaf, varNameTypeNode, assignmentOpLeaf, valueExpr, semicolonLeaf);
        }

        private static TypeNode ParseType(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is IdentifierLeaf idToken)
            {
                position++;
                return new IdentifierNode(idToken);
            }
            if(tokens[position] is Int8Leaf int8Token)
            {
                position++;
                return new Int8Node(int8Token);
            }
            if (tokens[position] is Int16Leaf int16Token)
            {
                position++;
                return new Int16Node(int16Token);
            }
            if (tokens[position] is Int32Leaf int32Token)
            {
                position++;
                return new Int32Node(int32Token);
            }
            if (tokens[position] is Int64Leaf int64Token)
            {
                position++;
                return new Int64Node(int64Token);
            }
            if (tokens[position] is BoolLeaf boolToken)
            {
                position++;
                return new BoolNode(boolToken);
            }

            throw new ArgumentException($"Expected an identifier or primitive type!");
        }
        
        private static VariableNameTypeNode ParseVariableNameType(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not IdentifierLeaf idToken)
                throw new ArgumentException($"Expected an identifier as variable name, not {tokens[position]}!");

            position++;

            if (tokens[position] is not ColonLeaf colonToken)
                throw new ArgumentException($"Expected a ':' token after variable name '{idToken.Value}', not {tokens[position]}!");

            position++;

            var typeNode = ParseType(tokens, ref position)
                ?? throw new ArgumentException($"Expected a type after ':' token, not {tokens[position]}!");

            return new VariableNameTypeNode(new IdentifierNode(idToken), colonToken, typeNode);
        }

        private static SyntaxNode ParseValueExpression(List<LeafNode> tokens, ref int position)
        {
            var highExpr = ParseHighValueExpr(ref position);
            var rest = ParseLowValueExprRest(ref position);
            if (rest is EpsilonNode) return highExpr;

            rest.Children[0] = highExpr;
            rest.UpdateRange();
            return rest;

            SyntaxNode ParseHighValueExpr(ref int position)
            {
                var lhs = ParseValueTerm(ref position);
                var rest = ParseHighValueExprRest(ref position);
                if (rest is EpsilonNode) return lhs;

                rest.Children[0] = lhs;
                rest.UpdateRange();
                return rest;

                SyntaxNode ParseHighValueExprRest(ref int position)
                {
                    if (position >= tokens.Count) return EpsilonNode.Instance;

                    var op = ParseHighOp(ref position);
                    if (op is null) return EpsilonNode.Instance;

                    var rhs = ParseHighValueExpr(ref position)
                        ?? throw new ArgumentException($"Could not parse the expression after the operator {op.Children[1]}!");

                    op.Children[^1] = rhs;
                    return op;

                    SyntaxNode? ParseHighOp(ref int position)
                    {
                        if (tokens[position] is MultiplyOperatorLeaf) return new MultiplyOperationNode(children: [null, tokens[position++], null]);
                        else if (tokens[position] is DivideOperatorLeaf) return new DivideOperationNode(children: [null, tokens[position++], null]);
                        else return null;
                    }
                }
            }
            SyntaxNode ParseLowValueExprRest(ref int position)
            {
                if (position >= tokens.Count) return EpsilonNode.Instance;

                int start = position;

                var op = ParseLowOp(ref position);
                if (op is null) return EpsilonNode.Instance;

                var rhs = ParseValueExpression(tokens, ref position);
                if (rhs is null)
                {
                    position = start;
                    return EpsilonNode.Instance;
                }

                op.Children[^1] = rhs;
                return op;

                SyntaxNode? ParseLowOp(ref int position)
                {
                    if (tokens[position] is AddOperatorLeaf) return new AddOperationNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is NegateOperatorLeaf) return new SubtractOperationNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is OrOperatorLeaf) return new OrOperationNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is AndOperatorLeaf) return new AndOperationNode(children: [null, tokens[position++], null]);
                    else if (tokens[position] is EqualityOperatorLeaf) return new EqualityOperationNode(children: [null, tokens[position++], null]);
                    else return null;
                }
            }
            SyntaxNode ParseValueTerm(ref int position)
            {
                switch (tokens[position])
                {
                    case NotOperatorLeaf notToken:
                        {
                            position++;
                            var notValueNode = ParseValueTerm(ref position)
                                ?? throw new ArgumentException("Could not parse the expression after the '!' token!");

                            var notOpNode = new NotOperationNode([notToken, null]);
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

                    default: throw new ArgumentException($"Could not parse Value Term, found {tokens[position]}!");
                }
            }
        }



        public class ProgramNode : SyntaxNode;
        public class VariableDefinitionNode : SyntaxNode
        {
            public VariableDefinitionNode(LetKeywordLeaf let, VariableNameTypeNode nameType, AssignmentOperatorLeaf equals, SyntaxNode value, SemicolonLeaf semicolon)
                : base([let, nameType, equals, value, semicolon])
                => UpdateRange();
        }
        public class VariableNameTypeNode : SyntaxNode
        {
            public VariableNameTypeNode(IdentifierNode id, ColonLeaf colon, TypeNode type) : base([id, colon, type])
                => UpdateRange();
        }

        public abstract class CollapsibleNode : SyntaxNode;
        public class EpsilonNode : CollapsibleNode
        {
            public static EpsilonNode Instance { get; } = new();
            private EpsilonNode() { }
        }
    }
}
