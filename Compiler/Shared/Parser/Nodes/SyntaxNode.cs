using System.Net.Http.Headers;
using Compiler.Lexer;
using Compiler.Shared.Lexer;

namespace Compiler.Parser.Nodes
{
    public abstract class SyntaxNode(List<SyntaxNode> children)
    {
        public List<SyntaxNode> Children { get; } = children;

        public SyntaxNode() : this([]) { }

        public uint StartLine { get; set; }
        public uint StartChar { get; set; }
        public uint EndLine { get; set; }
        public uint EndChar { get; set; }

        public virtual void UpdateRange()
        {
            if (Children.Count == 0) return;

            StartLine = Children[0].StartLine;
            StartChar = Children[0].StartChar;
            EndLine = Children[^1].EndLine;
            EndChar = Children[^1].EndChar;
        }

        // prints entire subtree
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

    public abstract class LeafNode : SyntaxNode
    {
        public string Value { get; }

        public LeafNode(string value, uint startLine, uint startChar, uint endLine, uint endChar)
            : base()
        {
            Value = value;
            StartLine = startLine;
            StartChar = startChar;
            EndLine = endLine;
            EndChar = endChar;
        }

        public override void UpdateRange()
        {
            if (Children.Count == 0) return;

            if (Children[0].StartLine < StartLine
            || (Children[0].StartLine == StartLine && Children[0].StartChar < StartChar))
            {
                StartLine = Children[0].StartLine;
                StartChar = Children[0].StartChar;
            }
            else if (Children[^1].EndLine > EndLine
                 || (Children[^1].EndLine == EndLine && Children[^1].EndChar > EndChar))
            {
                EndLine = Children[^1].EndLine;
                EndChar = Children[^1].EndChar;
            }
        }

        public override string GetPrintable(int indent = 0)
        {
            var indentString = new string(' ', indent);
            var result = $"{indentString}{GetType().Name} [{StartLine}.{StartChar} - {EndLine}.{EndChar}] Token: {Value}\n";
            foreach (var child in Children)
            {
                result += child.GetPrintable(indent + 4);
            }
            return result;
        }
    }

    public class ProgramNode() : SyntaxNode
    {
    }
}
