using System.Text.RegularExpressions;

namespace Compiler.Lexer
{
    public class RegexLexer : ILexer
    {
        private static readonly TokenDefinition[] tokenDefinitions =
        [
            new(0, TokenType.Int8, @"\b(Int8)\b"),
            new(0, TokenType.Int16, @"\b(Int16)\b"),
            new(0, TokenType.Int32, @"\b(Int32)\b"),
            new(0, TokenType.Int64, @"\b(Int64)\b"),
            new(0, TokenType.Boolean, @"\b(Boolean)\b"),
            new(0, TokenType.Void, @"\b(void)\b"),

            new(5, TokenType.EqualityOperator, @"(==)"),
            new(5, TokenType.DotOperator, @"(\.)"),
            new(5, TokenType.ModulusOperator, @"(%)"),
            new(5, TokenType.MinusSign, @"(-)"),
            new(5, TokenType.PlusSign, @"(\+)"),
            new(5, TokenType.MultiplySign, @"(\*)"),
            new(5, TokenType.DivideSign, @"(/)"),

            new(7, TokenType.AssignmentOperator, @"(=)"),

            new(10, TokenType.Using, @"\b(using)\b"),
            new(10, TokenType.While, @"\b(while)\b"),
            new(10, TokenType.Func, @"\b(func)\b"),
            new(10, TokenType.Return, @"\b(return)\b"),
            new(10, TokenType.IfStatement, @"\b(if)\b"),
            new(10, TokenType.ElseStatement, @"\b(else)\b"),
            new(10, TokenType.Whitespace, @"([ \t\r\n]+)"),
            new(15, TokenType.IntegerLiteral, @"\b(\d+)\b"),
            new(15, TokenType.BooleanLiteral, @"\b(true|false)\b"),

            new(30, TokenType.Semicolon, @"(;)"),
            new(30, TokenType.Comma, @"(,)"),
            new(30, TokenType.OpenBrace, @"(\{)"),
            new(30, TokenType.CloseBrace, @"(\})"),
            new(30, TokenType.OpenParenthesis, @"(\()"),
            new(30, TokenType.CloseParenthesis, @"(\))"),
            new(30, TokenType.OpenSquareBracket, @"(\[)"),
            new(30, TokenType.CloseSquareBracket, @"(\])"),
            new(30, TokenType.OpenAngleBracket, @"(<)"),
            new(30, TokenType.CloseAngleBracket, @"(>)"),

            new(40, TokenType.Identifier, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b"),

            new(uint.MaxValue, TokenType.Undefined, @"(.)")
        ];

        public List<Token> TokenizeFile(string filePath, ILexer.OnUnexpectedTokenHandler? onUnexpectedToken = null)
        {
            string[] lines = File.ReadAllLines(filePath);
            return Tokenize(lines, onUnexpectedToken);
        }

        public List<Token> Tokenize(string[] lines, ILexer.OnUnexpectedTokenHandler? onUnexpectedToken = null)
        {
            var orderedDefinitions = tokenDefinitions.OrderBy(d => d.Priority);
#if DEBUG
            if (!orderedDefinitions.Any()) throw new InvalidOperationException("No token definitions available!");

            var missing = Enum.GetValues<TokenType>().Except(tokenDefinitions.Select(def => def.Type)).Where(type => !Enum.GetName(type)!.Contains("Flag"));
            if (missing.Any()) throw new InvalidOperationException($"Some token types are missing definitions: {string.Join(", ", missing)}");
#endif

            List<Token> tokens = [];

            for (uint lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                uint charIdx = 0;

                while (charIdx < lines[lineIdx].Length)
                {
                    TokenDefinition? matchedDef = null;
                    Match match = Match.Empty;

                    foreach (var def in orderedDefinitions)
                    {
                        match = def.Regex.Match(lines[lineIdx], (int)charIdx);

                        if (!match.Success || match.Index != charIdx) continue;

                        matchedDef = def;
                        break;
                    }

                    if (onUnexpectedToken != null && matchedDef!.Type == TokenType.Undefined)
                    {
                        onUnexpectedToken(lineIdx, charIdx, match.Value);
                    }

                    tokens.Add(new Token(matchedDef!.Type, match, lineIdx, charIdx));
                    charIdx += (uint)match.Length;
                }
            }

            return tokens;
        }
    }
}