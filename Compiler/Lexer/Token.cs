using System.Text.RegularExpressions;

namespace Compiler.Lexer
{
    public class Token(TokenType type, Match match, uint baseIdx)
    {
        public TokenType Type { get; } = type;
        public string Value { get; } = match.Value;
        public uint Start { get; } = baseIdx + (uint)match.Index;
        public uint End { get; } = baseIdx + (uint)(match.Index + match.Length);

        public override string ToString()
        {
            return $"[{Start}..{End}] {Type}: {Value}";
        }
    }
}