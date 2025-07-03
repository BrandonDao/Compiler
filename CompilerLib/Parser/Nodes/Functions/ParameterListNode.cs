using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes.Functions
{
    public class ParameterListNode : SyntaxNode
    {
        public List<VariableNameTypeNode> Parameters { get; private set; }
        private static List<SyntaxNode> EmptyParams { get; } = [];

        public ParameterListNode(OpenParenthesisLeaf openParen, List<SyntaxNode> parameters, CloseParenthesisLeaf closeParen)
            : base([openParen, .. parameters, closeParen])
        {
            Parameters = new(capacity: parameters.Count);
            foreach (var param in parameters)
            {
                if (param is VariableNameTypeNode variableNameType)
                {
                    Parameters.Add(variableNameType);
                }
                else if (param is CommaLeaf _) { }
                else throw new ArgumentException("Parameters must only contains VariableNameTypeNodes or CommaLeafs!");
            }
            UpdateRange();
        }

        public ParameterListNode(OpenParenthesisLeaf openParen, CloseParenthesisLeaf closeParen)
            : this(openParen, EmptyParams, closeParen) { }

        public override SyntaxNode ToAST()
        {
            Children.Clear();
            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameters[i] = Parameters[i].ToAST();
                Children.Add(Parameters[i]);
            }
            return this;
        }
    }
}