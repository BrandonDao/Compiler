namespace CompilerLib.Parser.Nodes
{
    public class FunctionKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);
}