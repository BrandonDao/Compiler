using System.Text.RegularExpressions;
using CompilerLib.Parser.Nodes;
using static CompilerLib.Lexer.TokenDefinition;

namespace CompilerLib.Lexer
{
    public class TokenDefinition(LexPriority priority, string pattern, LeafNodeFactory nodeFactory)
    {
        public delegate LeafNode LeafNodeFactory(string value, int startLine, int startChar, int endLine, int endChar);

        public Regex Regex { get; } = new Regex(pattern, RegexOptions.Compiled);
        public LexPriority Priority { get; } = priority;
        public LeafNodeFactory NodeFactory { get; } = nodeFactory;
    }
}