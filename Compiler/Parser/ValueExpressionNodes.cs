using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Punctuation;

namespace Compiler.Parser
{
    public abstract class ValueOperationNode : SyntaxNode
    {
        public ValueOperationNode() { }
        public ValueOperationNode(List<SyntaxNode> children) : base(children) { }
    }
    public class ParenthesizedExpression(OpenParenthesisLeaf open, SyntaxNode expr, CloseParenthesisLeaf close)
        : ValueOperationNode([open, expr, close]);


    public class MultiplyOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);
    public class DivideOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);
    public class ModOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);
    public class AddOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);
    public class SubtractOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);


    public class NotOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);
    public class OrOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);
    public class AndOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);
    public class EqualityOperationNode(List<SyntaxNode> children) : ValueOperationNode(children);
}
