namespace CompilerLib.Parser.Nodes
{
    public abstract class PrimitiveLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);

    public class Int8Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar);
    public class Int16Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar);
    public class Int32Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar);
    public class Int64Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar);
    public class BoolLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar);


    public class IdentifierLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}