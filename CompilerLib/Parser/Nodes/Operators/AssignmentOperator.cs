namespace CompilerLib.Parser.Nodes.Operators
{
    public class AssignmentOperatorLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : OperatorLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "OpAssign";
    }
}
