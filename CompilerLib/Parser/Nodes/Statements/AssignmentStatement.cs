using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Punctuation;
using CompilerLib.Parser.Nodes.Types;

namespace CompilerLib.Parser.Nodes.Statements
{
    public class AssignmentStatementNode : SyntaxNode
    {
        public IdentifierLeaf Identifier { get; }
        public SyntaxNode AssignedValue { get; }

        public AssignmentStatementNode(IdentifierLeaf id, AssignmentOperatorLeaf equals, SyntaxNode value, SemicolonLeaf semicolon)
            : base([id, equals, value, semicolon])
        {
            Identifier = id;
            AssignedValue = value;
            UpdateRange();
        }

        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(3); // Remove the semicolon
            Children.RemoveAt(1); // Remove the assignment operator
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
}