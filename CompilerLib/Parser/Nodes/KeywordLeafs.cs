namespace CompilerLib.Parser.Nodes
{
    public abstract class KeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);


    public class VoidLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar)
    {
        public override string GrammarIdentifier => "Void";
        public bool IsInserted { get; init; }
        public VoidLeaf(int startLine, int startChar)
            : this("void", startLine, startChar, startLine, startChar)
            => IsInserted = true;

        public override string GetPrintable(int indent)
        {
            if (IsInserted)
            {
                var indentString = new string(' ', indent);
                return $"[{StartLine}.{StartChar}]\t\t{indentString}{GetType().Name} (INSERTED)\n";
            }
            return base.GetPrintable(indent);
        }
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