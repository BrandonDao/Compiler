namespace CompilerLib.Parser.Nodes
{
    public class Undefined(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}
