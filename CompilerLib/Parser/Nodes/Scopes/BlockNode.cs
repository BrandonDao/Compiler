using System.Text;
using CompilerLib.Parser.Nodes.Functions;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Punctuation;
using CompilerLib.Parser.Nodes.Statements;
using static CompilerLib.ILGenerator;
using static CompilerLib.SymbolTable;

namespace CompilerLib.Parser.Nodes.Scopes
{
    public abstract class BlockNode
        : SyntaxNode,
        IContainsScopeNode
    {
        public string Name => "Anonymous Block";
        public BlockNode Block => this;
        public uint ID { get; set; }

        public BlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
            : base([openBrace, .. statements, closeBrace])
            => UpdateRange();

        public override SyntaxNode ToAST()
        {
            if (Children.Count == 3 && Children[1] is BlockNode blockNode)
            {
                return blockNode.ToAST();
            }

            Children.RemoveAt(Children.Count - 1); // Remove the close brace
            Children.RemoveAt(0); // Remove the open brace
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
    public abstract class CodeGenBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
        : BlockNode(openBrace, statements, closeBrace),
        IGeneratesCode
    {
        public abstract void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel);
    }

    public class FunctionBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
        : CodeGenBlockNode(openBrace, statements, closeBrace)
    {
        public FunctionInfo? FunctionInfo { get; set; }
        public bool IsEntryPoint { get; set; } = false;

        public override void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel)
        {
            if (FunctionInfo == null) throw new InvalidOperationException("Function Info missing! Semantic analysis failed!");

            ilGen.ResetStackTracking();
            List<(string statement, int indentLevel)> statementInfos = [];

            List<SymbolInfo> locals = new(capacity: FunctionInfo.ChildScopeInfo.SymbolInfoByName.Count);
            Dictionary<string, int> localIdToIndex = new(capacity: FunctionInfo.ChildScopeInfo.SymbolInfoByName.Count);

            for (int i = 0; i < FunctionInfo.ParameterInfos.Count; i++)
            {
                SymbolInfo param = FunctionInfo.ParameterInfos[i];

                localIdToIndex.Add(param.Name, i);
                locals.Add(param);
                statementInfos.Add((ilGen.Emit(OpCode.ldarg, i), indentLevel));
                statementInfos.Add((ilGen.Emit(OpCode.stloc, i), indentLevel));
            }

            foreach (KeyValuePair<string, SymbolInfo> kvp in FunctionInfo.ChildScopeInfo.SymbolInfoByName)
            {
                if (FunctionInfo.ParameterInfos.Any(p => p.Name == kvp.Key)) continue;

                localIdToIndex.Add(kvp.Key, locals.Count);
                locals.Add(kvp.Value);
            }

            foreach (var child in Children)
            {
                if (child is VariableDefinitionNode varDefNode)
                {
                    switch (varDefNode.AssignedValue)
                    {
                        case ValueOperationNode valOp:
                            valOp.GenerateCode(ilGen, symbolTable, FunctionInfo.ChildScopeInfo.ID, statementInfos, indentLevel, localIdToIndex);
                            break;
                        default:
                            ResolveOperand(ilGen, symbolTable, FunctionInfo.ChildScopeInfo.ID, statementInfos, indentLevel, localIdToIndex, varDefNode.AssignedValue);
                            break;
                    }
                    statementInfos.Add((ilGen.Emit(OpCode.stloc, localIdToIndex[varDefNode.NameTypeNode.Name]), indentLevel));
                }
                else if (child is FunctionCallStatementNode funcCallNode)
                {
                    ResolveOperand(ilGen, symbolTable, FunctionInfo.ChildScopeInfo.ID, statementInfos, indentLevel, localIdToIndex, funcCallNode.FunctionCallExpression);
                }
                else if (child is EmptyStatementNode)
                {
                    statementInfos.Add((ilGen.Emit(OpCode.nop), indentLevel));
                }
                else throw new NotImplementedException();
            }

            codeBuilder.AppendIndentedLine($".maxstack {ilGen.MaxStack}", indentLevel);
            if (IsEntryPoint)
            {
                codeBuilder.AppendIndentedLine(".entrypoint // CURRENTLY DETERMINED ONLY BY METHOD NAME", indentLevel);
            }

            codeBuilder.AppendIndentedLine(".locals init ( // ONLY SUPPORTED TYPES ARE: INT32, BOOL", indentLevel);
            indentLevel++;
            for (int i = 0; i < locals.Count - 1; i++)
            {
                SymbolInfo symInfo = locals[i];
                codeBuilder.AppendIndentedLine($"[{i}] {PrimitiveNameMap[symInfo.Type]}, // {symInfo.Name}", indentLevel);
            }
            SymbolInfo lastSymInfo = locals[^1];
            codeBuilder.AppendIndentedLine($"[{locals.Count - 1}] {PrimitiveNameMap[lastSymInfo.Type]} // {lastSymInfo.Name}", indentLevel);
            indentLevel--;
            codeBuilder.AppendIndentedLine(")", indentLevel);

            foreach ((string statement, int statementIndentLevel) in statementInfos)
            {
                codeBuilder.AppendIndentedLine(statement, statementIndentLevel);
            }
            codeBuilder.AppendIndentedLine("ret", indentLevel);
        }
    }
    public class LocalBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
        : BlockNode(openBrace, statements, closeBrace)
    { }

    public class NonLocalBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace) : CodeGenBlockNode(openBrace, statements, closeBrace)
    {
        private readonly List<SyntaxNode> innerStatements = statements;

        public override void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel)
        {
            indentLevel++;
            foreach (var child in innerStatements)
            {
                if (child is FunctionDefinitionNode functionDef)
                {
                    functionDef.GenerateILCode(ilGen, symbolTable, codeBuilder, indentLevel);
                }
                else throw new NotImplementedException();
            }
        }
    }
}