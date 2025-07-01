using System.Text;
using CompilerLib.Parser.Nodes.Functions;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Punctuation;
using CompilerLib.Parser.Nodes.Statements;
using CompilerLib.Parser.Nodes.Types;
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
        public abstract void GenerateILCode(ILGenerator ilGen, StringBuilder codeBuilder, int indentLevel);
    }

    public class FunctionBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
        : CodeGenBlockNode(openBrace, statements, closeBrace)
    {
        public bool IsEntryPoint { get; set; } = false;
        public ScopeInfo? ScopeInfo { get; set; }

        public override void GenerateILCode(ILGenerator ilGen, StringBuilder codeBuilder, int indentLevel)
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
                            valOp.GenerateCode(ilGen, localIdToIndex);
                            break;
                        case LiteralLeaf literal:
                            if (literal is IntLiteralLeaf intLiteral)
                            {
                                statementInfos.Add((ilGen.Emit(OpCode.ldc_i4, intLiteral.Value), indentLevel));
                            }
                            else throw new NotImplementedException();
                            break;

                        default: throw new NotImplementedException();
                    }
                    statementInfos.Add((ilGen.Emit(OpCode.stloc, localIdToIndex[varDefNode.NameTypeNode.Name]), indentLevel));
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
            for (int i = 0; i < locals.Count; i++)
            {
                SymbolInfo symInfo = locals[i];
                codeBuilder.AppendIndentedLine($"[{i}] {PrimitiveNameMap[symInfo.Type]} // {symInfo.Name}", indentLevel);
            }
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

    public class NonLocalBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
        : CodeGenBlockNode(openBrace, statements, closeBrace)
    {
        private readonly List<SyntaxNode> statements = statements;

        public override void GenerateILCode(ILGenerator ilGen, StringBuilder codeBuilder, int indentLevel)
        {
            indentLevel++;
            foreach (var child in statements)
            {
                if (child is FunctionDefinitionNode functionDef)
                {
                    functionDef.GenerateILCode(ilGen, codeBuilder, indentLevel);
                }
                else throw new NotImplementedException();
            }
        }
    }
}