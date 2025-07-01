using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Functions;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Punctuation;
using CompilerLib.Parser.Nodes.Scopes;
using CompilerLib.Parser.Nodes.Statements;
using CompilerLib.Parser.Nodes.Statements.Controls;
using CompilerLib.Parser.Nodes.Types;

namespace Compiler.Parser
{
    public partial class RecursiveDescentParser : IParser
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

            return FixAssociativity(rest);

            SyntaxNode ParseHighValueExpr(ref int position)
            {
                var lhs = ParseValueTerm(ref position);
                var rest = ParseHighValueExprRest(ref position);
                if (rest is EpsilonNode) return lhs;

                rest.Children[0] = lhs;

                return FixAssociativity(rest);

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

                        if (tokens[position] is OpenParenthesisLeaf openParen)
                        {
                            position++;
                            var funcCall = ParseFuncCallExpr(tokens, ref position, openParen, idToken);
                            return funcCall;
                        }
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
            SyntaxNode FixAssociativity(SyntaxNode node)
            {
                if ((node is LowPrecedenceOperationNode && node.Children.Count > 1 && node.Children[2] is LowPrecedenceOperationNode)
                || (node is HighPrecedenceOperationNode && node.Children.Count > 1 && node.Children[2] is HighPrecedenceOperationNode))
                {
                    var newRoot = node.Children[2];
                    node.Children[2] = newRoot.Children[0];
                    newRoot.Children[0] = node;
                    node.UpdateRange();
                    newRoot.UpdateRange();
                    node = newRoot;

                    node.Children[0] = FixAssociativity(node.Children[0]);
                }
                node.UpdateRange();
                return node;
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
                var arg = ParseValueExpression(tokens, ref position);
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
            var condition = ParseValueExpression(tokens, ref position);

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
                        SyntaxNode assignedValue = ParseValueExpression(tokens, ref position);

                        if (tokens[position] is not SemicolonLeaf semicolon)
                            throw new ArgumentException($"Expected ';' token at the end of assignment statement, not {tokens[position]}!");

                        position++;
                        statements.Add(new AssignmentStatementNode(identifier, assignmentOp, assignedValue, semicolon));
                    }
                    else throw new ArgumentException($"Unexpected token after identifier in statement: {tokens[position]}!");
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