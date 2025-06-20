namespace CompilerLib.Parser.Nodes.Operators
{
    public class AssignmentOperator(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpAssign";
    }
}
