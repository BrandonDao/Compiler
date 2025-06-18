using System.Text.RegularExpressions;
using Compiler.Shared.Parser.Nodes.Primitives;
using Compiler.Shared.Parser.Nodes.Operators;
using Boolean = Compiler.Shared.Parser.Nodes.Primitives.Boolean;
using Int16 = Compiler.Shared.Parser.Nodes.Primitives.Int16;
using Int32 = Compiler.Shared.Parser.Nodes.Primitives.Int32;
using Int64 = Compiler.Shared.Parser.Nodes.Primitives.Int64;
using Compiler.Parser.Nodes;
using Compiler.Shared.Parser.Nodes.Punctuation;
using Compiler.Shared.Parser.Nodes.Identifiers;
using Compiler.Shared.Parser.Nodes;

namespace Compiler.Lexer
{
    public class RegexLexer : ILexer
    {
        private static readonly TokenDefinition[] tokenDefinitions =
        [
            new(0, TokenType.Int8, @"\b(int8)\b", (v, sl, sc, el, ec) => new Int8(v, sl, sc, el, ec)),
            new(0, TokenType.Int16, @"\b(int16)\b", (v, sl, sc, el, ec) => new Int16(v, sl, sc, el, ec)),
            new(0, TokenType.Int32, @"\b(int32)\b", (v, sl, sc, el, ec) => new Int32(v, sl, sc, el, ec)),
            new(0, TokenType.Int64, @"\b(int64)\b", (v, sl, sc, el, ec) => new Int64(v, sl, sc, el, ec)),
            new(0, TokenType.Boolean, @"\b(boolean)\b", (v, sl, sc, el, ec) => new Boolean(v, sl, sc, el, ec)),

            // new(5, TokenType.EqualityOperator, @"(==)"),
            // new(5, TokenType.DotOperator, @"(\.)"),
            // new(5, TokenType.ModulusOperator, @"(%)"),
            new(5, TokenType.MinusSign, @"(-)", (v, sl, sc, el, ec) => new NegateOperator(v, sl, sc, el, ec)),
            new(5, TokenType.PlusSign, @"(\+)", (v, sl, sc, el, ec) => new AddOperator(v, sl, sc, el, ec)),
            new(5, TokenType.MultiplySign, @"(\*)", (v, sl, sc, el, ec) => new MultiplyOperator(v, sl, sc, el, ec)),
            new(5, TokenType.DivideSign, @"(/)", (v, sl, sc, el, ec) => new DivideOperator(v, sl, sc, el, ec)),

            new(7, TokenType.AssignmentOperator, @"(=)", (v, sl, sc, el, ec) => new AssignmentOperator(v, sl, sc, el, ec)),

            // new(10, TokenType.Alias, @"\b(alias)\b"),
            // new(10, TokenType.While, @"\b(while)\b"),
            // new(10, TokenType.Func, @"\b(func)\b"),
            // new(10, TokenType.Void, @"\b(void)\b"),
            // new(10, TokenType.Return, @"\b(return)\b"),
            // new(10, TokenType.IfStatement, @"\b(if)\b"),
            // new(10, TokenType.ElseStatement, @"\b(else)\b"),
            new(15, TokenType.IntegerLiteral, @"\b(\d+)\b"),
            new(15, TokenType.BooleanLiteral, @"\b(true|false)\b"),

            new(30, TokenType.Whitespace, @"([ \t\r\n]+)"),
            new(30, TokenType.Semicolon, @"(;)", (v, sl, sc, el, ec) => new Semicolon(v, sl, sc, el, ec)),
            new(30, TokenType.Comma, @"(,)", (v, sl, sc, el, ec) => new Comma(v, sl, sc, el, ec)),
            new(30, TokenType.OpenBrace, @"(\{)", (v, sl, sc, el, ec) => new OpenBrace(v, sl, sc, el, ec)),
            new(30, TokenType.CloseBrace, @"(\})", (v, sl, sc, el, ec) => new CloseBrace(v, sl, sc, el, ec)),
            new(30, TokenType.OpenParenthesis, @"(\()", (v, sl, sc, el, ec) => new OpenParenthesis(v, sl, sc, el, ec)),
            new(30, TokenType.CloseParenthesis, @"(\))", (v, sl, sc, el, ec) => new CloseParenthesis(v, sl, sc, el, ec)),
            new(30, TokenType.OpenSquareBracket, @"(\[)", (v, sl, sc, el, ec) => new OpenSquareBracket(v, sl, sc, el, ec)),
            new(30, TokenType.CloseSquareBracket, @"(\])", (v, sl, sc, el, ec) => new CloseSquareBracket(v, sl, sc, el, ec)),
            new(30, TokenType.OpenAngleBracket, @"(<)", (v, sl, sc, el, ec) => new OpenAngleBracket(v, sl, sc, el, ec)),
            new(30, TokenType.CloseAngleBracket, @"(>)", (v, sl, sc, el, ec) => new CloseAngleBracket(v, sl, sc, el, ec)),

            new(40, TokenType.Identifier, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b", (v, sl, sc, el, ec) => new IdentifierName(v, sl, sc, el, ec)),

            new(uint.MaxValue, TokenType.Undefined, @"(.)", (v, sl, sc, el, ec) => new Undefined(v, sl, sc, el, ec))
        ];

        public List<LeafNode> TokenizeFile(string filePath, ILexer.OnUnexpectedTokenHandler? onUnexpectedToken = null)
        {
            string[] lines = File.ReadAllLines(filePath);
            return Tokenize(lines, onUnexpectedToken);
        }

        public List<LeafNode> Tokenize(string[] lines, ILexer.OnUnexpectedTokenHandler? onUnexpectedToken = null)
        {
            var orderedDefinitions = tokenDefinitions.OrderBy(d => d.Priority);
#if DEBUG
            if (!orderedDefinitions.Any()) throw new InvalidOperationException("No token definitions available!");

            var missing = Enum.GetValues<TokenType>().Except(tokenDefinitions.Select(def => def.Type)).Where(type => !Enum.GetName(type)!.Contains("Flag"));
            if (missing.Any()) throw new InvalidOperationException($"Some token types are missing definitions: {string.Join(", ", missing)}");
#endif

            List<LeafNode> tokens = [];

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

                    tokens.Add(matchedDef!.NodeFactory.Invoke(
                        match.Value,
                        lineIdx,
                        (uint)(charIdx + match.Index),
                        lineIdx,
                        (uint)(charIdx + match.Index + match.Length)));

                    charIdx += (uint)match.Length;
                }
            }

            return tokens;
        }
    }
}