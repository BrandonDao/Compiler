namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class OpenParenthesis(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
    public class CloseParenthesis(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);

    public class OpenBrace(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
    public class CloseBrace(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);

    public class OpenAngleBracket(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
    public class CloseAngleBracket(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);

    public class CloseSquareBracket(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
    public class OpenSquareBracket(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar);
}
