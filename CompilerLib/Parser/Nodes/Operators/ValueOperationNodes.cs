using CompilerLib.Parser.Nodes.Punctuation;

namespace CompilerLib.Parser.Nodes.Operators
{
    public abstract class ValueOperationNode : SyntaxNode
    {
        public ValueOperationNode() { }
        public ValueOperationNode(List<SyntaxNode> children) : base(children) { }

        public abstract void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex);

        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(1); // Remove operator

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }

            return this;
        }
    }
    public abstract class UnaryOperationNode(List<SyntaxNode> children) : ValueOperationNode(children)
    {
        public SyntaxNode Operand { get; } = children[1];
    }
    public abstract class BinaryOperationNode(List<SyntaxNode> children, string operatorToDisplay) : ValueOperationNode(children)
    {
        public SyntaxNode LeftOperand => Children[0];
        public  SyntaxNode RightOperand => Children[1];
        public string Operator { get; } = operatorToDisplay;
    }
    public abstract class LowPrecedenceOperationNode(List<SyntaxNode> children, string operatorToDisplay) : BinaryOperationNode(children, operatorToDisplay);
    public abstract class HighPrecedenceOperationNode(List<SyntaxNode> children, string operatorToDisplay) : BinaryOperationNode(children, operatorToDisplay);

    public class ParenthesizedExpression(OpenParenthesisLeaf open, SyntaxNode expr, CloseParenthesisLeaf close)
        : ValueOperationNode([open, expr, close])
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode ToAST()
        {
            return Children[1].ToAST(); // Return the expression inside the parentheses
        }
    }

    public class MultiplyOperationNode(List<SyntaxNode> children) : HighPrecedenceOperationNode(children, "*")
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class DivideOperationNode(List<SyntaxNode> children) : HighPrecedenceOperationNode(children, "/")
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class ModOperationNode(List<SyntaxNode> children) : HighPrecedenceOperationNode(children, "%")
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class AddOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "+")
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class SubtractOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "-")
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class NotOperationNode(List<SyntaxNode> children) : UnaryOperationNode(children)
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode ToAST()
        {
            // Remove the 'not' operator
            Children.RemoveAt(0);
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }
            return this;
        }
    }
    public class OrOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "|")
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class AndOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "&")
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class EqualityOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "==")
    {
        public override void GenerateCode(ILGenerator ilGen, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }
}