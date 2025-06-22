using System.Text;

namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class WhitespaceLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar, EmptyChildren)
    {
        private static List<SyntaxNode> EmptyChildren { get; } = [];

        public override string GrammarIdentifier => "Whitespace";
        public bool IsLeading { get; set; }

        public override void FlattenBackToInput(StringBuilder builder)
        {
            builder.Append(Value);
        }

        public override string GetPrintable(int indent = 0)
        {
            var indentString = new string(' ', indent);
            return $"[{StartLine}.{StartChar} - {EndLine}.{EndChar}]\t{indentString}{(IsLeading ? "Leading" : "Trailing")} Whitespace\n";
        }
    }
}
