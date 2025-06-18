namespace CompilerLib.Parser.Nodes.Identifiers
{
    public class IdentifierName(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}
