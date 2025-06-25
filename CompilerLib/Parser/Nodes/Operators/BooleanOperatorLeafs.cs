namespace CompilerLib.Parser.Nodes.Operators
{
    public class EqualityOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class OrOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class AndOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);

    public class NotOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
}
