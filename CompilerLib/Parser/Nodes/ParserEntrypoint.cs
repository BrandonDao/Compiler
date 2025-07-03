using System.Numerics;
using System.Text;
using CompilerLib.Parser.Nodes.Scopes;

namespace CompilerLib.Parser.Nodes
{
    public static class StringBuilderExtensions
    {
        public static void AppendIndented(this StringBuilder sb, string text, int indentLevel)
        {
            sb.Append(' ', indentLevel << 2); // 4 spaces per indent level
            sb.Append(text);
        }
        public static void AppendIndentedLine(this StringBuilder sb, string line, int indentLevel)
        {
            sb.Append(' ', indentLevel << 2); // 4 spaces per indent level
            sb.AppendLine(line);
        }
    }
    public class ParserEntrypointNode(NamespaceDefinitionNode child)
        : SyntaxNode([child]),
        IGeneratesCode
    {
        public List<IGeneratesCode> CodeGenChildren => [namespaceChild];

        private readonly NamespaceDefinitionNode namespaceChild = child;

        public void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel)
        {
            codeBuilder.AppendIndentedLine(".assembly _ { }", indentLevel);
            codeBuilder.AppendIndentedLine(".assembly extern System.Runtime { }", indentLevel);
            codeBuilder.AppendIndentedLine(".assembly extern System.Console { }", indentLevel);
            codeBuilder.AppendLine();
            codeBuilder.AppendIndentedLine(".class public auto ansi beforefieldinit Program", indentLevel);
            codeBuilder.AppendIndentedLine("extends [System.Runtime]System.Object", indentLevel + 1);
            codeBuilder.AppendIndentedLine("{", indentLevel);

            // Program's ctor
            codeBuilder.AppendIndentedLine(".method public hidebysig specialname rtspecialname", indentLevel + 1);
            codeBuilder.AppendIndentedLine("instance void .ctor () cil managed", indentLevel + 2);
            codeBuilder.AppendIndentedLine("{", indentLevel + 1);
            codeBuilder.AppendIndentedLine(".maxstack 8", indentLevel + 2);
            codeBuilder.AppendIndentedLine("IL_0000: ldarg.0", indentLevel + 2);
            codeBuilder.AppendIndentedLine("IL_0001: call instance void [System.Runtime]System.Object::.ctor()", indentLevel + 2);
            codeBuilder.AppendIndentedLine("IL_0006: nop", indentLevel + 2);
            codeBuilder.AppendIndentedLine("IL_0007: ret", indentLevel + 2);
            codeBuilder.AppendIndentedLine("} // end of method Program::.ctor\n", indentLevel + 1);

            // methods in the program
            namespaceChild.GenerateILCode(ilGen, symbolTable, codeBuilder, indentLevel);
            codeBuilder.AppendIndentedLine("} // end of class Program", indentLevel);
        }

        public override ParserEntrypointNode ToAST()
        {
            Children[0] = Children[0].ToAST();
            return this;
        }
    }
}