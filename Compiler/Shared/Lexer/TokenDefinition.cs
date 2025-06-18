using System.Text.RegularExpressions;
using Compiler.Parser.Nodes;
using Compiler.Shared.Lexer;

namespace Compiler.Lexer
{

    public class TokenDefinition(uint priority, TokenType type, string pattern, Func<string, uint, uint, uint, uint, LeafNode> nodeFactory)
    {
        public TokenType Type { get; } = type;
        public Regex Regex { get; } = new Regex(pattern, RegexOptions.Compiled);
        public uint Priority { get; } = priority;
        public Func<string, uint, uint, uint, uint, LeafNode> NodeFactory { get; } = nodeFactory;
    }
}