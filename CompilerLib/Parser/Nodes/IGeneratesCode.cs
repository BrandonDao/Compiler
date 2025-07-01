using System.Text;

namespace CompilerLib.Parser.Nodes
{
    public interface IGeneratesCode
    {
        public void GenerateCode(StringBuilder codeBuilder, int indentLevel);
    }
}