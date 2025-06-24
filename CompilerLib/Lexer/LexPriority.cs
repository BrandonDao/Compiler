namespace CompilerLib.Lexer
{
    public enum LexPriority : byte
    {
        PrimitiveOrKeyword = 0,
        PrimaryPunctuation = 2,
        PrimaryOperator = 5,
        SecondaryOperator = 10,
        Literal = 20,
        Whitespace = 30,
        SecondaryPunctuation = 40,
        Identifier = 50,
        Lowest = byte.MaxValue
    }
}