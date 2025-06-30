namespace CompilerLib.Parser.Nodes.Scopes
{
    public interface IContainsScopeNode : IHasChildren
    {
        public string Name { get; }
        public BlockNode Block { get; }
    }
}