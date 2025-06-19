namespace CompilerLib.Parser.Nodes.Primitives
{
    public abstract class Primitive(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}