namespace CompilerLib.Parser.Nodes.Types
{
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
        public override string TypeName => LanguageNames.Primitives.Int8;
    }
    public class Int16Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => LanguageNames.Primitives.Int16;
    }
    public class Int32Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => LanguageNames.Primitives.Int32;
    }
    public class Int64Leaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => LanguageNames.Primitives.Int64;
    }
    public class BoolLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PrimitiveLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName => LanguageNames.Primitives.Bool;
    }


    public class IdentifierLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : TypeLeafNode(value, startLine, startChar, endLine, endChar)
    {
        public override string TypeName { get; } = value;
    }
}