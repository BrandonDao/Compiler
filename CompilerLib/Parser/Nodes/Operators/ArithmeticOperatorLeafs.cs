namespace CompilerLib.Parser.Nodes.Operators
{
    public class AddOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class NegateOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class MultiplyOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class DivideOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class ModOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
}
