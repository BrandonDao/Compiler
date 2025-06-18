using Compiler.Shared.Parser.Nodes.Operators;

namespace Compiler.Shared.Parser.Nodes.Punctuation
{
    public class CloseAngleBracket(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Operator(value, startLine, startChar, endLine, endChar);
}
