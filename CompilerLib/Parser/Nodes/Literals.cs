namespace CompilerLib.Parser.Nodes
{
    public abstract class LiteralLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);

    public class IntLiteralLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LiteralLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "LiteralInt";
    }

    public class BoolLiteralLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LiteralLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "LiteralBool";
    }
}
