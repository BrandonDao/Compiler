namespace CompilerLib.Parser.Nodes.Primitives
{
    public class Int8(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Primitive(value, startLine, startChar, endLine, endChar);

    public class Int16(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Primitive(value, startLine, startChar, endLine, endChar);

    public class Int32(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Primitive(value, startLine, startChar, endLine, endChar);
    
    public class Int64(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Primitive(value, startLine, startChar, endLine, endChar);
}