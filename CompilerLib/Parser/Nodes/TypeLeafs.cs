namespace CompilerLib.Parser.Nodes
{
    public abstract class PrimitiveLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar)
    {
        public abstract string TypeName { get; }
    }

    public class Int8Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => "int8";
    }
    public class Int16Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => "int16";
    }
    public class Int32Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => "int32";
    }
    public class Int64Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => "int64";
    }
    public class BoolLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => "bool";
    }


    public class IdentifierLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}