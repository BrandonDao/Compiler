namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class CommaLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar);
}
