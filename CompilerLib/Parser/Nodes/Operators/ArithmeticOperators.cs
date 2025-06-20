namespace CompilerLib.Parser.Nodes.Operators
{
    public class AddOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpAdd";
    }
    public class NegateOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpNegate";
    }
    public class MultiplyOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpMultiply";
    }
    public class DivideOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpDivide";
    }
    public class ModOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpMod";
    }
}
