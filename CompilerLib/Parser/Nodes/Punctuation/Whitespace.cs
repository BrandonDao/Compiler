using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Text;

namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class Whitespace(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)
    {
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
