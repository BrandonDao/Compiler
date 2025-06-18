using Compiler.Parser.Nodes;

namespace Compiler.Shared.Parser.Nodes.Punctuation
{
    public abstract class Punctuation(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);
}
