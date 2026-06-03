using System.Text;
using CompilerLib.CodeGen;
using CompilerLib.SemanticAnalysis;

namespace CompilerLib.Nodes;

public interface IGeneratesCode
{
    public void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel);
}