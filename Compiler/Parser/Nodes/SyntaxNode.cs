using System.Runtime.InteropServices.Marshalling;
using Compiler.Lexer;

namespace Compiler.Parser.Nodes
{
    public abstract class SyntaxNode
    {
        public uint StartLine { get; set; }
        public uint StartChar { get; set; }
        public uint EndLine { get; set; }
        public uint EndChar { get; set; }
    }

    public abstract class ParentNode(List<SyntaxNode> children) : SyntaxNode
    {
        public List<SyntaxNode> Children { get; } = children;

        public ParentNode() : this([]) { }

        public virtual void UpdateRange()
        {
            if (Children.Count == 0) return;

            StartLine = Children[0].StartLine;
            StartChar = Children[0].StartChar;
            EndLine = Children[^1].EndLine;
            EndChar = Children[^1].EndChar;
        }
    }

    public abstract class LeafWrapperNode : ParentNode
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
    }

    public class ProgramNode() : ParentNode
    {
    }

    public class AliasDirective(string alias, string typeName) : ParentNode
    {
        public string Alias { get; } = alias;
        public string TypeName { get; } = typeName;
    }
}
