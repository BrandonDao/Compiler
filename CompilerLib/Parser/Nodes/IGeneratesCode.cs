using System.Text;

namespace CompilerLib.Parser.Nodes
{
    public interface IGeneratesCode
    {
        public void GenerateILCode(ILGenerator ilGen, StringBuilder codeBuilder, int indentLevel);
    }
}