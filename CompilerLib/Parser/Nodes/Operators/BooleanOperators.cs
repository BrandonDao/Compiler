namespace CompilerLib.Parser.Nodes.Operators
{
    public class EqualityOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpEquality";
    }
    public class OrOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpOr";
    }
    public class AndOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpAdd";
    }
}
