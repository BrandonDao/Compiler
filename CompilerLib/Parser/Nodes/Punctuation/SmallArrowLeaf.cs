namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class SmallArrowLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "SmallArrow";
        public bool IsInserted { get; init; }

        public SmallArrowLeaf(int startLine, int startChar)
            : this("->", startLine, startChar, startLine, startChar)
            => IsInserted = true;

        public override string GetPrintable(int indent)
        {
            if (IsInserted)
            {
                var indentString = new string(' ', indent);
                return $"[{StartLine}.{StartChar}]\t\t{indentString}{GetType().Name} (INSERTED)\n";
            }
            return base.GetPrintable(indent);
        }
    }
}