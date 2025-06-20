namespace CompilerLib.Parser.Nodes.Operators
{
    public abstract class OperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}
