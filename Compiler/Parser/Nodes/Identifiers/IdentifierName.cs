using Compiler.Lexer;

namespace Compiler.Parser.Nodes.Identifiers
{
    public class IdentifierName(Token token) : LeafWrapperNode(token)
    {
        public string Name { get; } = token.Value;
    }
}
