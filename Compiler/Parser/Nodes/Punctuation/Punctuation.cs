using Compiler.Lexer;

namespace Compiler.Parser.Nodes.Punctuation
{
    public abstract class Punctuation(Token token) : LeafWrapperNode(token)
    {
    }
}
