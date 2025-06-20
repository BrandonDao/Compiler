namespace CompilerLib.Parser.Nodes.Punctuation
{
    public class OpenParenthesisLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "OpenParen";
    }
    public class CloseParenthesisLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "CloseParen";
    }

    public class OpenBraceLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "OpenBrace";
    }
    public class CloseBraceLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "CloseBrace";
    }

    public class OpenAngleBracketLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "OpenAngleBracket";
    }
    public class CloseAngleBracketLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "CloseAngleBracket";
    }
    public class OpenSquareBracketLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "OpenAngleBracket";
    }

    public class CloseSquareBracketLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : PunctuationLeaf(value, startLine, startChar, endLine, endChar)    {
        public override string GrammarIdentifier => "CloseSquareBracket";
    }
}
