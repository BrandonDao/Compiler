using Compiler.Lexer;

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

    public abstract class LeafWrapperNode : SyntaxNode
    {
        public Token Token { get; }

        public LeafWrapperNode(Token token)
        {
            Token = token;
            StartLine = Token.LineIndex;
            StartChar = token.StartChar;
            EndLine = Token.LineIndex;
            EndChar = token.EndChar;
        }

        public override void UpdateRange()
        {
            if (Children.Count == 0) return;

            StartLine = Children[0].StartLine;
            StartChar = Children[0].StartChar;
            EndLine = Children[^1].EndLine;
            EndChar = Children[^1].EndChar;

            if (Token.LineIndex < StartLine
            || (Token.LineIndex == StartLine && Token.StartChar < StartChar))
            {
                StartLine = Token.LineIndex;
                StartChar = Token.StartChar;
            }
            else if (Token.LineIndex > EndLine
            || (Token.LineIndex == StartLine && Token.EndChar > EndChar))
            {
                EndLine = Token.LineIndex;
                EndChar = Token.EndChar;
            }
        }

        public override string GetPrintable(int indent = 0)
        {
            var indentString = new string(' ', indent);
            var result = $"{indentString}{GetType().Name} [{StartLine}.{StartChar} - {EndLine}.{EndChar}] Token: {Token.Value}\n";
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

    public class AliasDirective(string alias, string typeName) : SyntaxNode
    {
        public string Alias { get; } = alias;
        public string TypeName { get; } = typeName;
    }
}
