using System.Text.RegularExpressions;
using CompilerLib.Parser.Nodes;
using static CompilerLib.Lexer.TokenDefinition;

namespace CompilerLib.Lexer
{

    public class TokenDefinition(uint priority, string pattern, LeafNodeFactory nodeFactory)
    {
        public delegate LeafNode LeafNodeFactory(string value, uint startLine, uint startChar, uint endLine, uint endChar) ;

        public Regex Regex { get; } = new Regex(pattern, RegexOptions.Compiled);
        public uint Priority { get; } = priority;
        public LeafNodeFactory NodeFactory { get; } = nodeFactory;
    }
}