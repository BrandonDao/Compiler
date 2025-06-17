using Compiler.Lexer;

namespace Compiler.Parser.Nodes.Primitives
{
    public abstract class Primitive(Token token) : LeafWrapperNode(token)
    {
        public string Type { get; } = token.Value;
    }
}