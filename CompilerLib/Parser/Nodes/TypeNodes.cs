namespace CompilerLib.Parser.Nodes
{
    public abstract class TypeNode : SyntaxNode
    {
        public TypeNode(LeafNode token) : base([token])
            => UpdateRange();
    }

    public abstract class PrimitiveNode(LeafNode token) : TypeNode(token);

    public class Int8Node(LeafNode token) : PrimitiveNode(token);
    public class Int16Node(LeafNode token) : PrimitiveNode(token);
    public class Int32Node(LeafNode token) : PrimitiveNode(token);
    public class Int64Node(LeafNode token) : PrimitiveNode(token);
    public class BoolNode(LeafNode token) : PrimitiveNode(token);

    public class IdentifierNode(IdentifierLeaf token) : TypeNode(token);
}