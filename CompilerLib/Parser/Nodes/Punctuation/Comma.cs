namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class Comma(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Comma";
    }
}
