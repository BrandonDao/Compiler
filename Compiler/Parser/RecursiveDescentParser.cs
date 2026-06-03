using CompilerLib.Nodes;
using CompilerLib.Nodes.Functions;
using CompilerLib.Nodes.Operators;
using CompilerLib.Nodes.Punctuation;
using CompilerLib.Nodes.Scopes;
using CompilerLib.Nodes.Statements;
using CompilerLib.Nodes.Statements.Controls;
using CompilerLib.Nodes.Types;

namespace Compiler.Parser;

public class RecursiveDescentParser : IParser
{
    public static RecursiveDescentParser Instance { get; private set; } = new();

    private RecursiveDescentParser() { }

    public ParserEntrypointNode? ParseTokensToCST(List<LeafNode> tokens)
    {
        if (tokens.Count == 0)
        {
            return null;
        }

        tokens = HangWhitespace(tokens);
        TokenStream tokenStream = new(tokens);
        return new ParserEntrypointNode(ParseNamespaceDefinition(tokenStream));
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



    private static TypeLeafNode ParseType(TokenStream tokens)
        => tokens.CurrentToken switch
        {
            IdentifierLeaf => tokens.Consume<IdentifierLeaf>(),
            Int8Leaf => tokens.Consume<Int8Leaf>(),
            Int16Leaf => tokens.Consume<Int16Leaf>(),
            Int32Leaf => tokens.Consume<Int32Leaf>(),
            Int64Leaf => tokens.Consume<Int64Leaf>(),
            BoolLeaf => tokens.Consume<BoolLeaf>(),
            _ => throw new ArgumentException($"Expected an identifier or primitive type, not {tokens.CurrentToken}!")
        };


    private static VariableDefinitionNode ParseVariableDefinition(TokenStream tokens)
    {
        tokens.Consume(out LetKeywordLeaf letKeywordLeaf);
        VariableNameTypeNode varNameTypeNode = ParseVariableNameType(tokens);
        tokens.Consume(out AssignmentOperatorLeaf assignmentOpLeaf);
        SyntaxNode valueExpr = ParseExpression(tokens);
        tokens.Consume(out SemicolonLeaf semicolonLeaf);

        return new VariableDefinitionNode(letKeywordLeaf, varNameTypeNode, assignmentOpLeaf, valueExpr, semicolonLeaf);
    }

    private static VariableNameTypeNode ParseVariableNameType(TokenStream tokens)
    {
        tokens.Consume(out IdentifierLeaf idToken);
        tokens.Consume(out ColonLeaf colonToken);
        TypeLeafNode typeNode = ParseType(tokens)
            ?? throw new ArgumentException($"Expected a type after ':' token, not {tokens.CurrentToken}!");

        return new VariableNameTypeNode(idToken, colonToken, typeNode);
    }

    private static SyntaxNode ParseExpression(TokenStream tokens)
        => ParseExpression(tokens, minPrecedence: 0);

    private static SyntaxNode ParseExpression(TokenStream tokens, int minPrecedence)
    {
        SyntaxNode expr = ParsePrefix(tokens);

        // look ahead one to check for operator + precedence
        while (!tokens.IsAtEnd && TryGetInfixPrecedence(tokens.CurrentToken, out int opPrecedence))
        {
            // led = left/infix denotation parsing (terminology from Pratt parsing)
            LeafNode opToken = tokens.CurrentToken;
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
    private static SyntaxNode ParsePrefix(TokenStream tokens) => tokens.CurrentToken switch
    {
        NotOperatorLeaf notToken => ParseNotOperator(notToken, tokens),
        OpenParenthesisLeaf openToken => ParseParenthesizedExpression(openToken, tokens),
        IdentifierLeaf idToken => ParseIdentifierOrFunctionCall(idToken, tokens),
        IntLiteralLeaf intLiteralLeaf => ParseIntLiteral(intLiteralLeaf, tokens),
        BoolLiteralLeaf boolLiteralLeaf => ParseBoolLiteral(boolLiteralLeaf, tokens),
        _ => throw new ArgumentException($"Could not parse Primary Term, found {tokens.CurrentToken}!")
    };

    private static IntLiteralLeaf ParseIntLiteral(IntLiteralLeaf intLiteralLeaf, TokenStream tokens)
    {
        tokens.Advance();
        return intLiteralLeaf;
    }
    private static BoolLiteralLeaf ParseBoolLiteral(BoolLiteralLeaf boolLiteralLeaf, TokenStream tokens)
    {
        tokens.Advance();
        return boolLiteralLeaf;
    }

    private static NotOperationNode ParseNotOperator(NotOperatorLeaf notToken, TokenStream tokens)
    {
        tokens.Advance(); // consume the '!' token
        SyntaxNode? notValueNode = ParseExpression(tokens, minPrecedence: int.MaxValue)
            ?? throw new ArgumentException("Could not parse the expression after the '!' token!");

        NotOperationNode notOpNode = new([notToken, notValueNode]);
        notOpNode.UpdateRange();
        return notOpNode;
    }

    private static ParenthesizedExpression ParseParenthesizedExpression(OpenParenthesisLeaf openToken, TokenStream tokens)
    {
        tokens.Advance(); // consume the '(' token
        SyntaxNode? exprNode = ParseExpression(tokens)
            ?? throw new ArgumentException("Could not parse the expression after the '(' token!");

        tokens.Consume(out CloseParenthesisLeaf closeToken);

        ParenthesizedExpression parenthesizedExpr = new(openToken, exprNode, closeToken);
        parenthesizedExpr.UpdateRange();
        return parenthesizedExpr;
    }

    private static SyntaxNode ParseIdentifierOrFunctionCall(IdentifierLeaf idToken, TokenStream tokens)
    {
        tokens.Advance(); // consume the identifier token

        if (tokens.IsAtEnd || tokens.CurrentToken is not OpenParenthesisLeaf openParen)
        {
            return idToken;
        }

        tokens.Advance(); // consume the '(' token
        return ParseFuncCallExpr(tokens, openParen, idToken);
    }


    private static FunctionCallExpressionNode ParseFuncCallExpr(TokenStream tokens, OpenParenthesisLeaf openParen, IdentifierLeaf idToken)
    {
        ArgumentListNode argumentList = ParseArgumentList(tokens, openParen);
        return new FunctionCallExpressionNode(idToken, argumentList);
    }
    private static ArgumentListNode ParseArgumentList(TokenStream tokens, OpenParenthesisLeaf openParen)
    {
        if (tokens.CurrentToken is CloseParenthesisLeaf earlyCloseParen)
        {
            tokens.Advance(); // consume the ')' token
            return new ArgumentListNode(openParen, earlyCloseParen);
        }

        List<SyntaxNode> arguments = [];
        while (true)
        {
            SyntaxNode arg = ParseExpression(tokens);
            arguments.Add(arg);

            if (tokens.CurrentToken is CommaLeaf commaToken)
            {
                tokens.Advance(); // consume the ',' token
                arguments.Add(commaToken);
            }
            else if (tokens.CurrentToken is CloseParenthesisLeaf closeParen)
            {
                tokens.Advance(); // consume the ')' token
                return new ArgumentListNode(openParen, arguments, closeParen);
            }
            else
            {
                throw new ArgumentException($"Expected ',' or ')' in argument list, not {tokens.CurrentToken}!");
            }
        }
    }


    private static FunctionDefinitionNode ParseFunctionDefinition(TokenStream tokens)
    {
        tokens.Consume(out FunctionKeywordLeaf funcKeywordLeaf);
        tokens.Consume(out IdentifierLeaf idToken);
        ParameterListNode parameterList = ParseParameterList(tokens);

        if (tokens.CurrentToken is not SmallArrowLeaf arrowToken)
        {
            if (tokens.CurrentToken is not OpenBraceLeaf)
            {
                throw new ArgumentException($"Expected '->' or '{{' token after function parameters, not {tokens.CurrentToken}!");
            }

            FunctionBlockNode voidReturningBody = ParseFunctionBlock(tokens);
            LeafNode currentToken = tokens.CurrentToken;
            return new FunctionDefinitionNode(
                funcKeywordLeaf,
                idToken,
                parameterList,
                new ImplicitSmallArrowLeaf(currentToken.StartLine, currentToken.StartChar),
                new ImplicitVoidLeaf(currentToken.StartLine, currentToken.StartChar),
                voidReturningBody);
        }
        tokens.Advance(); // consume the '->' token

        if (tokens.CurrentToken is VoidLeaf)
        {
            tokens.Consume(out VoidLeaf voidLeaf);
            FunctionBlockNode body = ParseFunctionBlock(tokens);
            return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, voidLeaf, body);
        }
        else
        {
            TypeLeafNode returnType = ParseType(tokens);
            FunctionBlockNode body = ParseFunctionBlock(tokens);
            return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, returnType, body);
        }
    }
    private static ParameterListNode ParseParameterList(TokenStream tokens)
    {
        tokens.Consume(out OpenParenthesisLeaf openParenToken);

        if (tokens.CurrentToken is CloseParenthesisLeaf earlyCloseParenToken)
        {
            tokens.Advance(); // consume the ')' token
            return new ParameterListNode(openParenToken, earlyCloseParenToken);
        }

        List<SyntaxNode> parameters = [];

        CloseParenthesisLeaf closeParenToken;
        while (true)
        {
            if (tokens.CurrentToken is IdentifierLeaf)
            {
                VariableNameTypeNode nameType = ParseVariableNameType(tokens);
                parameters.Add(nameType);
            }

            if (tokens.CurrentToken is CommaLeaf comma)
            {
                tokens.Advance(); // consume the ',' token
                parameters.Add(comma);
            }
            else if (tokens.CurrentToken is CloseParenthesisLeaf closeParenCandidate)
            {
                closeParenToken = closeParenCandidate;
                tokens.Advance(); // consume the ')' token
                break;
            }
            else
            {
                throw new ArgumentException($"Expected ',' or ')' in parameter list, not {tokens.CurrentToken}!");
            }
        }
        return new ParameterListNode(openParenToken, parameters, closeParenToken);
    }

    private static WhileStatementNode ParseWhileStatement(TokenStream tokens)
    {
        tokens.Consume(out WhileKeywordLeaf whileKeywordLeaf);
        SyntaxNode condition = ParseExpression(tokens);
        FunctionBlockNode body = ParseFunctionBlock(tokens);

        return new WhileStatementNode(whileKeywordLeaf, condition, body);
    }

    private static NamespaceDefinitionNode ParseNamespaceDefinition(TokenStream tokens)
    {
        tokens.Consume(out NamespaceKeywordLeaf namespaceKeywordLeaf);
        QualifiedNameNode qualifiedName = ParseQualifiedName(tokens);
        NonLocalBlockNode block = ParseHighLevelBlock(tokens);

        return new NamespaceDefinitionNode(namespaceKeywordLeaf, qualifiedName, block);
    }
    private static QualifiedNameNode ParseQualifiedName(TokenStream tokens)
    {
        tokens.Consume(out IdentifierLeaf startIdLeaf);
        List<SyntaxNode> nameParts = [startIdLeaf];

        // contains ids and dots
        while (!tokens.IsAtEnd && tokens.CurrentToken is IdentifierLeaf or DotLeaf)
        {
            nameParts.Add(tokens.Consume<LeafNode>());
        }
        return new QualifiedNameNode(nameParts);
    }

    private static FunctionBlockNode ParseFunctionBlock(TokenStream tokens)
        => ParseBlock(tokens, (open, statements, close) => new FunctionBlockNode(open, statements, close));
    private static LocalBlockNode ParseLocalBlock(TokenStream tokens)
        => ParseBlock(tokens, (open, statements, close) => new LocalBlockNode(open, statements, close));

    private static T ParseBlock<T>(
            TokenStream tokens,
            Func<OpenBraceLeaf, List<SyntaxNode>, CloseBraceLeaf, T> factory)
        where T : BlockNode
    {
        tokens.Consume(out OpenBraceLeaf openBraceToken);
        (List<SyntaxNode> statements, CloseBraceLeaf closeBraceLeaf) = ParseStatementsInBlock(tokens);

        return factory.Invoke(openBraceToken, statements, closeBraceLeaf);
    }

    private static (List<SyntaxNode> statements, CloseBraceLeaf closeBrace) ParseStatementsInBlock(TokenStream tokens)
    {
        List<SyntaxNode> statements = [];

        CloseBraceLeaf closeBraceLeaf;
        while (true)
        {
            if (tokens.CurrentToken is LetKeywordLeaf)
            {
                statements.Add(ParseVariableDefinition(tokens));
            }
            else if (tokens.CurrentToken is WhileKeywordLeaf)
            {
                statements.Add(ParseWhileStatement(tokens));
            }
            else if (tokens.CurrentToken is IdentifierLeaf identifier)
            {
                tokens.Advance(); // consume the identifier token
                if (tokens.CurrentToken is OpenParenthesisLeaf openParen)
                {
                    tokens.Advance(); // consume the '(' token
                    FunctionCallExpressionNode funcCallExpr = ParseFuncCallExpr(tokens, openParen, identifier);

                    tokens.Consume(out SemicolonLeaf semicolon, $"Expected ';' token after a function call, not {tokens.CurrentToken}!");
                    
                    statements.Add(new FunctionCallStatementNode(funcCallExpr, semicolon));
                }
                else if (tokens.CurrentToken is AssignmentOperatorLeaf assignmentOp)
                {
                    tokens.Advance(); // consume the assignment operator token
                    SyntaxNode assignedValue = ParseExpression(tokens);

                    tokens.Consume(out SemicolonLeaf semicolon, $"Expected ';' token at the end of assignment statement, not {tokens.CurrentToken}!");
                    
                    statements.Add(new AssignmentStatementNode(identifier, assignmentOp, assignedValue, semicolon));
                }
                else
                {
                    throw new ArgumentException($"Unexpected token after identifier in statement: {tokens.CurrentToken}!");
                }
            }
            else if (tokens.CurrentToken is ReturnKeywordLeaf returnKeyword)
            {
                tokens.Advance(); // consume the 'return' token
                if (tokens.CurrentToken is SemicolonLeaf earlySemicolon)
                {
                    tokens.Advance(); // consume the ';' token
                    statements.Add(new ReturnStatementNode(returnKeyword, earlySemicolon));
                    continue;
                }

                SyntaxNode returnValue = ParseExpression(tokens);

                tokens.Consume(out SemicolonLeaf semicolonLeaf, $"Expected ';' token at the end of return statement, not {tokens.CurrentToken}!");

                statements.Add(new ReturnStatementNode(returnKeyword, returnValue, semicolonLeaf));
            }
            else if (tokens.CurrentToken is OpenBraceLeaf)
            {
                statements.Add(ParseLocalBlock(tokens));
            }
            else if (tokens.CurrentToken is SemicolonLeaf semicolon)
            {
                tokens.Consume<SemicolonLeaf>();
                statements.Add(new EmptyStatementNode(semicolon));
            }
            else if (tokens.CurrentToken is CloseBraceLeaf leaf)
            {
                tokens.Consume<CloseBraceLeaf>();
                closeBraceLeaf = leaf;
                break;
            }
            else
            {
                throw new ArgumentException($"Unexpected token in block: {tokens.CurrentToken}!");
            }
        }
        return (statements, closeBraceLeaf);
    }

    private static NonLocalBlockNode ParseHighLevelBlock(TokenStream tokens)
    {
        tokens.Consume(out OpenBraceLeaf openBraceToken);
        List<SyntaxNode> statements = [];

        while (true)
        {
            if (tokens.CurrentToken is FunctionKeywordLeaf)
            {
                statements.Add(ParseFunctionDefinition(tokens));
            }
            else if (tokens.CurrentToken is NamespaceKeywordLeaf)
            {
                statements.Add(ParseNamespaceDefinition(tokens));
            }
            else if (tokens.CurrentToken is LetKeywordLeaf)
            {
                statements.Add(ParseVariableDefinition(tokens));
            }
            else if (tokens.CurrentToken is CloseBraceLeaf)
            {
                tokens.Consume(out CloseBraceLeaf closeBraceLeaf);
                return new NonLocalBlockNode(openBraceToken, statements, closeBraceLeaf);
            }
            else
            {
                throw new ArgumentException($"Unexpected token in block: {tokens.CurrentToken}!");
            }
        }
    }
}