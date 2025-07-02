using System.Text;

namespace CompilerLib.Parser.Nodes
{
    public interface IGeneratesCode
    {
        public void GenerateILCode(ILGenerator ilGen, SymbolTable symbolTable, StringBuilder codeBuilder, int indentLevel);
    }
}