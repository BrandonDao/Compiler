namespace CompilerLib.Parser.Nodes.Operators
{
    public class AssignmentOperator(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Operator(value, startLine, startChar, endLine, endChar);
}
