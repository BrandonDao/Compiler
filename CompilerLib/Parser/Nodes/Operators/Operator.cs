namespace CompilerLib.Parser.Nodes.Operators
{
    public abstract class Operator(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}
