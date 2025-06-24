using System.Text.RegularExpressions;
using System.Reflection;
using CompilerLib.Lexer;
using CompilerLib.Parser.Nodes;
using System.Text;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Punctuation;

namespace Compiler
{
    public class RegexLexer : ILexer
    {
        private static readonly TokenDefinition[] tokenDefinitions =
        [
            new(LexPriority.PrimitiveOrKeyword, @"\b(int8|i8)\b", (v, sl, sc, el, ec) => new Int8Leaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimitiveOrKeyword, @"\b(int16|i16)\b", (v, sl, sc, el, ec) => new Int16Leaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimitiveOrKeyword, @"\b(int32|i32)\b", (v, sl, sc, el, ec) => new Int32Leaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimitiveOrKeyword, @"\b(int64|i64)\b", (v, sl, sc, el, ec) => new Int64Leaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimitiveOrKeyword, @"\bbool\b", (v, sl, sc, el, ec) => new BoolLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimitiveOrKeyword, @"\bvoid\b", (v, sl, sc, el, ec) => new VoidLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimitiveOrKeyword, @"\blet\b", (v, sl, sc, el, ec) => new LetKeywordLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimitiveOrKeyword, @"\bwhile\b", (v, sl, sc, el, ec) => new WhileKeywordLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimitiveOrKeyword, @"\b(func|fn)\b", (v, sl, sc, el, ec) => new FunctionKeywordLeaf(v, sl, sc, el, ec)),

            new(LexPriority.PrimaryPunctuation, @";", (v, sl, sc, el, ec) => new SemicolonLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryPunctuation, @":", (v, sl, sc, el, ec) => new ColonLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryPunctuation, @"->", (v, sl, sc, el, ec) => new SmallArrowLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryPunctuation, @",", (v, sl, sc, el, ec) => new CommaLeaf(v, sl, sc, el, ec)),

            new(LexPriority.PrimaryOperator, @"==", (v, sl, sc, el, ec) => new EqualityOperatorLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryOperator, @"-", (v, sl, sc, el, ec) => new NegateOperatorLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryOperator, @"\+", (v, sl, sc, el, ec) => new AddOperatorLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryOperator, @"\*", (v, sl, sc, el, ec) => new MultiplyOperatorLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryOperator, @"/", (v, sl, sc, el, ec) => new DivideOperatorLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryOperator, @"%", (v, sl, sc, el, ec) => new ModOperatorLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryOperator, @"\|", (v, sl, sc, el, ec) => new OrOperatorLeaf(v, sl, sc, el, ec)),
            new(LexPriority.PrimaryOperator, @"&", (v, sl, sc, el, ec) => new AndOperatorLeaf(v, sl, sc, el, ec)),

            new(LexPriority.SecondaryOperator, @"=", (v, sl, sc, el, ec) => new AssignmentOperatorLeaf(v, sl, sc, el, ec)),
            new(LexPriority.SecondaryOperator, @"!", (v, sl, sc, el, ec) => new NotOperatorLeaf(v, sl, sc, el, ec)),

            new(LexPriority.Literal, @"\b\d+\b", (v, sl, sc, el, ec) => new IntLiteralLeaf(v, sl, sc, el, ec)),
            new(LexPriority.Literal, @"\b(true|false)\b", (v, sl, sc, el, ec) => new BoolLiteralLeaf(v, sl, sc, el, ec)),

            new(LexPriority.Whitespace  , @"\s+", (v, sl, sc, el, ec) => new WhitespaceLeaf(v, sl, sc, el, ec)),

            new(LexPriority.SecondaryPunctuation, @"\{", (v, sl, sc, el, ec) => new OpenBraceLeaf(v, sl, sc, el, ec)),
            new(LexPriority.SecondaryPunctuation, @"\}", (v, sl, sc, el, ec) => new CloseBraceLeaf(v, sl, sc, el, ec)),
            new(LexPriority.SecondaryPunctuation, @"\(", (v, sl, sc, el, ec) => new OpenParenthesisLeaf(v, sl, sc, el, ec)),
            new(LexPriority.SecondaryPunctuation, @"\)", (v, sl, sc, el, ec) => new CloseParenthesisLeaf(v, sl, sc, el, ec)),
            new(LexPriority.SecondaryPunctuation, @"\[", (v, sl, sc, el, ec) => new OpenSquareBracketLeaf(v, sl, sc, el, ec)),
            new(LexPriority.SecondaryPunctuation, @"\]", (v, sl, sc, el, ec) => new CloseSquareBracketLeaf(v, sl, sc, el, ec)),
            new(LexPriority.SecondaryPunctuation, @"<", (v, sl, sc, el, ec) => new OpenAngleBracketLeaf(v, sl, sc, el, ec)),
            new(LexPriority.SecondaryPunctuation, @">", (v, sl, sc, el, ec) => new CloseAngleBracketLeaf(v, sl, sc, el, ec)),

            new(LexPriority.Identifier, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b", (v, sl, sc, el, ec) => new IdentifierLeaf(v, sl, sc, el, ec))
        ];

        public List<LeafNode> TokenizeFile(string filePath, ILexer.OnUnexpectedTokenHandler? onUnexpectedToken = null)
        {
            string text = File.ReadAllText(filePath);
            return Tokenize(text, onUnexpectedToken);
        }

        public List<LeafNode> Tokenize(string text, ILexer.OnUnexpectedTokenHandler? onUnexpectedToken = null)
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

            int relativeLineIdx = 1;
            int relativeCharIdx = 1;

            for (int textIdx = 0; textIdx < text.Length;)
            {
                TokenDefinition? matchedDef = null;
                Match match = Match.Empty;
                bool isSuccess = false;

                foreach (var def in orderedDefinitions)
                {
                    match = def.Regex.Match(text, textIdx);

                    isSuccess = match.Success && match.Index == textIdx;

                    if (!isSuccess) continue;

                    matchedDef = def;
                    break;
                }

                if (onUnexpectedToken != null && !isSuccess)
                {
                    errorFlag = true;
                    onUnexpectedToken.Invoke(relativeLineIdx, relativeCharIdx, text[textIdx].ToString());
                    errorMessageBuilder.Append($"Line {relativeLineIdx}, Char {relativeCharIdx}\t'{text[textIdx]}'\n");

                    relativeCharIdx++;
                    textIdx++;
                    continue;
                }


                int startLine = relativeLineIdx;
                int startChar = relativeCharIdx;

                if (matchedDef!.Priority == LexPriority.Whitespace)
                {
                    bool hasNewline = false;
                    int charIdxOffset = 0;
                    for (int i = 0; i < match.Length; i++)
                    {
                        if (text[textIdx + i] == '\n')
                        {
                            hasNewline = true;
                            relativeLineIdx++;
                            relativeCharIdx = 1;
                        }
                        else
                        {
                            charIdxOffset++;
                        }
                    }
                    if (hasNewline)
                    {
                        relativeCharIdx += charIdxOffset;
                    }
                }

                LeafNode node = matchedDef!.NodeFactory.Invoke(
                    value: match.Value,
                    startLine: startLine,
                    startChar: startChar,
                    endLine: relativeLineIdx,
                    endChar: relativeCharIdx + match.Length);

                tokens.Add(node);

                relativeCharIdx += match.Length;
                textIdx += match.Length;
            }

            if (errorFlag) throw new ArgumentException(errorMessageBuilder.ToString());

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