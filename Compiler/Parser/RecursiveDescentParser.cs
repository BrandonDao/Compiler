using CompilerLib.Lexer;
using CompilerLib.Nodes;
using CompilerLib.Nodes.Functions;
using CompilerLib.Nodes.Operators;
using CompilerLib.Nodes.Punctuation;
using CompilerLib.Nodes.Scopes;
using CompilerLib.Nodes.Statements;
using CompilerLib.Nodes.Statements.Controls;
using CompilerLib.Nodes.Types;
using CompilerLib.Parser;

namespace Compiler.Parser;

public class RecursiveDescentParser : IParser
{
    public static RecursiveDescentParser Instance { get; private set; } = new();

    private RecursiveDescentParser() { }

    public ParserEntrypointNode ParseTokensToCST(ITokenStream tokenStream)
    {
        tokenStream = tokenStream.IsAtEnd
            ? throw new ArgumentException("Cannot parse an empty stream of tokens!")
            : (ITokenStream)HangWhitespace(tokenStream);

        return new ParserEntrypointNode(ParseNamespaceDefinition(tokenStream));
    }

    public ParserEntrypointNode ParseCSTToAST(ParserEntrypointNode root) => root.ToAST();

    private static TokenStream HangWhitespace(ITokenStream tokenStream)
    {
        List<LeafNode> trimmedTokens = [];

        if (tokenStream.Peek() is WhitespaceLeaf leadingWhitespace)
        {
            tokenStream.Advance(); // consume the leading whitespace token
            leadingWhitespace.IsLeading = true;
            tokenStream.Peek().Children.Add(leadingWhitespace);
        }
        else
        {
            tokenStream.Consume(out LeafNode firstToken);
            trimmedTokens.Add(firstToken);
        }

        while (!tokenStream.IsAtEnd)
        {
            if (tokenStream.Peek() is WhitespaceLeaf currentWhitespace)
            {
                tokenStream.Advance(); // consume the whitespace token
                trimmedTokens[^1].Children.Add(currentWhitespace);
                continue;
            }

            tokenStream.Consume(out LeafNode currentToken);
            trimmedTokens.Add(currentToken);
        }

        return new TokenStream(trimmedTokens);
    }



    private static TypeLeafNode ParseType(ITokenStream tokens)
        => ParseType(tokens, $"Expected an identifier or primitive type, not {tokens.Peek()}!");
    private static TypeLeafNode ParseType(ITokenStream tokens, string customFailureMessage)
        => tokens.Peek() switch
        {
            IdentifierLeaf => tokens.Consume<IdentifierLeaf>(),
            Int8Leaf => tokens.Consume<Int8Leaf>(),
            Int16Leaf => tokens.Consume<Int16Leaf>(),
            Int32Leaf => tokens.Consume<Int32Leaf>(),
            Int64Leaf => tokens.Consume<Int64Leaf>(),
            BoolLeaf => tokens.Consume<BoolLeaf>(),
            _ => throw new ArgumentException(customFailureMessage)
        };

    private static VariableDefinitionNode ParseVariableDefinition(ITokenStream tokens)
    {
        tokens.Consume(out LetKeywordLeaf letKeywordLeaf);
        VariableNameTypeNode varNameTypeNode = ParseVariableNameType(tokens);
        tokens.Consume(out AssignmentOperatorLeaf assignmentOpLeaf);
        SyntaxNode valueExpr = ParseExpression(tokens);
        tokens.Consume(out SemicolonLeaf semicolonLeaf);

        return new VariableDefinitionNode(letKeywordLeaf, varNameTypeNode, assignmentOpLeaf, valueExpr, semicolonLeaf);
    }

    private static VariableNameTypeNode ParseVariableNameType(ITokenStream tokens)
    {
        tokens.Consume(out IdentifierLeaf idToken);
        tokens.Consume(out ColonLeaf colonToken);
        TypeLeafNode typeNode = ParseType(tokens, $"Expected a type after ':' token, not {tokens.Peek()}!");

        return new VariableNameTypeNode(idToken, colonToken, typeNode);
    }

    private static SyntaxNode ParseExpression(ITokenStream tokens)
        => ParseExpression(tokens, minPrecedence: 0);

    private static SyntaxNode ParseExpression(ITokenStream tokens, int minPrecedence)
    {
        SyntaxNode expr = ParsePrefix(tokens);

        // look ahead one to check for operator + precedence
        while (!tokens.IsAtEnd && TryGetInfixPrecedence(tokens.Peek(), out int opPrecedence))
        {
            // led = left/infix denotation parsing (terminology from Pratt parsing)
            LeafNode opToken = tokens.Peek();
            bool isLeftAssociative = IsLeftAssociative(opToken);

            if (opPrecedence < minPrecedence)
            {
                break;
            }

            tokens.Advance();

            SyntaxNode rhs = ParseExpression(
                tokens,
                isLeftAssociative ? opPrecedence + 1 : opPrecedence); // +1 to enforce leftward grouping for same-precedence ops

            expr = CreateBinaryOp(opToken, expr, rhs);
        }
        return expr;

        static bool TryGetInfixPrecedence(LeafNode token, out int precedence)
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
                _ => throw new ArgumentException($"Unknown operator: {op}! This should never happen if {nameof(TryGetInfixPrecedence)} is correct.")
            };
        }
    }

    // nud = null/prefix denotation parsing
    private static SyntaxNode ParsePrefix(ITokenStream tokens)
        => tokens.Peek() switch
        {
            NotOperatorLeaf notToken => ParseNotOperator(notToken, tokens),
            OpenParenthesisLeaf openToken => ParseParenthesizedExpression(openToken, tokens),
            IdentifierLeaf idToken => ParseIdentifierOrFunctionCall(idToken, tokens),
            IntLiteralLeaf intLiteralLeaf => ParseIntLiteral(intLiteralLeaf, tokens),
            BoolLiteralLeaf boolLiteralLeaf => ParseBoolLiteral(boolLiteralLeaf, tokens),
            _ => throw new ArgumentException($"Could not parse Primary Term, found {tokens.Peek()}!")
        };

    private static IntLiteralLeaf ParseIntLiteral(IntLiteralLeaf intLiteralLeaf, ITokenStream tokens)
    {
        tokens.Advance();
        return intLiteralLeaf;
    }
    private static BoolLiteralLeaf ParseBoolLiteral(BoolLiteralLeaf boolLiteralLeaf, ITokenStream tokens)
    {
        tokens.Advance();
        return boolLiteralLeaf;
    }

    private static NotOperationNode ParseNotOperator(NotOperatorLeaf notToken, ITokenStream tokens)
    {
        tokens.Advance(); // consume the '!' token
        SyntaxNode notValueNode = ParseExpression(tokens, minPrecedence: int.MaxValue);

        NotOperationNode notOpNode = new([notToken, notValueNode]);
        notOpNode.UpdateRange();
        return notOpNode;
    }

    private static ParenthesizedExpression ParseParenthesizedExpression(OpenParenthesisLeaf openToken, ITokenStream tokens)
    {
        tokens.Advance(); // consume the '(' token
        SyntaxNode exprNode = ParseExpression(tokens);

        tokens.Consume(out CloseParenthesisLeaf closeToken);

        ParenthesizedExpression parenthesizedExpr = new(openToken, exprNode, closeToken);
        parenthesizedExpr.UpdateRange();
        return parenthesizedExpr;
    }

    private static SyntaxNode ParseIdentifierOrFunctionCall(IdentifierLeaf idToken, ITokenStream tokens)
    {
        tokens.Advance(); // consume the identifier token

        if (tokens.IsAtEnd || tokens.Peek() is not OpenParenthesisLeaf openParen)
        {
            return idToken;
        }

        tokens.Advance(); // consume the '(' token
        return ParseFuncCallExpr(tokens, openParen, idToken);
    }


    private static FunctionCallExpressionNode ParseFuncCallExpr(ITokenStream tokens, OpenParenthesisLeaf openParen, IdentifierLeaf idToken)
    {
        ArgumentListNode argumentList = ParseArgumentList(tokens, openParen);
        return new FunctionCallExpressionNode(idToken, argumentList);
    }
    private static ArgumentListNode ParseArgumentList(ITokenStream tokens, OpenParenthesisLeaf openParen)
    {
        if (tokens.Peek() is CloseParenthesisLeaf earlyCloseParen)
        {
            tokens.Advance(); // consume the ')' token
            return new ArgumentListNode(openParen, earlyCloseParen);
        }

        List<SyntaxNode> arguments = [];
        while (true)
        {
            SyntaxNode arg = ParseExpression(tokens);
            arguments.Add(arg);

            if (tokens.Peek() is CommaLeaf commaToken)
            {
                tokens.Advance(); // consume the ',' token
                arguments.Add(commaToken);
            }
            else if (tokens.Peek() is CloseParenthesisLeaf closeParen)
            {
                tokens.Advance(); // consume the ')' token
                return new ArgumentListNode(openParen, arguments, closeParen);
            }
            else
            {
                throw new ArgumentException($"Expected ',' or ')' in argument list, not {tokens.Peek()}!");
            }
        }
    }


    private static FunctionDefinitionNode ParseFunctionDefinition(ITokenStream tokens)
    {
        tokens.Consume(out FunctionKeywordLeaf funcKeywordLeaf);
        tokens.Consume(out IdentifierLeaf idToken);
        ParameterListNode parameterList = ParseParameterList(tokens);

        if (tokens.Peek() is not SmallArrowLeaf arrowToken)
        {
            tokens.Consume(out OpenBraceLeaf openBraceToken, $"Expected '->' or '{{' token after function parameters, not {tokens.Peek()}!");

            FunctionBlockNode voidReturningBody = ParseFunctionBlock(tokens, openBraceToken);
            LeafNode currentToken = tokens.Peek(); // Used only for start/end line/char information
            return new FunctionDefinitionNode(
                funcKeywordLeaf,
                idToken,
                parameterList,
                new ImplicitSmallArrowLeaf(currentToken.StartLine, currentToken.StartChar),
                new ImplicitVoidLeaf(currentToken.StartLine, currentToken.StartChar),
                voidReturningBody);
        }
        tokens.Advance(); // consume the '->' token

        if (tokens.Peek() is VoidLeaf)
        {
            tokens.Consume(out VoidLeaf voidLeaf);

            tokens.Consume(out OpenBraceLeaf openBraceToken, $"Expected '{{' token after 'void' return type, not {tokens.Peek()}!");

            FunctionBlockNode body = ParseFunctionBlock(tokens, openBraceToken);
            return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, voidLeaf, body);
        }
        else
        {
            TypeLeafNode returnType = ParseType(tokens);

            tokens.Consume(out OpenBraceLeaf openBraceToken, $"Expected '{{' token after function '{returnType}' return type, not {tokens.Peek()}!");

            FunctionBlockNode body = ParseFunctionBlock(tokens, openBraceToken);
            return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, returnType, body);
        }
    }
    private static ParameterListNode ParseParameterList(ITokenStream tokens)
    {
        tokens.Consume(out OpenParenthesisLeaf openParenToken);

        if (tokens.Peek() is CloseParenthesisLeaf earlyCloseParenToken)
        {
            tokens.Advance(); // consume the ')' token
            return new ParameterListNode(openParenToken, earlyCloseParenToken);
        }

        List<SyntaxNode> parameters = [];

        CloseParenthesisLeaf closeParenToken;
        while (true)
        {
            if (tokens.Peek() is IdentifierLeaf)
            {
                VariableNameTypeNode nameType = ParseVariableNameType(tokens);
                parameters.Add(nameType);
            }

            if (tokens.Peek() is CommaLeaf comma)
            {
                tokens.Advance(); // consume the ',' token
                parameters.Add(comma);
            }
            else if (tokens.Peek() is CloseParenthesisLeaf closeParenCandidate)
            {
                closeParenToken = closeParenCandidate;
                tokens.Advance(); // consume the ')' token
                break;
            }
            else
            {
                throw new ArgumentException($"Expected ',' or ')' in parameter list, not {tokens.Peek()}!");
            }
        }
        return new ParameterListNode(openParenToken, parameters, closeParenToken);
    }

    private static WhileStatementNode ParseWhileStatement(ITokenStream tokens)
    {
        tokens.Consume(out WhileKeywordLeaf whileKeywordLeaf);
        SyntaxNode condition = ParseExpression(tokens);
        tokens.Consume(out OpenBraceLeaf openBraceToken, $"Expected '{{' token after while loop condition, not {tokens.Peek()}!");
        FunctionBlockNode body = ParseFunctionBlock(tokens, openBraceToken);

        return new WhileStatementNode(whileKeywordLeaf, condition, body);
    }

    private static NamespaceDefinitionNode ParseNamespaceDefinition(ITokenStream tokens)
    {
        tokens.Consume(out NamespaceKeywordLeaf namespaceKeywordLeaf);
        QualifiedNameNode qualifiedName = ParseQualifiedName(tokens);
        NonLocalBlockNode block = ParseHighLevelBlock(tokens);

        return new NamespaceDefinitionNode(namespaceKeywordLeaf, qualifiedName, block);
    }
    private static QualifiedNameNode ParseQualifiedName(ITokenStream tokens)
    {
        tokens.Consume(out IdentifierLeaf startIdLeaf);
        List<SyntaxNode> nameParts = [startIdLeaf];

        // contains ids and dots
        while (!tokens.IsAtEnd && tokens.Peek() is IdentifierLeaf or DotLeaf)
        {
            nameParts.Add(tokens.Consume<LeafNode>());
        }
        return new QualifiedNameNode(nameParts);
    }

    private static FunctionBlockNode ParseFunctionBlock(ITokenStream tokens, OpenBraceLeaf openBraceToken)
        => ParseBlock(tokens, openBraceToken, (open, statements, close) => new FunctionBlockNode(open, statements, close));
    private static LocalBlockNode ParseLocalBlock(ITokenStream tokens, OpenBraceLeaf openBraceToken)
        => ParseBlock(tokens, openBraceToken, (open, statements, close) => new LocalBlockNode(open, statements, close));

    private static T ParseBlock<T>(
            ITokenStream tokens,
            OpenBraceLeaf openBraceToken,
            Func<OpenBraceLeaf, List<SyntaxNode>, CloseBraceLeaf, T> factory)
        where T : BlockNode
    {
        (List<SyntaxNode> statements, CloseBraceLeaf closeBraceLeaf) = ParseStatementsInBlock(tokens);

        return factory.Invoke(openBraceToken, statements, closeBraceLeaf);
    }

    private static (List<SyntaxNode> statements, CloseBraceLeaf closeBrace) ParseStatementsInBlock(ITokenStream tokens)
    {
        List<SyntaxNode> statements = [];

        CloseBraceLeaf closeBraceLeaf;
        while (true)
        {
            if (tokens.Peek() is LetKeywordLeaf)
            {
                statements.Add(ParseVariableDefinition(tokens));
            }
            else if (tokens.Peek() is WhileKeywordLeaf)
            {
                statements.Add(ParseWhileStatement(tokens));
            }
            else if (tokens.Peek() is IdentifierLeaf identifier)
            {
                tokens.Advance(); // consume the identifier token
                if (tokens.Peek() is OpenParenthesisLeaf openParen)
                {
                    tokens.Advance(); // consume the '(' token
                    FunctionCallExpressionNode funcCallExpr = ParseFuncCallExpr(tokens, openParen, identifier);

                    tokens.Consume(out SemicolonLeaf semicolon, $"Expected ';' token after a function call, not {tokens.Peek()}!");

                    statements.Add(new FunctionCallStatementNode(funcCallExpr, semicolon));
                }
                else if (tokens.Peek() is AssignmentOperatorLeaf assignmentOp)
                {
                    tokens.Advance(); // consume the assignment operator token
                    SyntaxNode assignedValue = ParseExpression(tokens);

                    tokens.Consume(out SemicolonLeaf semicolon, $"Expected ';' token at the end of assignment statement, not {tokens.Peek()}!");

                    statements.Add(new AssignmentStatementNode(identifier, assignmentOp, assignedValue, semicolon));
                }
                else
                {
                    throw new ArgumentException($"Unexpected token after identifier in statement: {tokens.Peek()}!");
                }
            }
            else if (tokens.Peek() is ReturnKeywordLeaf returnKeyword)
            {
                tokens.Advance(); // consume the 'return' token
                if (tokens.Peek() is SemicolonLeaf earlySemicolon)
                {
                    tokens.Advance(); // consume the ';' token
                    statements.Add(new ReturnStatementNode(returnKeyword, earlySemicolon));
                    continue;
                }

                SyntaxNode returnValue = ParseExpression(tokens);

                tokens.Consume(out SemicolonLeaf semicolonLeaf, $"Expected ';' token at the end of return statement, not {tokens.Peek()}!");

                statements.Add(new ReturnStatementNode(returnKeyword, returnValue, semicolonLeaf));
            }
            else if (tokens.Peek() is OpenBraceLeaf openBraceToken)
            {
                tokens.Advance(); // consume the '{' token
                statements.Add(ParseLocalBlock(tokens, openBraceToken));
            }
            else if (tokens.Peek() is SemicolonLeaf semicolon)
            {
                tokens.Advance(); // consume the ';' token
                statements.Add(new EmptyStatementNode(semicolon));
            }
            else if (tokens.Peek() is CloseBraceLeaf leaf)
            {
                tokens.Advance(); // consume the '}' token
                closeBraceLeaf = leaf;
                break;
            }
            else
            {
                throw new ArgumentException($"Unexpected token in block: {tokens.Peek()}!");
            }
        }
        return (statements, closeBraceLeaf);
    }

    private static NonLocalBlockNode ParseHighLevelBlock(ITokenStream tokens)
    {
        tokens.Consume(out OpenBraceLeaf openBraceToken);
        List<SyntaxNode> statements = [];

        while (true)
        {
            if (tokens.Peek() is FunctionKeywordLeaf)
            {
                statements.Add(ParseFunctionDefinition(tokens));
            }
            else if (tokens.Peek() is NamespaceKeywordLeaf)
            {
                statements.Add(ParseNamespaceDefinition(tokens));
            }
            else if (tokens.Peek() is LetKeywordLeaf)
            {
                statements.Add(ParseVariableDefinition(tokens));
            }
            else if (tokens.Peek() is CloseBraceLeaf)
            {
                tokens.Consume(out CloseBraceLeaf closeBraceLeaf);
                return new NonLocalBlockNode(openBraceToken, statements, closeBraceLeaf);
            }
            else
            {
                throw new ArgumentException($"Unexpected token in block: {tokens.Peek()}!");
            }
        }
    }
}