
using System.Diagnostics;

namespace CompilerLib.Parser.Nodes
{
    [DebuggerDisplay("{StartLine}[{StartChar}..{EndChar}]: {Value}")]
    public abstract class LeafNode(string value, int startLine, int startChar, int endLine, int endChar)
        : SyntaxNode(startLine, startChar, endLine, endChar)
    {
        public abstract string GrammarIdentifier { get; }
        public string Value { get; } = value;

        public override void UpdateRange()
        {
            if (Children.Count == 0) return;

            if (Children[0].StartLine < StartLine
            || Children[0].StartLine == StartLine && Children[0].StartChar < StartChar)
            {
                StartLine = Children[0].StartLine;
                StartChar = Children[0].StartChar;
            }
            else if (Children[^1].EndLine > EndLine
                 || Children[^1].EndLine == EndLine && Children[^1].EndChar > EndChar)
            {
                EndLine = Children[^1].EndLine;
                EndChar = Children[^1].EndChar;
            }
        }

        public override string GetPrintable(int indent = 0)
        {
            var indentString = new string(' ', indent);
            var result = $"{indentString}{GetType().Name} [{StartLine}.{StartChar} - {EndLine}.{EndChar}] Token: {Value}\n";
            foreach (var child in Children)
            {
                result += child.GetPrintable(indent + 4);
            }
            return result;
        }
    }
}
