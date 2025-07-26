using CompilerLib.Nodes.Types;
using static CompilerLib.SymbolTable;

namespace CompilerLib.Nodes.Functions
{
    public class FunctionCallExpressionNode : SyntaxNode
    {
        public IdentifierLeaf Identifier { get; }
        public ArgumentListNode ArgumentList { get; }
        public FunctionInfo? FunctionInfo { get; set; }

        public FunctionCallExpressionNode(IdentifierLeaf id, ArgumentListNode args)
            : base([id, args])
        {
            Identifier = id;
            ArgumentList = args;
            UpdateRange();
        }

        public override SyntaxNode ToAST()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
}