namespace CompilerLib.Parser.Nodes.Primitives
{
    public class Bool(string value, int startLine, int startChar, int endLine, int endChar)
        : Primitive(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Bool";
    }
}
