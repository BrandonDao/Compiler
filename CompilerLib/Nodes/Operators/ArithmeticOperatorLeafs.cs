namespace CompilerLib.Nodes.Operators
{
    public class PlusOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class MinusOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class MultiplyOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class DivideOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
    public class ModOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar);
}
