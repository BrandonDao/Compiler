using System.Text;
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
            return ParseNamespaceDefinition(tokens, ref position);
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

                var voidReturningBody = ParseStatementBlock(tokens, ref position);
                return new FunctionDefinitionNode(
                    funcKeywordLeaf,
                    idToken,
                    parameterList,
                    new ImplicitSmallArrowLeaf(tokens[position].StartLine, tokens[position].StartChar),
                    new ImplicitVoidLeaf(tokens[position].StartLine, tokens[position].StartChar),
                    voidReturningBody);
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

        private static BlockNode ParseStatementBlock(List<LeafNode> tokens, ref int position)
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
            return new BlockNode(openBraceToken, statements, closeBraceLeaf);
        }
        private static BlockNode ParseHighLevelBlock(List<LeafNode> tokens, ref int position)
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
                else if(tokens[position] is LetKeywordLeaf)
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
            return new BlockNode(openBraceToken, statements, closeBraceLeaf);
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

        public class QualifiedNameNode : SyntaxNode
        {
            public string Name { get; set; }
            private readonly List<SyntaxNode> trimmedChildren;

            public QualifiedNameNode(List<SyntaxNode> children) : base(children)
            {
                trimmedChildren = new List<SyntaxNode>(capacity: children.Count);

                if (children.Count == 0 || children[0] is not IdentifierLeaf startIdLeaf)
                    throw new ArgumentException("QualifiedNameNode must start with an IdentifierLeaf!");

                StringBuilder nameBuilder = new(startIdLeaf.Value);
                for (int i = 1; i < children.Count; i++)
                {
                    SyntaxNode? child = children[i];
                    if (child is IdentifierLeaf idLeaf)
                    {
                        nameBuilder.Append('.');
                        nameBuilder.Append(idLeaf.Value);
                        trimmedChildren.Add(idLeaf);
                    }
                }
                Name = nameBuilder.ToString();
            }

            public override SyntaxNode ToAST()
            {
                Children = trimmedChildren;
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i] = Children[i].ToAST();
                }
                return this;
            }

            public override string GetPrintable(int indent = 0)
            {
                var indentString = new string(' ', indent);
                StringBuilder builder = new();
                builder.Append($"[{StartLine}.{StartChar} - {EndLine}.{EndChar}]\t{indentString}{GetType().Name} Name: {Name}\n");
                foreach (var child in Children)
                {
                    builder.Append(child.GetPrintable(indent + 4));
                }
                return builder.ToString();
            }
        }
        public class NamespaceDefinitionNode(NamespaceKeywordLeaf namespaceKeyword, QualifiedNameNode name, BlockNode block)
            : SyntaxNode([namespaceKeyword, name, block])
        {
            public override SyntaxNode ToAST()
            {
                Children.RemoveAt(0); // Remove the namespace keyword
                Children[0] = Children[0].ToAST(); // Convert the qualified name to AST
                Children[1] = Children[1].ToAST(); // Convert the block to AST
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
            public FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SyntaxNode arrow, SyntaxNode returnType, BlockNode body)
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
            public WhileStatementNode(WhileKeywordLeaf whileKeyword, SyntaxNode condition, BlockNode body)
                : base([whileKeyword, condition, body])
                => UpdateRange();

            public override SyntaxNode ToAST()
            {
                Children.RemoveAt(0); // Remove the while keyword
                Children[1] = Children[1].ToAST(); // Convert the condition to AST
                return this;
            }
        }

        public class BlockNode : SyntaxNode
        {
            public BlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
                : base([openBrace, .. statements, closeBrace])
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
