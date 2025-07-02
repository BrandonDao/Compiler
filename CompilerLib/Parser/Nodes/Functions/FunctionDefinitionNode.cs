using System.Text;
using CompilerLib.Parser.Nodes.Scopes;
using CompilerLib.Parser.Nodes.Types;
using static CompilerLib.SymbolTable;

namespace CompilerLib.Parser.Nodes.Functions
{
    public class FunctionDefinitionNode :
        SyntaxNode,
        IContainsScopeNode, IGeneratesCode
    {
        public FunctionInfo? FunctionInfo { get; set; }
        public string Name { get; }
        public ParameterListNode ParameterList { get; }
        public BlockNode Block => FunctionBlockNode;
        public string ReturnTypeName { get; }

        public readonly FunctionBlockNode FunctionBlockNode;

        private FunctionDefinitionNode(FunctionKeywordLeaf func, IdentifierLeaf id, ParameterListNode parameterList, SyntaxNode arrow, LeafNode returnTypeNode, string returnTypeName, FunctionBlockNode body)
            : base([func, id, parameterList, arrow, returnTypeNode, body])
        {
            Name = id.Value;
            ParameterList = parameterList;
            FunctionBlockNode = body;
            FunctionBlockNode.IsEntryPoint = Name == "Main";
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

        public void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel)
        {
            if (FunctionBlockNode.IsEntryPoint)
            {
                codeBuilder.AppendIndentedLine(".method public hidebysig static", indentLevel);
                codeBuilder.AppendIndentedLine($"void 'Main' (/*PARAMETERS ARE UNSUPPORTED*/) cil managed", indentLevel + 1);
            }
            else
            {
                codeBuilder.AppendIndentedLine($".method public hidebysig static", indentLevel);
                if (ILGenerator.PrimitiveNameMap.TryGetValue(ReturnTypeName, out var retPrimitiveTypeName))
                {
                    codeBuilder.AppendIndented(retPrimitiveTypeName, indentLevel + 1);
                }
                else
                {
                    codeBuilder.AppendIndented(ReturnTypeName, indentLevel + 1);
                }
                codeBuilder.AppendLine($" '{Name}' (");

                if (FunctionInfo == null) throw new InvalidOperationException("FunctionInfo is not set for IL code generation! SymbolTable may not have been built correctly.");

                for (int i = 0; true; i++)
                {
                    SymbolInfo param = FunctionInfo.ParameterInfos[i];
                    if (ILGenerator.PrimitiveNameMap.TryGetValue(param.Type, out var paramPrimitiveTypeName))
                    {
                        codeBuilder.AppendIndented($"{paramPrimitiveTypeName} {param.Name}", indentLevel + 1);
                    }
                    else
                    {
                        codeBuilder.AppendIndented($"{param.Type} {param.Name}", indentLevel + 1);
                    }
                    if (i + 1 < FunctionInfo.ParameterInfos.Count)
                    {
                        codeBuilder.AppendLine(", ");
                    }
                    else break;
                }
                codeBuilder.AppendLine(")");
            }
            codeBuilder.AppendIndentedLine("{", indentLevel);
            FunctionBlockNode.GenerateILCode(ilGen, symbolTable, codeBuilder, indentLevel);
            codeBuilder.AppendIndentedLine($"}} // End of method '{Name}'\n", indentLevel);
        }
    }
}