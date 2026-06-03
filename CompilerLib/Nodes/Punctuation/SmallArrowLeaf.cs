namespace CompilerLib.Nodes.Punctuation;

public class SmallArrowLeaf(string value, int startLine, int startChar, int endLine, int endChar)
    : PunctuationLeaf(value, startLine, startChar, endLine, endChar)
{
    public bool IsInserted { get; init; }
}

public class ImplicitSmallArrowLeaf(int startLine, int startChar)
    : SmallArrowLeaf("->", startLine, startChar, startLine, startChar)
{
    public override string GetPrintable(int indent)
    {
        string indentString = new(' ', indent);
        return $"[{StartLine}.{StartChar}]\t\t{indentString}{GetType().Name}\n";
    }
}