namespace CompilerLib.Parser.Nodes
{
    public class EpsilonNode : SyntaxNode
    {
        public static EpsilonNode Instance { get; } = new();
        private EpsilonNode() { }

        public override SyntaxNode ToAST() => this;
    }
}
