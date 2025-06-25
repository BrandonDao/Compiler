namespace CompilerLib.Parser.Nodes
{
    public abstract class ImplicitNode(string value, int startLine, int startChar) : LeafNode(value, startLine, startChar, startLine, startChar)
    {
        public override string GetPrintable(int indent)
        {
            var indentString = new string(' ', indent);
            return $"[{StartLine}.{StartChar}]\t\t{indentString}{GetType().Name}\n";
        }
    }
}