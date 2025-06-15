namespace Compiler.Lexer
{
    public interface ILexer
    {
        delegate void OnUnexpectedTokenHandler(uint lineIdx, uint charIdx, string tokenValue);

        List<Token> TokenizeFile(string filePath, OnUnexpectedTokenHandler? onUnexpectedToken = null);
        List<Token> Tokenize(string[] lines, OnUnexpectedTokenHandler? onUnexpectedToken = null);
    }
}