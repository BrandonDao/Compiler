using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes.Statements
{
    public class VariableDefinitionNode : SyntaxNode
    {
        public VariableNameTypeNode NameTypeNode { get; }

        public SyntaxNode AssignedValue { get; }
        public VariableDefinitionNode(LetKeywordLeaf let, VariableNameTypeNode nameType, AssignmentOperatorLeaf equals, SyntaxNode value, SemicolonLeaf semicolon)
            : base([let, nameType, equals, value, semicolon])
        {
            NameTypeNode = nameType;
            AssignedValue = value;
            UpdateRange();
        }

        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(4); // Remove the semicolon
            Children.RemoveAt(2); // Remove the assignment operator
            Children.RemoveAt(0); // Remove the let keyword
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
}