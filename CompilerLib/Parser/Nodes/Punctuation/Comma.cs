namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class Comma(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
}
