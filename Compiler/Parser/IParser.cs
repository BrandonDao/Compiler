using CompilerLib.Nodes;

namespace Compiler.Parser;

public interface IParser
{
    public ParserEntrypointNode ParseTokensToCST(ITokenStream tokenStream);
    public ParserEntrypointNode ParseCSTToAST(ParserEntrypointNode root);
}