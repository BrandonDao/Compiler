namespace CompilerLib.Parser.Nodes
{
    public abstract class KeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);


    public class VoidLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);
    public class ImplicitVoidLeaf(int startLine, int startChar)
        : VoidLeaf("void", startLine, startChar, startLine, startChar)
    {
        public override string GetPrintable(int indent)
        {
            var indentString = new string(' ', indent);
            return $"[{StartLine}.{StartChar}]\t\t{indentString}{GetType().Name}\n";
        }
    }

    public class NamespaceKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);
    public class LetKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);
    public class WhileKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);
    public class FunctionKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);
}