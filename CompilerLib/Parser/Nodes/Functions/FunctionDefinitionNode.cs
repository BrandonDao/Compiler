using System.Text;
using CompilerLib.Parser.Nodes.Scopes;
using CompilerLib.Parser.Nodes.Types;

namespace CompilerLib.Parser.Nodes.Functions
{
    public class FunctionDefinitionNode :
        SyntaxNode,
        IContainsScopeNode, IGeneratesCode
    {
        public string Name { get; }
        public BlockNode Block => FunctionBlockNode;
        public string ReturnTypeName { get; }

        public readonly FunctionBlockNode FunctionBlockNode;

        private FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SyntaxNode arrow, LeafNode returnTypeNode, string returnTypeName, FunctionBlockNode body)
            : base([func, id, parameterList, arrow, returnTypeNode, body])
        {
            Name = id.Value;
            FunctionBlockNode = body;
            ReturnTypeName = returnTypeName;
            UpdateRange();
        }

        public FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SyntaxNode arrow, TypeLeafNode returnType, FunctionBlockNode body)
            : this(func, id, parameterList, arrow, returnType, returnType.TypeName, body) { }
        public FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SyntaxNode arrow, VoidLeaf voidReturnType, FunctionBlockNode body)
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

        public void GenerateCode(StringBuilder codeBuilder, int indentLevel)
        {
            codeBuilder.AppendIndentedLine(".method public hidebysig static", indentLevel);
            codeBuilder.AppendIndentedLine($"{ReturnTypeName} {Name} (/*PARAMETERS ARE UNSUPPORTED*/) cil managed", indentLevel + 1);
            codeBuilder.AppendIndentedLine("{", indentLevel);
            FunctionBlockNode.GenerateCode(codeBuilder, indentLevel);
            codeBuilder.AppendIndentedLine($"}} // End of method '{Name}'\n", indentLevel);
        }
    }
}