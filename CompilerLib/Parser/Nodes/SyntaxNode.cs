using System.Text;

namespace CompilerLib.Parser.Nodes
{
    public abstract class SyntaxNode
    {
        public List<SyntaxNode> Children { get; }
        public int StartLine { get; set; }
        public int StartChar { get; set; }
        public int EndLine { get; set; }
        public int EndChar { get; set; }

        private SyntaxNode(int startLine, int startChar, int endLine, int endChar, List<SyntaxNode> children)
        {
            Children = children;
            StartLine = startLine;
            StartChar = startChar;
            EndLine = endLine;
            EndChar = endChar;
        }

        public SyntaxNode(List<SyntaxNode> children)
            : this(0, 0, 0, 0, children) { }
        public SyntaxNode(int startLine, int startChar, int endLine, int endChar)
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
            var result = $"[{StartLine}.{StartChar} - {EndLine}.{EndChar}]\t{indentString}{GetType().Name}\n";
            foreach (var child in Children)
            {
                result += child.GetPrintable(indent + 4);
            }
            return result;
        }
        public virtual void FlattenBackToInput(StringBuilder builder)
        {
            foreach (var child in Children)
            {
                child.FlattenBackToInput(builder);
            }
        }
    }
}
