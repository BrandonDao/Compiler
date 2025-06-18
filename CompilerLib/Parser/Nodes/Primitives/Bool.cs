namespace CompilerLib.Parser.Nodes.Primitives
{
    public class Bool(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Primitive(value, startLine, startChar, endLine, endChar);
}
