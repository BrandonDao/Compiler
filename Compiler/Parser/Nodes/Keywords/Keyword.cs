using Compiler.Lexer;

namespace Compiler.Parser.Nodes.Keywords
{
    public abstract class Keyword(Token token) : ParentNode
    {
        public Token Token { get; } = token;
    }
}
