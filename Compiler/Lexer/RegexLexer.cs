using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Compiler.Lexer
{
    public class RegexLexer : ILexer
    {
        private static readonly TokenDefinition[] tokenDefinitions =
        [
            new TokenDefinition(0, TokenType.Int8, @"\b(Int8)\b"),
            new TokenDefinition(0, TokenType.Int16, @"\b(Int16)\b"),
            new TokenDefinition(0, TokenType.Int32, @"\b(Int32)\b"),
            new TokenDefinition(0, TokenType.Int64, @"\b(Int64)\b"),
            new TokenDefinition(0, TokenType.Boolean, @"\b(Boolean)\b"),
            new TokenDefinition(0, TokenType.Void, @"\b(void)\b"),
            new TokenDefinition(5, TokenType.EqualityOperator, @"(==)"),
            new TokenDefinition(5, TokenType.DotOperator, @"(\.)"),
            new TokenDefinition(5, TokenType.MinusSign, @"(-)"),
            new TokenDefinition(5, TokenType.PlusSign, @"(\+)"),
            new TokenDefinition(5, TokenType.MultiplySign, @"(\*)"),
            new TokenDefinition(5, TokenType.DivideSign, @"(/)"),
            new TokenDefinition(7, TokenType.AssignmentOperator, @"(=)"),
            new TokenDefinition(10, TokenType.Using, @"(using)"),
            new TokenDefinition(10, TokenType.While, @"(while)"),
            new TokenDefinition(10, TokenType.Func, @"\b(func)\b"),
            new TokenDefinition(10, TokenType.Return, @"(return)"),
            new TokenDefinition(10, TokenType.Whitespace, @"([ \t\r\n]+)"),
            new TokenDefinition(15, TokenType.IntegerLiteral, @"\b(\d+)\b"),
            new TokenDefinition(15, TokenType.BooleanLiteral, @"\b(true|false)\b"),
            new TokenDefinition(30, TokenType.Semicolon, @"(;)"),
            new TokenDefinition(30, TokenType.Comma, @"(,)"),
            new TokenDefinition(30, TokenType.LeftBrace, @"(\{)"),
            new TokenDefinition(30, TokenType.RightBrace, @"(\})"),
            new TokenDefinition(30, TokenType.LeftParenthesis, @"(\()"),
            new TokenDefinition(30, TokenType.RightParenthesis, @"(\))"),
            new TokenDefinition(30, TokenType.LeftSquareBracket, @"(\[)"),
            new TokenDefinition(30, TokenType.RightSquareBracket, @"(\])"),
            new TokenDefinition(30, TokenType.LeftAngleBracket, @"(<)"),
            new TokenDefinition(30, TokenType.RightAngleBracket, @"(>)"),
            new TokenDefinition(40, TokenType.Identifier, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b"),
        ];

        public List<Token> TokenizeFile(string filePath)
        {
            string text = File.ReadAllText(filePath);
            return Tokenize(text);
        }

        public List<Token> Tokenize(string text)
        {
            var orderedDefinitions = tokenDefinitions.OrderBy(d => d.Priority);
            if (!orderedDefinitions.Any()) throw new InvalidOperationException("No token definitions available!");

            var missing = Enum.GetValues<TokenType>().Except(tokenDefinitions.Select(def => def.Type));
            if (missing.Any()) throw new InvalidOperationException($"Some token types are missing definitions: {string.Join(", ", missing)}");

            List<Token> tokens = [];
            uint position = 0;

            while (position < text.Length)
            {
                TokenDefinition? matchedDef = null;
                Match match = Match.Empty;

                foreach (var def in orderedDefinitions)
                {
                    match = def.Regex.Match(text, (int)position);

                    if (!match.Success || match.Index != position) continue;

                    matchedDef = def;
                    break;
                }

                if (matchedDef == null || !match.Success) throw new InvalidOperationException($"Unexpected character at position {position}: '{text[(int)position]}'!");

                tokens.Add(new Token(matchedDef.Type, match, position));
                position += (uint)match.Length;
            }
            return tokens;
        }
    }
}