namespace Compiler.Lexer
{
    [Flags]
    public enum TokenType : ushort
    {
        Undefined = 0,

        PrimitiveFlag = 0b1000_0000 << 8,
        Int8,
        Int16,
        Int32,
        Int64,
        Boolean,


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
        Alias,
        While,
        Func,
        Void,
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
}