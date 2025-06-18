using System.Text.RegularExpressions;
using Compiler.Lexer;

namespace Compiler.Shared.Lexer
{
    public class Token(TokenType type, Match match, uint lineIdx, uint baseIdx)
    {
        public TokenType Type { get; } = type;
        public string Value { get; } = match.Value;
        public uint LineIndex { get; } = lineIdx;
        public uint StartChar { get; } = baseIdx + (uint)match.Index;
        public uint EndChar { get; } = baseIdx + (uint)(match.Index + match.Length);

        public override string ToString()
        {
            return $"{LineIndex}[{StartChar}..{EndChar}] {Type}: {Value}";
        }
    }
}