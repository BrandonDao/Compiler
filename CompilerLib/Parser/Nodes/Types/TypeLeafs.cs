namespace CompilerLib.Parser.Nodes.Types
{
    public static class PrimitiveTypeNames
    {
        public const string Int8 = "int8";
        public const string Int16 = "int16";
        public const string Int32 = "int32";
        public const string Int64 = "int64";
        public const string Bool = "bool";
    }

    public abstract class TypeLeafNode(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar)
    {
        public abstract string TypeName { get; }
    }
    public abstract class PrimitiveLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : TypeLeafNode(value, startLine, startChar, endLine, endChar);

    public class Int8Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => PrimitiveTypeNames.Int8;
    }
    public class Int16Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => PrimitiveTypeNames.Int16;
    }
    public class Int32Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => PrimitiveTypeNames.Int32;
    }
    public class Int64Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => PrimitiveTypeNames.Int64;
    }
    public class BoolLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => PrimitiveTypeNames.Bool;
    }


    public class IdentifierLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : TypeLeafNode(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName { get; } = value;
    }
}