namespace CompilerLib.Parser.Nodes.Punctuation
{
    public abstract class PunctuationLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}
