namespace Compiler.Lexer
{
    public interface ILexer
    {
        List<Token> TokenizeFile(string filePath);
        List<Token> Tokenize(string[] lines);
    }
}