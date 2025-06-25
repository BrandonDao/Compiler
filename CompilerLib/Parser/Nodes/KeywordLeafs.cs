namespace CompilerLib.Parser.Nodes
{
    public abstract class KeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);


    public class VoidLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Void";
    }
    public class ImplicitVoidLeaf(int startLine, int startChar)
        : ImplicitNode("void", startLine, startChar);

    public class NamespaceKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Namespace";
    }
    public class LetKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Let";
    }
    public class WhileKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "While";
    }
    public class FunctionKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Func";
    }
}