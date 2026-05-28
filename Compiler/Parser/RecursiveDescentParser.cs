using CompilerLib.Nodes;
using CompilerLib.Nodes.Functions;
using CompilerLib.Nodes.Operators;
using CompilerLib.Nodes.Punctuation;
using CompilerLib.Nodes.Scopes;
using CompilerLib.Nodes.Statements;
using CompilerLib.Nodes.Statements.Controls;
using CompilerLib.Nodes.Types;

namespace Compiler.Parser
{
    public class RecursiveDescentParser : IParser
    {
        public static RecursiveDescentParser Instance { get; private set; } = new();

        private RecursiveDescentParser() { }

        public ParserEntrypointNode? ParseTokensToCST(List<LeafNode> tokens)
        {
            if (tokens.Count == 0) return null;

            int position = 0;
            tokens = HangWhitespace(tokens);
            return new ParserEntrypointNode(ParseNamespaceDefinition(tokens, ref position));
        }

        public ParserEntrypointNode ParseCSTToAST(ParserEntrypointNode root) => root.ToAST();

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



        private static TypeLeafNode ParseType(List<LeafNode> tokens, ref int position)
        {
            switch (tokens[position])
            {
                case IdentifierLeaf idToken: position++; return idToken;
                case Int8Leaf int8Token: position++; return int8Token;
                case Int16Leaf int16Token: position++; return int16Token;
                case Int32Leaf int32Token: position++; return int32Token;
                case Int64Leaf int64Token: position++; return int64Token;
                case BoolLeaf boolToken: position++; return boolToken;
                default: throw new ArgumentException($"Expected an identifier or primitive type, not {tokens[position]}!");
            }
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
            var valueExpr = ParseExpression(tokens, ref position);

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

        private static SyntaxNode ParseExpression(List<LeafNode> tokens, ref int position)
            => ParseExpression(tokens, ref position, minPrecedence: 0);
        private static SyntaxNode ParseExpression(List<LeafNode> tokens, ref int position, int minPrecedence)
        {
            SyntaxNode expr = ParsePrimary(tokens, ref position);

            // look ahead one to check for operator + precedence
            while (position < tokens.Count && TryGetOperatorPrecedence(tokens[position], out int opPrecedence))
            {
                // led = left/infix denotation parsing (terminology from Pratt parsing)
                LeafNode opToken = tokens[position];
                bool isLeftAssociative = IsLeftAssociative(opToken);

                if (opPrecedence < minPrecedence) break;

                position++;

                SyntaxNode rhs = ParseExpression(
                    tokens,
                    ref position,
                    isLeftAssociative ? opPrecedence + 1 : opPrecedence); // +1 to enforce leftward grouping for same-precedence ops

                expr = CreateBinaryOp(opToken, expr, rhs);
            }
            return expr;

            // Helpers
            static bool TryGetOperatorPrecedence(LeafNode token, out int precedence)
            {
                precedence = token switch
                {
                    OrOperatorLeaf => 10,
                    AndOperatorLeaf => 20,
                    EqualityOperatorLeaf => 30,

                    PlusOperatorLeaf
                    or MinusOperatorLeaf
                    => 40,

                    MultiplyOperatorLeaf
                    or DivideOperatorLeaf
                    or ModOperatorLeaf
                    => 50,

                    _ => -1
                };
                return precedence != -1;
            }
            static bool IsLeftAssociative(LeafNode token) => token is not AssignmentOperatorLeaf;

            static SyntaxNode CreateBinaryOp(LeafNode op, SyntaxNode lhs, SyntaxNode rhs)
            {
                List<SyntaxNode> children = [lhs, op, rhs];
                return op switch
                {
                    PlusOperatorLeaf => new AddOperationNode(children),
                    MinusOperatorLeaf => new SubtractOperationNode(children),
                    MultiplyOperatorLeaf => new MultiplyOperationNode(children),
                    DivideOperatorLeaf => new DivideOperationNode(children),
                    ModOperatorLeaf => new ModOperationNode(children),
                    OrOperatorLeaf => new OrOperationNode(children),
                    AndOperatorLeaf => new AndOperationNode(children),
                    EqualityOperatorLeaf => new EqualityOperationNode(children),
                    _ => throw new ArgumentException($"Unknown operator: {op}! This should never happen if {nameof(TryGetOperatorPrecedence)} is correct.")
                };
            }
        }

        // nud = null/prefix denotation parsing
        private static SyntaxNode ParsePrimary(List<LeafNode> tokens, ref int position)
        {
            switch (tokens[position])
            {
                case NotOperatorLeaf notToken:
                    {
                        position++;
                        SyntaxNode? notValueNode = ParseExpression(tokens, ref position, minPrecedence: int.MaxValue)
                            ?? throw new ArgumentException("Could not parse the expression after the '!' token!");

                        NotOperationNode notOpNode = new([notToken, notValueNode]);
                        notOpNode.UpdateRange();
                        return notOpNode;
                    }

                case OpenParenthesisLeaf openToken:
                    {
                        position++;
                        SyntaxNode? exprNode = ParseExpression(tokens, ref position)
                            ?? throw new ArgumentException("Could not parse the expression after the '(' token!");

                        if(position >= tokens.Count) throw new ArgumentException("Expected ')' token, found end of input instead!");

                        if (tokens[position++] is not CloseParenthesisLeaf closeToken)
                            throw new ArgumentException("Expected ')' token!");

                        ParenthesizedExpression parenthesizedExpr = new(openToken, exprNode, closeToken);
                        parenthesizedExpr.UpdateRange();
                        return parenthesizedExpr;
                    }

                case IdentifierLeaf idToken:
                    position++;

                    if (position >= tokens.Count || tokens[position] is not OpenParenthesisLeaf openParen) return idToken;

                    position++; // consume openParen
                    return ParseFuncCallExpr(tokens, ref position, openParen, idToken);

                case IntLiteralLeaf litToken:
                    position++;
                    return litToken;

                case BoolLiteralLeaf strLitToken:
                    position++;
                    return strLitToken;

                default: throw new ArgumentException($"Could not parse Primary Term, found {tokens[position]}!");
            }
        }

        private static FunctionCallExpressionNode ParseFuncCallExpr(List<LeafNode> tokens, ref int position, OpenParenthesisLeaf openParen, IdentifierLeaf idToken)
        {
            var argumentList = ParseArgumentList(tokens, ref position, openParen);
            return new FunctionCallExpressionNode(idToken, argumentList);
        }
        private static ArgumentListNode ParseArgumentList(List<LeafNode> tokens, ref int position, OpenParenthesisLeaf openParen)
        {
            if (tokens[position] is CloseParenthesisLeaf earlyCloseParen)
            {
                position++;
                return new ArgumentListNode(openParen, earlyCloseParen);
            }

            List<SyntaxNode> arguments = [];
            while (true)
            {
                var arg = ParseExpression(tokens, ref position);
                arguments.Add(arg);

                if (tokens[position] is CommaLeaf commaToken)
                {
                    position++;
                    arguments.Add(commaToken);
                }
                else if (tokens[position] is CloseParenthesisLeaf closeParen)
                {
                    position++;
                    return new ArgumentListNode(openParen, arguments, closeParen);
                }
                else throw new ArgumentException($"Expected ',' or ')' in argument list, not {tokens[position]}!");
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

                var voidReturningBody = ParseFunctionBlock(tokens, ref position);
                return new FunctionDefinitionNode(
                    funcKeywordLeaf,
                    idToken,
                    parameterList,
                    new ImplicitSmallArrowLeaf(tokens[position].StartLine, tokens[position].StartChar),
                    new ImplicitVoidLeaf(tokens[position].StartLine, tokens[position].StartChar),
                    voidReturningBody);
            }
            position++;

            if (tokens[position] is VoidLeaf voidLeaf)
            {
                position++;
                var body = ParseFunctionBlock(tokens, ref position);
                return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, voidLeaf, body);
            }
            else
            {
                var returnType = ParseType(tokens, ref position);
                var body = ParseFunctionBlock(tokens, ref position);
                return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, returnType, body);
            }
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
            var condition = ParseExpression(tokens, ref position);

            var body = ParseFunctionBlock(tokens, ref position);

            return new WhileStatementNode(whileKeywordLeaf, condition, body);
        }

        private static NamespaceDefinitionNode ParseNamespaceDefinition(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not NamespaceKeywordLeaf namespaceKeywordLeaf)
                throw new ArgumentException($"Expected 'namespace' keyword at start of namespace definition, not {tokens[position]}!");

            position++;
            var qualifiedName = ParseQualifiedName(tokens, ref position);

            var block = ParseHighLevelBlock(tokens, ref position);

            return new NamespaceDefinitionNode(namespaceKeywordLeaf, qualifiedName, block);
        }
        private static QualifiedNameNode ParseQualifiedName(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not IdentifierLeaf startIdLeaf)
                throw new ArgumentException($"Expected an identifier at start of qualified name, not {tokens[position]}!");

            List<SyntaxNode> nameParts = [startIdLeaf];
            position++;

            // contains ids and dots
            while (tokens[position] is IdentifierLeaf or DotLeaf)
            {
                nameParts.Add(tokens[position]);
                position++;
            }
            return new QualifiedNameNode(nameParts);
        }

        private static FunctionBlockNode ParseFunctionBlock(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not OpenBraceLeaf openBraceToken)
                throw new ArgumentException($"Expected '{{' token at start of block, not {tokens[position]}!");

            position++;
            var (statements, closeBraceLeaf) = ParseStatementsInBlock(tokens, ref position);

            return new FunctionBlockNode(openBraceToken, statements, closeBraceLeaf);
        }
        private static LocalBlockNode ParseLocalBlock(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not OpenBraceLeaf openBraceToken)
                throw new ArgumentException($"Expected '{{' token at start of block, not {tokens[position]}!");

            position++;
            var (statements, closeBraceLeaf) = ParseStatementsInBlock(tokens, ref position);

            return new LocalBlockNode(openBraceToken, statements, closeBraceLeaf);
        }

        private static (List<SyntaxNode> statements, CloseBraceLeaf closeBrace) ParseStatementsInBlock(List<LeafNode> tokens, ref int position)
        {
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
                else if (tokens[position] is IdentifierLeaf identifier)
                {
                    position++;
                    if (tokens[position] is OpenParenthesisLeaf openParen)
                    {
                        position++;
                        FunctionCallExpressionNode funcCallExpr = ParseFuncCallExpr(tokens, ref position, openParen, identifier);

                        if (tokens[position] is not SemicolonLeaf semicolon) throw new ArgumentException($"Unexpected token after function call statement: {tokens[position]}!");

                        position++;
                        statements.Add(new FunctionCallStatementNode(funcCallExpr, semicolon));
                    }
                    else if (tokens[position] is AssignmentOperatorLeaf assignmentOp)
                    {
                        position++;
                        SyntaxNode assignedValue = ParseExpression(tokens, ref position);

                        if (tokens[position] is not SemicolonLeaf semicolon)
                            throw new ArgumentException($"Expected ';' token at the end of assignment statement, not {tokens[position]}!");

                        position++;
                        statements.Add(new AssignmentStatementNode(identifier, assignmentOp, assignedValue, semicolon));
                    }
                    else throw new ArgumentException($"Unexpected token after identifier in statement: {tokens[position]}!");
                }
                else if (tokens[position] is ReturnKeywordLeaf returnKeyword)
                {
                    position++;
                    if (tokens[position] is SemicolonLeaf earlySemicolon)
                    {
                        position++;
                        statements.Add(new ReturnStatementNode(returnKeyword, earlySemicolon));
                        continue;
                    }

                    SyntaxNode returnValue = ParseExpression(tokens, ref position);

                    if (tokens[position] is not SemicolonLeaf semicolonLeaf)
                        throw new ArgumentException($"Expected ';' token at the end of return statement, not {tokens[position]}!");

                    position++;
                    statements.Add(new ReturnStatementNode(returnKeyword, returnValue, semicolonLeaf));
                }
                else if (tokens[position] is OpenBraceLeaf)
                {
                    statements.Add(ParseLocalBlock(tokens, ref position));
                }
                else if (tokens[position] is SemicolonLeaf semicolon)
                {
                    position++;
                    statements.Add(new EmptyStatementNode(semicolon));
                }
                else if (tokens[position] is CloseBraceLeaf leaf)
                {
                    position++;
                    closeBraceLeaf = leaf;
                    break;
                }
                else throw new ArgumentException($"Unexpected token in block: {tokens[position]}!");
            }
            return (statements, closeBraceLeaf);
        }

        private static NonLocalBlockNode ParseHighLevelBlock(List<LeafNode> tokens, ref int position)
        {
            if (tokens[position] is not OpenBraceLeaf openBraceToken)
                throw new ArgumentException($"Expected '{{' token at start of block, not {tokens[position]}!");

            position++;
            List<SyntaxNode> statements = [];

            CloseBraceLeaf closeBraceLeaf;
            while (true)
            {
                if (tokens[position] is FunctionKeywordLeaf)
                {
                    statements.Add(ParseFunctionDefinition(tokens, ref position));
                }
                else if (tokens[position] is NamespaceKeywordLeaf)
                {
                    statements.Add(ParseNamespaceDefinition(tokens, ref position));
                }
                else if (tokens[position] is LetKeywordLeaf)
                {
                    statements.Add(ParseVariableDefinition(tokens, ref position));
                }
                else if (tokens[position] is CloseBraceLeaf leaf)
                {
                    position++;
                    closeBraceLeaf = leaf;
                    break;
                }
                else throw new ArgumentException($"Unexpected token in block: {tokens[position]}!");
            }
            return new NonLocalBlockNode(openBraceToken, statements, closeBraceLeaf);
        }
    }
}