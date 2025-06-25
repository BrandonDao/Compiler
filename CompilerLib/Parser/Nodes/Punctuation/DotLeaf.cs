namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class DotLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Dot";
    }
}