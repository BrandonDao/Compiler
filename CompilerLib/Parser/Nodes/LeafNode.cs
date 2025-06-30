using System.Text;
using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes
{
    public abstract class LeafNode(string value, int startLine, int startChar, int endLine, int endChar, List<SyntaxNode> children)
        : SyntaxNode(startLine, startChar, endLine, endChar, children)
    {
        public string Value { get; } = value;

        public LeafNode(string value, int startLine, int startChar, int endLine, int endChar)
            : this(value, startLine, startChar, endLine, endChar, []) { }

        public override SyntaxNode ToAST()
        {
            Children.Clear(); // Clear whitespace
            return this;
        }

        public override void UpdateRange()
        {
            if (Children.Count == 0) return;

            if (Children[0].StartLine < StartLine
            || Children[0].StartLine == StartLine && Children[0].StartChar < StartChar)
            {
                StartLine = Children[0].StartLine;
                StartChar = Children[0].StartChar;
            }
            else if (Children[^1].EndLine > EndLine
                 || Children[^1].EndLine == EndLine && Children[^1].EndChar > EndChar)
            {
                EndLine = Children[^1].EndLine;
                EndChar = Children[^1].EndChar;
            }
        }

        public override string GetPrintable(int indent = 0)
        {
            var indentString = new string(' ', indent);
            var result = $"[{StartLine}.{StartChar} - {EndLine}.{EndChar}]\t{indentString}{GetType().Name} Token: {Value}\n";
            foreach (var child in Children)
            {
                result += child.GetPrintable(indent + 4);
            }
            return result;
        }

        public override void FlattenBackToInput(StringBuilder builder)
        {
            if (Children.Count == 1)
            {
                if (((WhitespaceLeaf)Children[0]).IsLeading)
                {
                    Children[0].FlattenBackToInput(builder);
                    builder.Append(Value);
                }
                else
                {
                    builder.Append(Value);
                    Children[0].FlattenBackToInput(builder);
                }
            }
            else if (Children.Count == 0)
            {
                builder.Append(Value);
            }
            else
            {
                Children[0].FlattenBackToInput(builder);
                builder.Append(Value);
                Children[1].FlattenBackToInput(builder);
            }
        }

        public override string ToString() => $"[{StartLine}.{StartChar} - {EndLine}.{EndChar}] {GetType().Name} Token: {Value}";
    }
}