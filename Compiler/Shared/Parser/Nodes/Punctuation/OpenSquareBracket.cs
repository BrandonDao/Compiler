namespace Compiler.Shared.Parser.Nodes.Punctuation
{
    public class OpenSquareBracket(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
}
