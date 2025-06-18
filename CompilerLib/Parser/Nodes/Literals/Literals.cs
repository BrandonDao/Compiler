namespace CompilerLib.Parser.Nodes.Literals
{
    public abstract class Literal(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : LeafNode(value, startLine, startChar, endLine, endChar);



    public class IntLiteral(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Literal(value, startLine, startChar, endLine, endChar);
    
    public class BoolLiteral(string value, uint startLine, uint startChar, uint endLine, uint endChar)
        : Literal(value, startLine, startChar, endLine, endChar);
}
