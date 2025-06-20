using CompilerLib.Parser.Nodes;

namespace Compiler.Parser
{
    public interface IParser
    {
        public SyntaxNode ParseTokens(List<LeafNode> tokens);
        public SyntaxNode ToAST(SyntaxNode root);
    }
}