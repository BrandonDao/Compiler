namespace Compiler.Shared.Parser.Nodes.Primitives
{
    public class Int64(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Primitive(value, startLine, startChar, endLine, endChar);
}
