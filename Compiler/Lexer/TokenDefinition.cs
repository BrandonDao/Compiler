using System.Text.RegularExpressions;

namespace Compiler.Lexer
{
    public enum TokenType : byte
    {
        Int8,
        Int16,
        Int32,
        Int64,
        Boolean,
        Void,

        EqualityOperator,
        DotOperator,
        MinusSign,
        PlusSign,
        MultiplySign,
        DivideSign,

        AssignmentOperator,

        Using,
        While,
        Func,
        Return,

        Whitespace,

        IntegerLiteral,
        BooleanLiteral,

        Semicolon,
        Comma,
        LeftBrace,
        RightBrace,
        LeftParenthesis,
        RightParenthesis,
        LeftSquareBracket,
        RightSquareBracket,
        LeftAngleBracket,
        RightAngleBracket,

        Identifier,
    }

    public class TokenDefinition(uint priority, TokenType type, string pattern)
    {
        public TokenType Type { get; } = type;
        public Regex Regex { get; } = new Regex(pattern, RegexOptions.Compiled);
        public uint Priority { get; } = priority;
    }
}