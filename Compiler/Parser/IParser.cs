using CompilerLib.Parser.Nodes;

namespace Compiler.Parser
{
    public interface IParser
    {
        public ParserEntrypointNode? ParseTokensToCST(List<LeafNode> tokens);
        public ParserEntrypointNode ParseCSTToAST(ParserEntrypointNode root);
    }
}