namespace CompilerLib.Parser.Nodes.Identifiers
{
    public class IdentifierName(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Id";
    }
}
