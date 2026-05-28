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
    private class TokenReader(List<LeafNode> tokens)
    {
        private readonly List<LeafNode> tokens = tokens;
        private int position = 0;

        public bool IsAtEnd => position >= tokens.Count;
        public LeafNode CurrentToken
        {
            get
            {
                if (IsAtEnd)
                {
                    throw new ArgumentException("Unexpected end of input!");
                }
                return tokens[position];
            }
        }

        public void Advance() => position++;

        // This signature isn't quite standard/idiomatic C#, but it allows type inference without using `var`
        // - `var` usage will remain an error to prevent agentic AI abuse
        // - `T token = Consume<T>();` may be more idiomatic but requires typing `T` twice which can become pretty verbose.
        public void Consume<T>(out T token) where T : LeafNode
            => token = Consume<T>();
        public void Consume<T>(out T token, string messageOnUnexpectedToken) where T : LeafNode
            => token = Consume<T>(messageOnUnexpectedToken);
        public T Consume<T>() where T : LeafNode
            => Consume<T>($"Expected token of type {typeof(T).Name}, not {tokens[position]}!");
        public T Consume<T>(string messageOnUnexpectedToken) where T : LeafNode
        {
            if (IsAtEnd)
            {
                throw new ArgumentException($"Unexpected end of input, expected token of type {typeof(T).Name}!");
            }
            if (tokens[position] is not T token)
            {
                throw new ArgumentException(messageOnUnexpectedToken);
            }
            position++;
            return token;
        }
    }

    public static RecursiveDescentParser Instance { get; private set; } = new();

    private RecursiveDescentParser() { }

    public ParserEntrypointNode? ParseTokensToCST(List<LeafNode> tokens)
    {
        if (tokens.Count == 0)
        {
            return null;
        }

        tokens = HangWhitespace(tokens);
        TokenReader reader = new(tokens);
        return new ParserEntrypointNode(ParseNamespaceDefinition(reader));
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



    private static TypeLeafNode ParseType(TokenReader reader)
        => reader.CurrentToken switch
        {
            IdentifierLeaf => reader.Consume<IdentifierLeaf>(),
            Int8Leaf => reader.Consume<Int8Leaf>(),
            Int16Leaf => reader.Consume<Int16Leaf>(),
            Int32Leaf => reader.Consume<Int32Leaf>(),
            Int64Leaf => reader.Consume<Int64Leaf>(),
            BoolLeaf => reader.Consume<BoolLeaf>(),
            _ => throw new ArgumentException($"Expected an identifier or primitive type, not {reader.CurrentToken}!")
        };


    private static VariableDefinitionNode ParseVariableDefinition(TokenReader reader)
    {
        reader.Consume(out LetKeywordLeaf letKeywordLeaf);
        VariableNameTypeNode varNameTypeNode = ParseVariableNameType(reader);
        reader.Consume(out AssignmentOperatorLeaf assignmentOpLeaf);
        SyntaxNode valueExpr = ParseExpression(reader);
        reader.Consume(out SemicolonLeaf semicolonLeaf);

        return new VariableDefinitionNode(letKeywordLeaf, varNameTypeNode, assignmentOpLeaf, valueExpr, semicolonLeaf);
    }

    private static VariableNameTypeNode ParseVariableNameType(TokenReader reader)
    {
        reader.Consume(out IdentifierLeaf idToken);
        reader.Consume(out ColonLeaf colonToken);
        TypeLeafNode typeNode = ParseType(reader)
            ?? throw new ArgumentException($"Expected a type after ':' token, not {reader.CurrentToken}!");

        return new VariableNameTypeNode(idToken, colonToken, typeNode);
    }

    private static SyntaxNode ParseExpression(TokenReader reader)
        => ParseExpression(reader, minPrecedence: 0);

    private static SyntaxNode ParseExpression(TokenReader reader, int minPrecedence)
    {
        SyntaxNode expr = ParsePrefix(reader);

        // look ahead one to check for operator + precedence
        while (!reader.IsAtEnd && TryGetInfixPrecedence(reader.CurrentToken, out int opPrecedence))
        {
            // led = left/infix denotation parsing (terminology from Pratt parsing)
            LeafNode opToken = reader.CurrentToken;
            bool isLeftAssociative = IsLeftAssociative(opToken);

            if (opPrecedence < minPrecedence)
            {
                break;
            }

            reader.Advance();

            SyntaxNode rhs = ParseExpression(
                reader,
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
    private static SyntaxNode ParsePrefix(TokenReader reader) => reader.CurrentToken switch
    {
        NotOperatorLeaf notToken => ParseNotOperator(notToken, reader),
        OpenParenthesisLeaf openToken => ParseParenthesizedExpression(openToken, reader),
        IdentifierLeaf idToken => ParseIdentifierOrFunctionCall(idToken, reader),
        IntLiteralLeaf intLiteralLeaf => ParseIntLiteral(intLiteralLeaf, reader),
        BoolLiteralLeaf boolLiteralLeaf => ParseBoolLiteral(boolLiteralLeaf, reader),
        _ => throw new ArgumentException($"Could not parse Primary Term, found {reader.CurrentToken}!")
    };

    private static IntLiteralLeaf ParseIntLiteral(IntLiteralLeaf intLiteralLeaf, TokenReader reader)
    {
        reader.Advance();
        return intLiteralLeaf;
    }
    private static BoolLiteralLeaf ParseBoolLiteral(BoolLiteralLeaf boolLiteralLeaf, TokenReader reader)
    {
        reader.Advance();
        return boolLiteralLeaf;
    }

    private static NotOperationNode ParseNotOperator(NotOperatorLeaf notToken, TokenReader reader)
    {
        reader.Advance(); // consume the '!' token
        SyntaxNode? notValueNode = ParseExpression(reader, minPrecedence: int.MaxValue)
            ?? throw new ArgumentException("Could not parse the expression after the '!' token!");

        NotOperationNode notOpNode = new([notToken, notValueNode]);
        notOpNode.UpdateRange();
        return notOpNode;
    }

    private static ParenthesizedExpression ParseParenthesizedExpression(OpenParenthesisLeaf openToken, TokenReader reader)
    {
        reader.Advance(); // consume the '(' token
        SyntaxNode? exprNode = ParseExpression(reader)
            ?? throw new ArgumentException("Could not parse the expression after the '(' token!");

        reader.Consume(out CloseParenthesisLeaf closeToken);

        ParenthesizedExpression parenthesizedExpr = new(openToken, exprNode, closeToken);
        parenthesizedExpr.UpdateRange();
        return parenthesizedExpr;
    }

    private static SyntaxNode ParseIdentifierOrFunctionCall(IdentifierLeaf idToken, TokenReader reader)
    {
        reader.Advance(); // consume the identifier token

        if (reader.IsAtEnd || reader.CurrentToken is not OpenParenthesisLeaf openParen)
        {
            return idToken;
        }

        reader.Advance(); // consume the '(' token
        return ParseFuncCallExpr(reader, openParen, idToken);
    }


    private static FunctionCallExpressionNode ParseFuncCallExpr(TokenReader reader, OpenParenthesisLeaf openParen, IdentifierLeaf idToken)
    {
        ArgumentListNode argumentList = ParseArgumentList(reader, openParen);
        return new FunctionCallExpressionNode(idToken, argumentList);
    }
    private static ArgumentListNode ParseArgumentList(TokenReader reader, OpenParenthesisLeaf openParen)
    {
        if (reader.CurrentToken is CloseParenthesisLeaf earlyCloseParen)
        {
            reader.Advance(); // consume the ')' token
            return new ArgumentListNode(openParen, earlyCloseParen);
        }

        List<SyntaxNode> arguments = [];
        while (true)
        {
            SyntaxNode arg = ParseExpression(reader);
            arguments.Add(arg);

            if (reader.CurrentToken is CommaLeaf commaToken)
            {
                reader.Advance(); // consume the ',' token
                arguments.Add(commaToken);
            }
            else if (reader.CurrentToken is CloseParenthesisLeaf closeParen)
            {
                reader.Advance(); // consume the ')' token
                return new ArgumentListNode(openParen, arguments, closeParen);
            }
            else
            {
                throw new ArgumentException($"Expected ',' or ')' in argument list, not {reader.CurrentToken}!");
            }
        }
    }


    private static FunctionDefinitionNode ParseFunctionDefinition(TokenReader reader)
    {
        reader.Consume(out FunctionKeywordLeaf funcKeywordLeaf);
        reader.Consume(out IdentifierLeaf idToken);
        ParameterListNode parameterList = ParseParameterList(reader);

        if (reader.CurrentToken is not SmallArrowLeaf arrowToken)
        {
            if (reader.CurrentToken is not OpenBraceLeaf)
            {
                throw new ArgumentException($"Expected '->' or '{{' token after function parameters, not {reader.CurrentToken}!");
            }

            FunctionBlockNode voidReturningBody = ParseFunctionBlock(reader);
            LeafNode currentToken = reader.CurrentToken;
            return new FunctionDefinitionNode(
                funcKeywordLeaf,
                idToken,
                parameterList,
                new ImplicitSmallArrowLeaf(currentToken.StartLine, currentToken.StartChar),
                new ImplicitVoidLeaf(currentToken.StartLine, currentToken.StartChar),
                voidReturningBody);
        }
        reader.Advance(); // consume the '->' token

        if (reader.CurrentToken is VoidLeaf)
        {
            reader.Consume(out VoidLeaf voidLeaf);
            FunctionBlockNode body = ParseFunctionBlock(reader);
            return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, voidLeaf, body);
        }
        else
        {
            TypeLeafNode returnType = ParseType(reader);
            FunctionBlockNode body = ParseFunctionBlock(reader);
            return new FunctionDefinitionNode(funcKeywordLeaf, idToken, parameterList, arrowToken, returnType, body);
        }
    }
    private static ParameterListNode ParseParameterList(TokenReader reader)
    {
        reader.Consume(out OpenParenthesisLeaf openParenToken);

        if (reader.CurrentToken is CloseParenthesisLeaf earlyCloseParenToken)
        {
            reader.Advance(); // consume the ')' token
            return new ParameterListNode(openParenToken, earlyCloseParenToken);
        }

        List<SyntaxNode> parameters = [];

        CloseParenthesisLeaf closeParenToken;
        while (true)
        {
            if (reader.CurrentToken is IdentifierLeaf)
            {
                VariableNameTypeNode nameType = ParseVariableNameType(reader);
                parameters.Add(nameType);
            }

            if (reader.CurrentToken is CommaLeaf comma)
            {
                reader.Advance(); // consume the ',' token
                parameters.Add(comma);
            }
            else if (reader.CurrentToken is CloseParenthesisLeaf closeParenCandidate)
            {
                closeParenToken = closeParenCandidate;
                reader.Advance(); // consume the ')' token
                break;
            }
            else
            {
                throw new ArgumentException($"Expected ',' or ')' in parameter list, not {reader.CurrentToken}!");
            }
        }
        return new ParameterListNode(openParenToken, parameters, closeParenToken);
    }

    private static WhileStatementNode ParseWhileStatement(TokenReader reader)
    {
        reader.Consume(out WhileKeywordLeaf whileKeywordLeaf);
        SyntaxNode condition = ParseExpression(reader);
        FunctionBlockNode body = ParseFunctionBlock(reader);

        return new WhileStatementNode(whileKeywordLeaf, condition, body);
    }

    private static NamespaceDefinitionNode ParseNamespaceDefinition(TokenReader reader)
    {
        reader.Consume(out NamespaceKeywordLeaf namespaceKeywordLeaf);
        QualifiedNameNode qualifiedName = ParseQualifiedName(reader);
        NonLocalBlockNode block = ParseHighLevelBlock(reader);

        return new NamespaceDefinitionNode(namespaceKeywordLeaf, qualifiedName, block);
    }
    private static QualifiedNameNode ParseQualifiedName(TokenReader reader)
    {
        reader.Consume(out IdentifierLeaf startIdLeaf);
        List<SyntaxNode> nameParts = [startIdLeaf];

        // contains ids and dots
        while (!reader.IsAtEnd && reader.CurrentToken is IdentifierLeaf or DotLeaf)
        {
            nameParts.Add(reader.Consume<LeafNode>());
        }
        return new QualifiedNameNode(nameParts);
    }

    private static FunctionBlockNode ParseFunctionBlock(TokenReader reader)
        => ParseBlock(reader, (open, statements, close) => new FunctionBlockNode(open, statements, close));
    private static LocalBlockNode ParseLocalBlock(TokenReader reader)
        => ParseBlock(reader, (open, statements, close) => new LocalBlockNode(open, statements, close));

    private static T ParseBlock<T>(
            TokenReader reader,
            Func<OpenBraceLeaf, List<SyntaxNode>, CloseBraceLeaf, T> factory)
        where T : BlockNode
    {
        reader.Consume(out OpenBraceLeaf openBraceToken);
        (List<SyntaxNode> statements, CloseBraceLeaf closeBraceLeaf) = ParseStatementsInBlock(reader);

        return factory.Invoke(openBraceToken, statements, closeBraceLeaf);
    }

    private static (List<SyntaxNode> statements, CloseBraceLeaf closeBrace) ParseStatementsInBlock(TokenReader reader)
    {
        List<SyntaxNode> statements = [];

        CloseBraceLeaf closeBraceLeaf;
        while (true)
        {
            if (reader.CurrentToken is LetKeywordLeaf)
            {
                statements.Add(ParseVariableDefinition(reader));
            }
            else if (reader.CurrentToken is WhileKeywordLeaf)
            {
                statements.Add(ParseWhileStatement(reader));
            }
            else if (reader.CurrentToken is IdentifierLeaf identifier)
            {
                reader.Advance(); // consume the identifier token
                if (reader.CurrentToken is OpenParenthesisLeaf openParen)
                {
                    reader.Advance(); // consume the '(' token
                    FunctionCallExpressionNode funcCallExpr = ParseFuncCallExpr(reader, openParen, identifier);

                    reader.Consume(out SemicolonLeaf semicolon, $"Expected ';' token after a function call, not {reader.CurrentToken}!");
                    
                    statements.Add(new FunctionCallStatementNode(funcCallExpr, semicolon));
                }
                else if (reader.CurrentToken is AssignmentOperatorLeaf assignmentOp)
                {
                    reader.Advance(); // consume the assignment operator token
                    SyntaxNode assignedValue = ParseExpression(reader);

                    reader.Consume(out SemicolonLeaf semicolon, $"Expected ';' token at the end of assignment statement, not {reader.CurrentToken}!");
                    
                    statements.Add(new AssignmentStatementNode(identifier, assignmentOp, assignedValue, semicolon));
                }
                else
                {
                    throw new ArgumentException($"Unexpected token after identifier in statement: {reader.CurrentToken}!");
                }
            }
            else if (reader.CurrentToken is ReturnKeywordLeaf returnKeyword)
            {
                reader.Advance(); // consume the 'return' token
                if (reader.CurrentToken is SemicolonLeaf earlySemicolon)
                {
                    reader.Advance(); // consume the ';' token
                    statements.Add(new ReturnStatementNode(returnKeyword, earlySemicolon));
                    continue;
                }

                SyntaxNode returnValue = ParseExpression(reader);

                reader.Consume(out SemicolonLeaf semicolonLeaf, $"Expected ';' token at the end of return statement, not {reader.CurrentToken}!");

                statements.Add(new ReturnStatementNode(returnKeyword, returnValue, semicolonLeaf));
            }
            else if (reader.CurrentToken is OpenBraceLeaf)
            {
                statements.Add(ParseLocalBlock(reader));
            }
            else if (reader.CurrentToken is SemicolonLeaf semicolon)
            {
                reader.Consume<SemicolonLeaf>();
                statements.Add(new EmptyStatementNode(semicolon));
            }
            else if (reader.CurrentToken is CloseBraceLeaf leaf)
            {
                reader.Consume<CloseBraceLeaf>();
                closeBraceLeaf = leaf;
                break;
            }
            else
            {
                throw new ArgumentException($"Unexpected token in block: {reader.CurrentToken}!");
            }
        }
        return (statements, closeBraceLeaf);
    }

    private static NonLocalBlockNode ParseHighLevelBlock(TokenReader reader)
    {
        reader.Consume(out OpenBraceLeaf openBraceToken);
        List<SyntaxNode> statements = [];

        while (true)
        {
            if (reader.CurrentToken is FunctionKeywordLeaf)
            {
                statements.Add(ParseFunctionDefinition(reader));
            }
            else if (reader.CurrentToken is NamespaceKeywordLeaf)
            {
                statements.Add(ParseNamespaceDefinition(reader));
            }
            else if (reader.CurrentToken is LetKeywordLeaf)
            {
                statements.Add(ParseVariableDefinition(reader));
            }
            else if (reader.CurrentToken is CloseBraceLeaf)
            {
                reader.Consume(out CloseBraceLeaf closeBraceLeaf);
                return new NonLocalBlockNode(openBraceToken, statements, closeBraceLeaf);
            }
            else
            {
                throw new ArgumentException($"Unexpected token in block: {reader.CurrentToken}!");
            }
        }
    }
}