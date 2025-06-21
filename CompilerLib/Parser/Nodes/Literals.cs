namespace CompilerLib.Parser.Nodes
{
    public abstract class LiteralNode(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);



    public class IntLiteralLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LiteralNode(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "LiteralInt";
    }
    public class IntLiteralNode : SyntaxNode
    {
        public IntLiteralNode(IntLiteralLeaf token) : base(children: [token])
            => UpdateRange();
    }

    public class BoolLiteralLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LiteralNode(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "LiteralBool";
    }
    public class BoolLiteralNode : SyntaxNode
    {
        public BoolLiteralNode(BoolLiteralLeaf token) : base(children: [token])
            => UpdateRange();
    }
}
