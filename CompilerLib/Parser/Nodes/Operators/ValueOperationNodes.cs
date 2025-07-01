using CompilerLib.Parser.Nodes.Punctuation;
using CompilerLib.Parser.Nodes.Types;

namespace CompilerLib.Parser.Nodes.Operators
{
    public abstract class ValueOperationNode : SyntaxNode
    {
        public ValueOperationNode() { }
        public ValueOperationNode(List<SyntaxNode> children) : base(children) { }

        public abstract void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex);

        public override SyntaxNode ToAST()
        {
            Children.RemoveAt(1); // Remove operator

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].ToAST();
            }

            return this;
        }

        public static void ResolveOperand(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex, SyntaxNode operand)
        {
            if (operand is IdentifierLeaf identifier)
            {
                LoadIdentifier(ilGen, statementInfos, indentLevel, localIdToIndex, identifier);
            }
            else if (operand is ValueOperationNode valueOpNode)
            {
                valueOpNode.GenerateCode(ilGen, statementInfos, indentLevel, localIdToIndex);
            }
            else if (operand is LiteralLeaf literal)
            {
                if (literal is IntLiteralLeaf intLiteral)
                {
                    statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.ldc_i4, intLiteral.Value), indentLevel));
                }
                else throw new NotImplementedException();
            }
            else throw new NotImplementedException($"Unsupported operand type: {operand.GetType().Name}");
        }
        protected static void LoadIdentifier(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex, IdentifierLeaf identifier)
        {
            if (!localIdToIndex.TryGetValue(identifier.Value, out int index))
                throw new NotImplementedException($"Non-local identifiers unsupported!");

            statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.ldloc, index), indentLevel));
        }
    }
    public abstract class UnaryOperationNode(List<SyntaxNode> children) : ValueOperationNode(children)
    {
        public SyntaxNode Operand { get; } = children[1];
    }
    public abstract class BinaryOperationNode(List<SyntaxNode> children, string operatorToDisplay)
        : ValueOperationNode(children)
    {
        public SyntaxNode LeftOperand => Children[0];
        public SyntaxNode RightOperand => Children[1];
        public string Operator { get; } = operatorToDisplay;
    }
    public abstract class LowPrecedenceOperationNode(List<SyntaxNode> children, string operatorToDisplay)
        : BinaryOperationNode(children, operatorToDisplay);
    public abstract class HighPrecedenceOperationNode(List<SyntaxNode> children, string operatorToDisplay)
        : BinaryOperationNode(children, operatorToDisplay);

    public class ParenthesizedExpression(OpenParenthesisLeaf open, SyntaxNode expr, CloseParenthesisLeaf close)
        : SyntaxNode([open, expr, close])
    {
        public override SyntaxNode ToAST() => Children[1].ToAST(); // Return the expression inside the parentheses
    }

    public class MultiplyOperationNode(List<SyntaxNode> children) : HighPrecedenceOperationNode(children, "*")
    {
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
        {
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, LeftOperand);
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, RightOperand);
            statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.mul), indentLevel));
        }
    }
    public class DivideOperationNode(List<SyntaxNode> children) : HighPrecedenceOperationNode(children, "/")
    {
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
        {
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, LeftOperand);
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, RightOperand);
            statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.div), indentLevel));
        }
    }
    public class ModOperationNode(List<SyntaxNode> children) : HighPrecedenceOperationNode(children, "%")
    {
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
        {
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, LeftOperand);
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, RightOperand);
            statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.rem), indentLevel));
        }
    }

    public class AddOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "+")
    {
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
        {
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, LeftOperand);
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, RightOperand);
            statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.add), indentLevel));
        }
    }
    public class SubtractOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "-")
    {
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
        {
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, LeftOperand);
            ResolveOperand(ilGen, statementInfos, indentLevel, localIdToIndex, RightOperand);
            statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.sub), indentLevel));
        }
    }

    public class NotOperationNode(List<SyntaxNode> children) : UnaryOperationNode(children)
    {
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
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
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class AndOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "&")
    {
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class EqualityOperationNode(List<SyntaxNode> children) : LowPrecedenceOperationNode(children, "==")
    {
        public override void GenerateCode(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
        {
            throw new NotImplementedException();
        }
    }
}