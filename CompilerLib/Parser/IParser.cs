using CompilerLib.Nodes;

namespace CompilerLib.Parser;

public interface IParser
{
    public ParserEntrypointNode ParseTokensToCST(List<LeafNode> tokenStream);
    public ParserEntrypointNode ParseCSTToAST(ParserEntrypointNode root);
}