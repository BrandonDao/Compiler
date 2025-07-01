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

        protected static void PushStack(ref int maxStack, ref int currentStack)
        {
            currentStack++;
            maxStack = Math.Max(maxStack, currentStack);
        }
        protected static void PopStack(ref int currentStack)
        {
            currentStack--;
        }
    }
    public abstract class CodeGenBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
        : BlockNode(openBrace, statements, closeBrace),
        IGeneratesCode
    {
        public abstract void GenerateCode(StringBuilder codeBuilder, int indentLevel);
    }

    public class FunctionBlockNode(OpenBraceLeaf openBrace, List<SyntaxNode> statements, CloseBraceLeaf closeBrace)
        : CodeGenBlockNode(openBrace, statements, closeBrace)
    {
        public ScopeInfo? ScopeInfo { get; set; }

        public override void GenerateCode(StringBuilder codeBuilder, int indentLevel)
        {
            if (ScopeInfo == null) throw new InvalidOperationException("Function Scope Info missing!");

            indentLevel++;

            int maxStack = 0;
            int currStack = 0;
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
                            valOp.GenerateCode(ref maxStack, ref currStack, localIdToIndex);
                            break;
                        case LiteralLeaf literal:
                            if (literal is IntLiteralLeaf intLiteral)
                            {
                                statementInfos.Add((Emit(OpCode.ldc_i4, intLiteral.Value), indentLevel));
                                PushStack(ref maxStack, ref currStack);
                            }
                            else throw new NotImplementedException();
                            break;

                        default: throw new NotImplementedException();
                    }
                    statementInfos.Add((Emit(OpCode.stloc, localIdToIndex[varDefNode.NameTypeNode.Name]), indentLevel));
                    PopStack(ref currStack);
                }
                else throw new NotImplementedException();
            }

            codeBuilder.AppendIndentedLine($".maxstack {maxStack}", indentLevel);
            codeBuilder.AppendIndentedLine(".entrypoint", indentLevel);

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

        public override void GenerateCode(StringBuilder codeBuilder, int indentLevel)
        {
            indentLevel++;
            foreach (var child in statements)
            {
                if (child is FunctionDefinitionNode functionDef)
                {
                    functionDef.GenerateCode(codeBuilder, indentLevel);
                }
                else throw new NotImplementedException();
            }
        }
    }
}