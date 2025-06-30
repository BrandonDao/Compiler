using CompilerLib.Parser.Nodes.Scopes;
using CompilerLib.Parser.Nodes.Types;

namespace CompilerLib.Parser.Nodes.Functions
{
    public class FunctionDefinitionNode : SyntaxNode, IContainsScopeNode
    {
        public string Name { get; }
        public BlockNode Block { get; }
        public string ReturnTypeName { get; }
        private FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SyntaxNode arrow, LeafNode returnTypeNode, string returnTypeName, BlockNode body)
            : base([func, id, parameterList, arrow, returnTypeNode, body])
        {
            Name = id.Value;
            Block = body;
            ReturnTypeName = returnTypeName;
            UpdateRange();
        }

        public FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SyntaxNode arrow, TypeLeafNode returnType, BlockNode body)
            : this(func, id, parameterList, arrow, returnType, returnType.TypeName, body) { }
        public FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SyntaxNode arrow, VoidLeaf voidReturnType, BlockNode body)
            : this(func, id, parameterList, arrow, voidReturnType, "void", body) { }

        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(3); // Remove the arrow
            Children.RemoveAt(0); // Remove the function keyword
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
}