namespace CompilerLib.Parser.Nodes.Literals
{
    public abstract class Literal(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);



    public class IntLiteralToken(string value, int startLine, int startChar, int endLine, int endChar)
        : Literal(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "LiteralInt";
    }

    public class BoolLiteral(string value, int startLine, int startChar, int endLine, int endChar)
        : Literal(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "LiteralBool";
    }
}
