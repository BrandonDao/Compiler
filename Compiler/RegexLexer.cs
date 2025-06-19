using System.Text.RegularExpressions;
using System.Reflection;
using CompilerLib.Lexer;
using CompilerLib.Parser.Nodes.Primitives;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Literals;
using CompilerLib.Parser.Nodes.Punctuation;
using CompilerLib.Parser.Nodes.Identifiers;
using Bool = CompilerLib.Parser.Nodes.Primitives.Bool;
using Int16 = CompilerLib.Parser.Nodes.Primitives.Int16;
using Int32 = CompilerLib.Parser.Nodes.Primitives.Int32;
using Int64 = CompilerLib.Parser.Nodes.Primitives.Int64;
using CompilerLib.Parser.Nodes;
using System.Text;

namespace Compiler
{
    public class RegexLexer : ILexer
    {
        private static readonly TokenDefinition[] tokenDefinitions =
        [
            new(0, @"\b(int8)\b", (v, sl, sc, el, ec) => new Int8(v, sl, sc, el, ec)),
            new(0, @"\b(int16)\b", (v, sl, sc, el, ec) => new Int16(v, sl, sc, el, ec)),
            new(0, @"\b(int32)\b", (v, sl, sc, el, ec) => new Int32(v, sl, sc, el, ec)),
            new(0, @"\b(int64)\b", (v, sl, sc, el, ec) => new Int64(v, sl, sc, el, ec)),
            new(0, @"\b(bool)\b", (v, sl, sc, el, ec) => new Bool(v, sl, sc, el, ec)),

            new(5, @"(==)", (v, sl, sc, el, ec) => new EqualityOperator(v, sl, sc, el, ec)),
            // new(5, TokenType.DotOperator, @"(\.)"),
            // new(5, TokenType.ModulusOperator, @"(%)"),
            new(5, @"(-)", (v, sl, sc, el, ec) => new NegateOperator(v, sl, sc, el, ec)),
            new(5, @"(\+)", (v, sl, sc, el, ec) => new AddOperator(v, sl, sc, el, ec)),
            new(5, @"(\*)", (v, sl, sc, el, ec) => new MultiplyOperator(v, sl, sc, el, ec)),
            new(5, @"(/)", (v, sl, sc, el, ec) => new DivideOperator(v, sl, sc, el, ec)),
            new(5, @"(%)", (v, sl, sc, el, ec) => new ModOperator(v, sl, sc, el, ec)),
            new(5, @"(\|)", (v, sl, sc, el, ec) => new OrOperator(v, sl, sc, el, ec)),
            new(5, @"(&)", (v, sl, sc, el, ec) => new AndOperator(v, sl, sc, el, ec)),

            new(7, @"(=)", (v, sl, sc, el, ec) => new AssignmentOperator(v, sl, sc, el, ec)),

            // new(10, TokenType.Alias, @"\b(alias)\b"),
            // new(10, TokenType.While, @"\b(while)\b"),
            // new(10, TokenType.Func, @"\b(func)\b"),
            // new(10, TokenType.Void, @"\b(void)\b"),
            // new(10, TokenType.Return, @"\b(return)\b"),
            // new(10, TokenType.IfStatement, @"\b(if)\b"),
            // new(10, TokenType.ElseStatement, @"\b(else)\b"),
            new(15, @"\b(\d+)\b", (v, sl, sc, el, ec) => new IntLiteral(v, sl, sc, el, ec)),
            new(15, @"\b(true|false)\b", (v, sl, sc, el, ec) => new BoolLiteral(v, sl, sc, el, ec)),

            new(30, @"([ \t\r\n]+)", (v, sl, sc, el, ec) => new Whitespace(v, sl, sc, el, ec)),
            new(30, @"(;)", (v, sl, sc, el, ec) => new Semicolon(v, sl, sc, el, ec)),
            new(30, @"(,)", (v, sl, sc, el, ec) => new Comma(v, sl, sc, el, ec)),
            new(30, @"(\{)", (v, sl, sc, el, ec) => new OpenBrace(v, sl, sc, el, ec)),
            new(30, @"(\})", (v, sl, sc, el, ec) => new CloseBrace(v, sl, sc, el, ec)),
            new(30, @"(\()", (v, sl, sc, el, ec) => new OpenParenthesis(v, sl, sc, el, ec)),
            new(30, @"(\))", (v, sl, sc, el, ec) => new CloseParenthesis(v, sl, sc, el, ec)),
            new(30, @"(\[)", (v, sl, sc, el, ec) => new OpenSquareBracket(v, sl, sc, el, ec)),
            new(30, @"(\])", (v, sl, sc, el, ec) => new CloseSquareBracket(v, sl, sc, el, ec)),
            new(30, @"(<)", (v, sl, sc, el, ec) => new OpenAngleBracket(v, sl, sc, el, ec)),
            new(30, @"(>)", (v, sl, sc, el, ec) => new CloseAngleBracket(v, sl, sc, el, ec)),

            new(40, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b", (v, sl, sc, el, ec) => new IdentifierName(v, sl, sc, el, ec))
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

            var LeafNodeType = typeof(LeafNode);
            var nodeTypes = (
                    Assembly.GetAssembly(LeafNodeType)?
                            .GetTypes()
                            .Where(t => t.IsSubclassOf(LeafNodeType) && !t.IsAbstract))
                ?? throw new InvalidOperationException($"Could not find any classes deriving type of {LeafNodeType}");

            var missing = nodeTypes.Except(tokenDefinitions.Select(def => def.NodeFactory(string.Empty, 0, 0, 0, 0).GetType()));

            if (missing.Any()) throw new InvalidOperationException($"Some token types are missing definitions: {string.Join(", ", missing)}");
#endif

            StringBuilder errorMessageBuilder = new("Unexpected characters found!\n");
            bool errorFlag = false;
            List<LeafNode> tokens = [];
            StringBuilder line = new();

            for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                line.Clear();
                line.Append(lines[lineIdx]);
                line.Append('\n');

                int charIdx = 0;

                while (charIdx < line.Length)
                {
                    TokenDefinition? matchedDef = null;
                    Match match = Match.Empty;

                    foreach (var def in orderedDefinitions)
                    {
                        match = def.Regex.Match(line.ToString(), charIdx);

                        if (!match.Success || match.Index != charIdx) continue;

                        matchedDef = def;
                        break;
                    }

                    if (onUnexpectedToken != null && !match.Success)
                    {
                        errorFlag = true;
                        onUnexpectedToken(lineIdx, charIdx, match.Value);
                        errorMessageBuilder.Append($"Line {lineIdx}\t'{line[charIdx]}'\n");
                    }

                    LeafNode node = matchedDef!.NodeFactory.Invoke(
                        value: match.Value,
                        startLine: lineIdx,
                        startChar: charIdx + match.Index,
                        endLine: lineIdx,
                        endChar: charIdx + match.Index + match.Length);
                    
                    tokens.Add(node);

                    charIdx += match.Length;
                }
            }

            if (errorFlag)
            {
                throw new ArgumentException(errorMessageBuilder.ToString());
            }

            return tokens;
        }

        public static string Detokenize(List<LeafNode> tokens)
        {
            StringBuilder builder = new();
            foreach (var token in tokens)
            {
                builder.Append(token.Value);
            }
            return builder.ToString();
        }
    }
}