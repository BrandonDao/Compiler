using CompilerLib.Nodes.Punctuation;

namespace CompilerLib.Nodes.Operators;

public abstract class ValueOperationNode : SyntaxNode
{
    public ValueOperationNode() { }
    public ValueOperationNode(List<SyntaxNode> children) : base(children) { }

    public abstract void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex);

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
public abstract class BinaryOperationNode(List<SyntaxNode> children, string operatorToDisplay)
    : ValueOperationNode(children)
{
    public SyntaxNode LeftOperand => Children[0];
    public SyntaxNode RightOperand => Children[1];
    public string Operator { get; } = operatorToDisplay;
}

public class ParenthesizedExpression(OpenParenthesisLeaf open, SyntaxNode expr, CloseParenthesisLeaf close)
    : SyntaxNode([open, expr, close])
{
    public override SyntaxNode ToAST() => Children[1].ToAST(); // Return the expression inside the parentheses
}

public class MultiplyOperationNode(List<SyntaxNode> children) : BinaryOperationNode(children, "*")
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, LeftOperand);
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, RightOperand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.mul), indentLevel));
    }
}
public class DivideOperationNode(List<SyntaxNode> children) : BinaryOperationNode(children, "/")
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, LeftOperand);
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, RightOperand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.div), indentLevel));
    }
}
public class ModOperationNode(List<SyntaxNode> children) : BinaryOperationNode(children, "%")
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, LeftOperand);
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, RightOperand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.rem), indentLevel));
    }
}

public class AddOperationNode(List<SyntaxNode> children) : BinaryOperationNode(children, "+")
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, LeftOperand);
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, RightOperand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.add), indentLevel));
    }
}
public class SubtractOperationNode(List<SyntaxNode> children) : BinaryOperationNode(children, "-")
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, LeftOperand);
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, RightOperand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.sub), indentLevel));
    }
}

public class NotOperationNode(List<SyntaxNode> children) : UnaryOperationNode(children)
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, Operand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.not), indentLevel));
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.ldc_i4, 1), indentLevel));
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.and), indentLevel));
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
public class OrOperationNode(List<SyntaxNode> children) : BinaryOperationNode(children, "|")
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, LeftOperand);
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, RightOperand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.or), indentLevel));
    }
}

public class AndOperationNode(List<SyntaxNode> children) : BinaryOperationNode(children, "&")
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, LeftOperand);
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, RightOperand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.and), indentLevel));
    }
}

public class EqualityOperationNode(List<SyntaxNode> children) : BinaryOperationNode(children, "==")
{
    public override void GenerateCode(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex)
    {
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, LeftOperand);
        ILGenerator.ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, RightOperand);
        statementInfos.Add((ilGen.Emit(ILGenerator.OpCode.ceq), indentLevel));
    }
}