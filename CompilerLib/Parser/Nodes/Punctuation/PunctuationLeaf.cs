namespace CompilerLib.Parser.Nodes.Punctuation
{
    public abstract class PunctuationLeaf(string value, int startLine, int startChar, int endLine, int endChar, List<SyntaxNode> children)
        : LeafNode(value, startLine, startChar, endLine, endChar, children)
    {
        public PunctuationLeaf(string value, int startLine, int startChar, int endLine, int endChar)
            : this(value, startLine, startChar, endLine, endChar, []) { }
    }
}
