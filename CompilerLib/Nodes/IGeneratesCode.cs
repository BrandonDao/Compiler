using System.Text;

namespace CompilerLib.Nodes;

public interface IGeneratesCode
{
    public void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel);
}