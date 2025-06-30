namespace CompilerLib.Parser.Nodes.Statements.Controls
{
    public class WhileKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);
}