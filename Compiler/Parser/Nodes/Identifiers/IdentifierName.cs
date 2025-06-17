using Compiler.Lexer;

namespace Compiler.Parser.Nodes.Identifiers
{
    public class IdentifierName(Token token) : ParentNode
    {
        public string Name { get; } = token.Value;
        public Token Token { get; } = token;
    }
}
