using Compiler.Parser.Nodes;

namespace Compiler.Shared.Parser.Nodes.Primitives
{
    public abstract class Primitive(string value, uint startLine, uint startChar, uint endLine, uint endChar)
    : LeafNode(value, startLine, startChar, endLine, endChar);
}