namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class SmallArrowLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "SmallArrow";
        public bool IsInserted { get; init; }
    }

    public class ImplicitSmallArrowLeaf(int startLine, int startChar)
        : ImplicitNode("->", startLine, startChar);
}