using Compiler.Parser.Nodes;

namespace Compiler.Shared.Parser.Nodes.Operators
{
    public class Operator(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}
