using System.Text.RegularExpressions;

namespace Compiler.Lexer
{

    public class TokenDefinition(uint priority, TokenType type, string pattern)
    {
        public TokenType Type { get; } = type;
        public Regex Regex { get; } = new Regex(pattern, RegexOptions.Compiled);
        public uint Priority { get; } = priority;
    }
}