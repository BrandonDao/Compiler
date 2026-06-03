using CompilerLib.Lexer;
using CompilerLib.Nodes;

namespace CompilerLib.Parser;

public interface IParser
{
    public ParserEntrypointNode ParseTokensToCST(ITokenStream tokenStream);
    public ParserEntrypointNode ParseCSTToAST(ParserEntrypointNode root);
}