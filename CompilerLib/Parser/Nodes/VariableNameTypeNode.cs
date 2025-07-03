using CompilerLib.Parser.Nodes.Punctuation;
using CompilerLib.Parser.Nodes.Types;

namespace CompilerLib.Parser.Nodes
{
    public class VariableNameTypeNode : SyntaxNode
    {
        public string Name => Identifier.Value;
        public string Type { get; }
        public IdentifierLeaf Identifier { get; }
        public VariableNameTypeNode(IdentifierLeaf id, ColonLeaf colon, SyntaxNode type) : base([id, colon, type])
        {
            Identifier = id;
            if (type is IdentifierLeaf idLeaf)
            {
                Type = idLeaf.Value;
            }
            else if (type is PrimitiveLeaf primitiveLeaf)
            {
                Type = primitiveLeaf.TypeName;
            }
            else
                throw new ArgumentException($"Expected an identifier or primitive type, not {type}!");

            UpdateRange();
        }

        public override VariableNameTypeNode ToAST()
        {
            Children.RemoveAt(1); // Remove the colon
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
}