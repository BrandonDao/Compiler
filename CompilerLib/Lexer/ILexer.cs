using CompilerLib.Parser.Nodes;

namespace CompilerLib.Lexer
{
    public interface ILexer
    {
        delegate void OnUnexpectedTokenHandler(uint lineIdx, uint charIdx, string tokenValue);

        List<LeafNode> TokenizeFile(string filePath, OnUnexpectedTokenHandler? onUnexpectedToken = null);
        List<LeafNode> Tokenize(string[] lines, OnUnexpectedTokenHandler? onUnexpectedToken = null);
    }
}