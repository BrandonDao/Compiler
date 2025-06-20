namespace CompilerLib.Parser.Nodes
{
    public class IdentifierLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Id";
    }
    public class IdentifierNode : SyntaxNode
        {
            public IdentifierNode(IdentifierLeaf token) : base(children: [token])
                => UpdateRange();
        }
}
