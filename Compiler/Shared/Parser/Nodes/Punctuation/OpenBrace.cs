namespace Compiler.Shared.Parser.Nodes.Punctuation
{
    public class OpenBrace(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
}
