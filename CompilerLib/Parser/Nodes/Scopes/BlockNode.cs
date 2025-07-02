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
        public bool IsEntryPoint { get; set; } = false;
        public ScopeInfo? ScopeInfo { get; set; }

        public override void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel)
        {
            if (ScopeInfo == null) throw new InvalidOperationException("Function Scope Info missing!");

            indentLevel++;

            ilGen.ResetStackTracking();
            List<(string statement, int indentLevel)> statementInfos = [];

            Dictionary<string, int> localIdToIndex = new(capacity: ScopeInfo.SymbolInfoByName.Count);
            List<SymbolInfo> locals = new(capacity: ScopeInfo.SymbolInfoByName.Count);
            foreach (KeyValuePair<string, SymbolInfo> kvp in ScopeInfo.SymbolInfoByName)
            {
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
                            valOp.GenerateCode(ilGen, symbolTable, ScopeInfo.ID, statementInfos, indentLevel, localIdToIndex);
                            break;
                        default:
                            ResolveOperand(ilGen, symbolTable, ScopeInfo.ID, statementInfos, indentLevel, localIdToIndex, varDefNode.AssignedValue);
                            break;
                    }
                    statementInfos.Add((ilGen.Emit(OpCode.stloc, localIdToIndex[varDefNode.NameTypeNode.Name]), indentLevel));
                }
                else if (child is FunctionCallStatementNode funcCallNode)
                {
                    ResolveOperand(ilGen, symbolTable, ScopeInfo.ID, statementInfos, indentLevel, localIdToIndex, funcCallNode.FunctionCallExpression);
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

            codeBuilder.AppendIndentedLine(".locals init ( // ONLY SUPPORTED TYPES ARE: INT32", indentLevel);
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