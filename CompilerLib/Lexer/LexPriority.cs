namespace CompilerLib.Lexer
{
    public enum LexPriority : byte
    {
        Primitive = 0,
        PrimaryOperator = 5,
        SecondaryOperator = 10,
        Literal = 20,
        Whitespace = 30,
        Punctuation = 40,
        Identifier = 50,
        Lowest = byte.MaxValue
    }
}