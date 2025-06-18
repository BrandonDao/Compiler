namespace Compiler.Shared.Parser.Nodes.Operators
{
    public class AddOperator(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Operator(value, startLine, startChar, endLine, endChar);
    public class NegateOperator(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Operator(value, startLine, startChar, endLine, endChar);
    public class MultiplyOperator(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Operator(value, startLine, startChar, endLine, endChar);
    public class DivideOperator(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Operator(value, startLine, startChar, endLine, endChar);
}
