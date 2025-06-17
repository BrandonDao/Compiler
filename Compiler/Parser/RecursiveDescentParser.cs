using Compiler.Lexer;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Identifiers;
using Compiler.Parser.Nodes.Keywords;
using Compiler.Parser.Nodes.Operators;
using Compiler.Parser.Nodes.Primitives;
using Compiler.Parser.Nodes.Punctuation;
using Compiler.Parser.Nodes.Whitespace;
using System.Diagnostics.CodeAnalysis;

using Boolean = Compiler.Parser.Nodes.Primitives.Boolean;
using Int16 = Compiler.Parser.Nodes.Primitives.Int16;
using Int32 = Compiler.Parser.Nodes.Primitives.Int32;
using Int64 = Compiler.Parser.Nodes.Primitives.Int64;

namespace Compiler.Parser
{
    public class RecursiveDescentParser(Token[] tokens)
    {
        private readonly Token[] tokens = tokens;
        private uint position = 0;

        private uint ParseWhitespace(List<SyntaxNode> children, bool isLeading)
        {
            if (Match(TokenType.Whitespace, out Token? token))
            {
                children.Add(new Whitespace(token, isLeading));
                return 1;
            }
            return 0;
        }

        public void ParseAliasDirective(ProgramNode program)
        {
            if (!Match(TokenType.Alias, out Token? usingToken)) return;

            AliasKeyword usingKeyword = new(usingToken);
            ParseWhitespace(usingKeyword.Children, false);

            if (!Match(TokenType.Identifier, out Token? aliasIdentifierToken))
                throw new InvalidOperationException("Expected identifier after 'alias' keyword!");

            IdentifierName aliasIdentifier = new(aliasIdentifierToken);
            ParseWhitespace(aliasIdentifier.Children, false);
            aliasIdentifier.UpdateRange();

            if (!Match(TokenType.AssignmentOperator, out Token? assignmentOpToken))
                throw new InvalidOperationException("Expected assignment operator '=' in alias directive!");

            AssignmentOperator assignmentOperator = new(assignmentOpToken);
            ParseWhitespace(assignmentOperator.Children, false);
            assignmentOperator.UpdateRange();

            ParentNode originalTypeOrIdentifier;
            if (Match(TokenType.Identifier, out Token? originalIdentifierToken))
            {
                originalTypeOrIdentifier = new IdentifierName(originalIdentifierToken);
            }
            else if (Match(TokenType.Int8, out originalIdentifierToken))
            {
                originalTypeOrIdentifier = new Int8(originalIdentifierToken);
            }
            else if (Match(TokenType.Int16, out originalIdentifierToken))
            {
                originalTypeOrIdentifier = new Int16(originalIdentifierToken);
            }
            else if (Match(TokenType.Int32, out originalIdentifierToken))
            {
                originalTypeOrIdentifier = new Int32(originalIdentifierToken);
            }
            else if (Match(TokenType.Int64, out originalIdentifierToken))
            {
                originalTypeOrIdentifier = new Int64(originalIdentifierToken);
            }
            else if (Match(TokenType.Boolean, out originalIdentifierToken))
            {
                originalTypeOrIdentifier = new Boolean(originalIdentifierToken);
            }
            else throw new InvalidOperationException("Expected identifier after assignment operator '=' in alias directive!");

            ParseWhitespace(originalTypeOrIdentifier.Children, false);

            if (!Match(TokenType.Semicolon, out Token? semicolonToken))
                throw new InvalidOperationException("Expected semicolon ';' at the end of alias directive!");

            Semicolon semicolon = new(semicolonToken);
            ParseWhitespace(semicolon.Children, false);
            semicolon.UpdateRange();

            AliasDirective aliasDirective = new(aliasIdentifier.Name, originalIdentifierToken.Value);
            aliasDirective.Children.Add(usingKeyword);
            aliasDirective.Children.Add(aliasIdentifier);
            aliasDirective.Children.Add(assignmentOperator);
            aliasDirective.Children.Add(originalTypeOrIdentifier);
            aliasDirective.Children.Add(semicolon);
            aliasDirective.UpdateRange();

            program.Children.Add(aliasDirective);
        }

        // Utility methods
        private bool Match(TokenType type, [NotNullWhen(true)] out Token? token)
        {
            if (Check(type))
            {
                token = Advance();
                return true;
            }
            token = null;
            return false;
        }
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;

            return tokens[position].Type == type;
        }
        private Token Advance()
        {
            if (!IsAtEnd()) position++;

            return tokens[position - 1];
        }
        private bool IsAtEnd() => position >= tokens.Length;
    }
}
