using System.Text.RegularExpressions;

namespace Compiler.Lexer
{
    [Flags]
    public enum TokenType : ushort
    {
        PrimitiveFlag = (0b1000_0000 << 8) | IdentifierFlag,
        Int8,
        Int16,
        Int32,
        Int64,
        Boolean,
        Void,


        OperatorFlag = 0b0100_0000 << 8,
        EqualityOperator,
        DotOperator,
        ModulusOperator,
        MinusSign,
        PlusSign,
        MultiplySign,
        DivideSign,
        AssignmentOperator,

        KeywordFlag = 0b0010_0000 << 8,
        Using,
        While,
        Func,
        Return,
        IfStatement,
        ElseStatement,

        WhitespaceFlag = 0b0001_0000 << 8,
        Whitespace,

        LiteralFlag = 0b0000_1000 << 8,
        IntegerLiteral,
        BooleanLiteral,

        PunctuationFlag = 0b0000_0100 << 8,
        Semicolon,
        Comma,
        OpenBrace,
        CloseBrace,
        OpenParenthesis,
        CloseParenthesis,
        OpenSquareBracket,
        CloseSquareBracket,
        OpenAngleBracket,
        CloseAngleBracket,

        IdentifierFlag = 0b0000_0010 << 8,
        Identifier,
    }

    public class TokenDefinition(uint priority, TokenType type, string pattern)
    {
        public TokenType Type { get; } = type;
        public Regex Regex { get; } = new Regex(pattern, RegexOptions.Compiled);
        public uint Priority { get; } = priority;
    }
}