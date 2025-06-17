using Compiler.Lexer;

namespace Compiler.Parser.Nodes.Operators
{
    public class Operator(Token token) : LeafWrapperNode(token)
    {
    }
}
