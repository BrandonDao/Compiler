using Compiler.Lexer;

namespace Compiler.Parser.Nodes.Whitespace
{
    public class Whitespace : SyntaxNode
    {
        public Token Token { get; }
        public bool IsLeading { get; }

        public Whitespace(Token token, bool isLeading)
        {
            Token = token;
            IsLeading = isLeading;
            
            StartLine = token.LineIndex;
            StartChar = token.StartChar;
            EndLine = token.LineIndex;
            EndChar = token.EndChar;
        }
    }
}
