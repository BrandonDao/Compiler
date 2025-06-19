namespace CompilerLib.Parser.Nodes.Primitives
{
    public class Int8(string value, int startLine, int startChar, int endLine, int endChar)
        : Primitive(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Int8";
    }

    public class Int16(string value, int startLine, int startChar, int endLine, int endChar)
        : Primitive(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Int16";
    }

    public class Int32(string value, int startLine, int startChar, int endLine, int endChar)
        : Primitive(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Int32";
    }

    public class Int64(string value, int startLine, int startChar, int endLine, int endChar)
        : Primitive(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Int64";
    }
}