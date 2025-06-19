namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class OpenParenthesis(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "OpenParen";
    }
    public class CloseParenthesis(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "CloseParen";
    }

    public class OpenBrace(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "OpenBrace";
    }
    public class CloseBrace(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "CloseBrace";
    }

    public class OpenAngleBracket(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "OpenAngleBracket";
    }
    public class CloseAngleBracket(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "CloseAngleBracket";
    }
    public class OpenSquareBracket(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "OpenAngleBracket";
    }

    public class CloseSquareBracket(string value, int startLine, int startChar, int endLine, int endChar)
        : Punctuation(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "CloseSquareBracket";
    }
}
