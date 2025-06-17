using Compiler.Lexer;

namespace Compiler.Parser.Nodes.Whitespace
{
    public class Whitespace(Token token, bool isLeading) : LeafWrapperNode(token)
    {
        public bool IsLeading { get; } = isLeading;

        public override string GetPrintable(int indent = 0)
        {
            var indentString = new string(' ', indent);
            return $"{indentString}{(IsLeading ? "Leading" : "Trailing")} {GetType().Name} [{StartLine}.{StartChar} - {EndLine}.{EndChar}]\n";
        }
    }
}
