namespace CompilerLib.Parser.Nodes
{
    public class ParserEntrypointNode(SyntaxNode child) : SyntaxNode([child])
    {
        public override SyntaxNode ToAST()
        {
            Children[0] = Children[0].ToAST();
            return this;
        }
    }
}