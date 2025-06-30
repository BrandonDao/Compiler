namespace CompilerLib.Parser.Nodes.Scopes
{
    public class NamespaceKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);
}