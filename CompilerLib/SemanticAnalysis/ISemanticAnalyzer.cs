
using CompilerLib.Nodes;

namespace CompilerLib.SemanticAnalysis;

public interface ISemanticAnalyzer
{
    public bool Analyze(SyntaxNode root, out string completionMessage);

    /// <summary>
    /// For debugging purposes
    /// </summary>
    /// <returns>A human-readable string representation of the current stateof the semantic analyzer</returns>
    public string GetPrintable();
}