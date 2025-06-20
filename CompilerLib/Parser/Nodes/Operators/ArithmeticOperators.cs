namespace CompilerLib.Parser.Nodes.Operators
{
    public class AddOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpAdd";
    }
    public class NegateOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpNegate";
    }
    public class MultiplyOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpMultiply";
    }
    public class DivideOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpDivide";
    }
    public class ModOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpMod";
    }
}
