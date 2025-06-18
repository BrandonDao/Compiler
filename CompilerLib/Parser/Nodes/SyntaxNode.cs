namespace CompilerLib.Parser.Nodes
{
    public abstract class SyntaxNode(uint startLine, uint startChar, uint endLine, uint endChar, List<SyntaxNode> children)
    {
        public List<SyntaxNode> Children { get; } = children;
        public uint StartLine { get; set; } = startLine;
        public uint StartChar { get; set; } = startChar;
        public uint EndLine { get; set; } = endLine;
        public uint EndChar { get; set; } = endChar;

        public SyntaxNode(List<SyntaxNode> children)
            : this(0, 0, 0, 0, children) { }
        public SyntaxNode(uint startLine, uint startChar, uint endLine, uint endChar)
            : this(startLine, startChar, endLine, endChar, []) { }
        public SyntaxNode() : this([]) { }

        public virtual void UpdateRange()
        {
            if (Children.Count == 0) return;

            StartLine = Children[0].StartLine;
            StartChar = Children[0].StartChar;
            EndLine = Children[^1].EndLine;
            EndChar = Children[^1].EndChar;
        }

        public virtual string GetPrintable(int indent = 0)
        {
            var indentString = new string(' ', indent);
            var result = $"{indentString}{GetType().Name} [{StartLine}.{StartChar} - {EndLine}.{EndChar}]\n";
            foreach (var child in Children)
            {
                result += child.GetPrintable(indent + 4);
            }
            return result;
        }
    }
}
