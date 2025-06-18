namespace Compiler.Shared.Parser.Nodes.Punctuation
{
    public class CloseBrace(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
}
