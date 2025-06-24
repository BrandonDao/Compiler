using System.Runtime.CompilerServices;
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
            return ParseFunctionDefinition(tokens, ref position);
        }

        public SyntaxNode ToAST(SyntaxNode root) => root.ToAST();

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



        private static SyntaxNode ParseType(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is IdentifierLeaf idToken)
            {
                position++;
                return idToken;
            }
            if (tokens[position] is Int8Leaf int8Token)
            {
                position++;
                return int8Token;
            }
            if (tokens[position] is Int16Leaf int16Token)
            {
                position++;
                return int16Token;
            }
            if (tokens[position] is Int32Leaf int32Token)
            {
                position++;
                return int32Token;
            }
            if (tokens[position] is Int64Leaf int64Token)
            {
                position++;
                return int64Token;
            }
            if (tokens[position] is BoolLeaf boolToken)
            {
                position++;
                return boolToken;
            }

            throw new ArgumentException($"Expected an identifier or primitive type, not {tokens[position]}!");
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
            position++;

            return new VariableDefinitionNode(letKeywordLeaf, varNameTypeNode, assignmentOpLeaf, valueExpr, semicolonLeaf);
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

            return new VariableNameTypeNode(idToken, colonToken, typeNode);
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
                        return idToken;

                    case IntLiteralLeaf litToken:
                        position++;
                        return litToken;

                    case BoolLiteralLeaf strLitToken:
                        position++;
                        return strLitToken;

                    default: throw new ArgumentException($"Could not parse Value Term, found {tokens[position]}!");
                }
            }
        }


        private static FunctionDefinitionNode ParseFunctionDefinition(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not FunctionKeywordLeaf funcKeywordLeaf)
                throw new ArgumentException($"Expected 'function' keyword at start of function definition, not {tokens[position]}!");

            position++;
            if (tokens[position] is not IdentifierLeaf idToken)
                throw new ArgumentException($"Expected an identifier as function name, not {tokens[position]}!");

            position++;
            var parameterList = ParseParameterList(tokens, ref position);

            if (tokens[position] is not SmallArrowLeaf arrowToken)
            {
                if (tokens[position] is not OpenBraceLeaf)
                    throw new ArgumentException($"Expected '->' or '{{' token after function parameters, not {tokens[position]}!");

                // Inserting Arrow and Void as return type
                arrowToken = new SmallArrowLeaf(tokens[position].StartLine, tokens[position].StartChar);
                var voidType = new VoidLeaf(tokens[position].StartLine, tokens[position].StartChar);

                var voidReturningBody = ParseStatementBlock(tokens, ref position);
                return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, voidType, voidReturningBody);
            }
            position++;

            SyntaxNode returnType;
            if (tokens[position] is VoidLeaf voidLeaf)
            {
                returnType = voidLeaf;
                position++;
            }
            else
            {
                returnType = ParseType(tokens, ref position);
            }

            var body = ParseStatementBlock(tokens, ref position);

            return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, returnType, body);
        }
        private static ParameterListNode ParseParameterList(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not OpenParenthesisLeaf openParenToken)
                throw new ArgumentException($"Expected '(' token, not {tokens[position]}!");

            position++;

            if (tokens[position] is CloseParenthesisLeaf earlyCloseParenToken)
            {
                position++;
                return new ParameterListNode(openParenToken, earlyCloseParenToken);
            }

            List<SyntaxNode> parameters = [];

            CloseParenthesisLeaf closeParenToken;
            while (true)
            {
                if (tokens[position] is IdentifierLeaf)
                {
                    var nameType = ParseVariableNameType(tokens, ref position);
                    parameters.Add(nameType);
                }
                if (tokens[position] is CommaLeaf comma)
                {
                    position++;
                    parameters.Add(comma);
                }
                else if (tokens[position] is CloseParenthesisLeaf closeParen)
                {
                    closeParenToken = closeParen;
                    position++;
                    break;
                }
                else throw new ArgumentException($"Expected ',' or ')' in parameter list, not {tokens[position]}!");
            }
            return new ParameterListNode(openParenToken, parameters, closeParenToken);
        }

        private static WhileStatementNode ParseWhileStatement(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not WhileKeywordLeaf whileKeywordLeaf)
                throw new ArgumentException($"Expected 'while' keyword at start of while loop, not {tokens[position]}!");

            position++;
            var condition = ParseValueExpression(tokens, ref position);

            var body = ParseStatementBlock(tokens, ref position);

            return new WhileStatementNode(whileKeywordLeaf, condition, body);
        }

        private static StatementBlockNode ParseStatementBlock(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not OpenBraceLeaf openBraceToken)
                throw new ArgumentException($"Expected '{{' token at start of block, not {tokens[position]}!");

            position++;
            List<SyntaxNode> statements = [];

            CloseBraceLeaf closeBraceLeaf;
            while (true)
            {
                if (tokens[position] is LetKeywordLeaf)
                {
                    statements.Add(ParseVariableDefinition(tokens, ref position));
                }
                else if (tokens[position] is WhileKeywordLeaf)
                {
                    statements.Add(ParseWhileStatement(tokens, ref position));
                }
                else if (tokens[position] is OpenBraceLeaf)
                {
                    statements.Add(ParseStatementBlock(tokens, ref position));
                }
                else if (tokens[position] is SemicolonLeaf semicolon)
                {
                    position++;
                    statements.Add(new EmptyStatement(semicolon));
                }
                else if (tokens[position] is CloseBraceLeaf leaf)
                {
                    position++;
                    closeBraceLeaf = leaf;
                    break;
                }
                else
                {
                    throw new ArgumentException($"Unexpected token in block: {tokens[position]}!");
                }
            }
            return new StatementBlockNode(openBraceToken, statements, closeBraceLeaf);
        }



        public class VariableDefinitionNode : SyntaxNode
        {
            public VariableDefinitionNode(LetKeywordLeaf let, VariableNameTypeNode nameType, AssignmentOperatorLeaf equals, SyntaxNode value, SemicolonLeaf semicolon)
                : base([let, nameType, equals, value, semicolon])
                => UpdateRange();

            public override SyntaxNode ToAST()
            {
                Children.RemoveAt(4); // Remove the semicolon
                Children.RemoveAt(2); // Remove the assignment operator
                Children.RemoveAt(0); // Remove the let keyword
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i] = Children[i].ToAST();
                }
                return this;
            }
        }
        public class VariableNameTypeNode : SyntaxNode
        {
            public VariableNameTypeNode(IdentifierLeaf id, ColonLeaf colon, SyntaxNode type) : base([id, colon, type])
                => UpdateRange();

            public override SyntaxNode ToAST()
            {
                Children.RemoveAt(1); // Remove the colon
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i] = Children[i].ToAST();
                }
                return this;
            }
        }

        public class FunctionDefinitionNode : SyntaxNode
        {
            public FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SmallArrowLeaf arrow, SyntaxNode returnType, StatementBlockNode body)
                : base([func, id, parameterList, arrow, returnType, body])
                => UpdateRange();

            public override SyntaxNode ToAST()
            {
                Children.RemoveAt(3); // Remove the arrow
                Children.RemoveAt(0); // Remove the function keyword
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i] = Children[i].ToAST();
                }
                return this;
            }
        }
        public class ParameterListNode : SyntaxNode
        {
            private static List<SyntaxNode> EmptyParams { get; } = [];

            public ParameterListNode(OpenParenthesisLeaf openParen, List<SyntaxNode> parameters, CloseParenthesisLeaf closeParen)
                : base([openParen, .. parameters, closeParen])
                => UpdateRange();
            public ParameterListNode(OpenParenthesisLeaf openParen, CloseParenthesisLeaf closeParen)
                : this(openParen, EmptyParams, closeParen) { }

            public override SyntaxNode ToAST()
            {
                Children.RemoveAt(Children.Count - 1); // Remove the close parenthesis
                Children.RemoveAt(0); // Remove the open parenthesis
                for (int i = 0; i < Children.Count; i++)
                {
                    if (Children[i] is CommaLeaf)
                    {
                        Children.RemoveAt(i);
                        i--;
                        continue;
                    }
                    Children[i] = Children[i].ToAST();
                }
                return this;
            }
        }

        public class WhileStatementNode : SyntaxNode
        {
            public WhileStatementNode(WhileKeywordLeaf whileKeyword, SyntaxNode condition, StatementBlockNode body)
                : base([whileKeyword, condition, body])
                => UpdateRange();

            public override SyntaxNode ToAST()
            {
                Children.RemoveAt(0); // Remove the while keyword
                Children[1] = Children[1].ToAST(); // Convert the condition to AST
                return this;
            }
        }

        public abstract class BlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
            : SyntaxNode([openBrace, .. statements, closeBrace]);
        public class StatementBlockNode : BlockNode
        {
            public StatementBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
                : base(openBrace, statements, closeBrace)
                => UpdateRange();

            public override SyntaxNode ToAST()
            {
                if (Children.Count == 3 && Children[1] is BlockNode blockNode)
                {
                    return blockNode.ToAST();
                }

                Children.RemoveAt(Children.Count - 1); // Remove the close brace
                Children.RemoveAt(0); // Remove the open brace
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i] = Children[i].ToAST();
                }
                return this;
            }
        }


        public class EpsilonNode : SyntaxNode
        {
            public static EpsilonNode Instance { get; } = new();
            private EpsilonNode() { }

            public override SyntaxNode ToAST() => this;
        }
        public class EmptyStatement : SyntaxNode
        {
            public EmptyStatement(SemicolonLeaf semicolon) : base([semicolon])
                => UpdateRange();

            public override SyntaxNode ToAST()
            {
                Children.Clear();
                return this;
            }
        }
    }
}
