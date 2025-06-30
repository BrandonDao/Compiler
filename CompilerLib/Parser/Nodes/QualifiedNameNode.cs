using System.Text;
using CompilerLib.Parser.Nodes.Types;

namespace CompilerLib.Parser.Nodes
{
    public class QualifiedNameNode : SyntaxNode
    {
        public string Name { get; set; }
        private readonly List<SyntaxNode> trimmedChildren;

        public QualifiedNameNode(List<SyntaxNode> children) : base(children)
        {
            trimmedChildren = new List<SyntaxNode>(capacity: children.Count);

            if (children.Count == 0 || children[0] is not IdentifierLeaf startIdLeaf)
                throw new ArgumentException("QualifiedNameNode must start with an IdentifierLeaf!");

            StringBuilder nameBuilder = new(startIdLeaf.Value);
            for (int i = 1; i < children.Count; i++)
            {
                SyntaxNode? child = children[i];
                if (child is IdentifierLeaf idLeaf)
                {
                    nameBuilder.Append('.');
                    nameBuilder.Append(idLeaf.Value);
                    trimmedChildren.Add(idLeaf);
                }
            }
            Name = nameBuilder.ToString();
        }

        public override SyntaxNode ToAST()
        {
            Children = trimmedChildren;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }

        public override string GetPrintable(int indent = 0)
        {
            var indentString = new string(' ', indent);
            StringBuilder builder = new();
            builder.Append($"[{StartLine}.{StartChar} - {EndLine}.{EndChar}]\t{indentString}{GetType().Name} Name: {Name}\n");
            foreach (var child in Children)
            {
                builder.Append(child.GetPrintable(indent + 4));
            }
            return builder.ToString();
        }
    }
}