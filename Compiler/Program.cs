using System.Text;
using Compiler.Lexer;
using Compiler.Parser;
using Compiler.SemanticAnalysis;
using CompilerLib;
using CompilerLib.Nodes;

namespace Compiler;

internal class Program
{
    static void Main()
    {
        RegexLexer lexer = new();

        const string relativeCodeFilePath = @"SampleCode\PassTest.txt";
        const string relativeILFilePath = @"GeneratedIL.txt";

        List<LeafNode> tokens = lexer.TokenizeFile(
#if DEBUG
            filePath: Path.Combine("..", "..", "..", relativeCodeFilePath),
#else
            filePath: codeFilePath,
#endif
            onUnexpectedToken: (lineIdx, charIdx, val)
                => Console.WriteLine($"LEXER: Unexpected token at line {lineIdx + 1}.{charIdx + 1}: \"{val}\"")
        );

        Console.WriteLine("TOKENIZATION");
        foreach (LeafNode token in tokens)
        {
            Console.Write(token.GetPrintable());
        }

        Console.WriteLine("\nDETOKENIZATION");
        Console.WriteLine(RegexLexer.Detokenize(tokens));

        Console.WriteLine("\nCONCRETE SYNTAX TREE");
        RecursiveDescentParser parser = RecursiveDescentParser.Instance;
        ParserEntrypointNode cstRoot = parser.ParseTokensToCST(tokens);
        Console.WriteLine(cstRoot.GetPrintable());

        Console.WriteLine("CST -> ORIGINAL INPUT");
        StringBuilder builder = new();
        cstRoot.FlattenBackToInput(builder);
        Console.WriteLine(builder.ToString());

        Console.WriteLine("\nCST -> AST");
        ParserEntrypointNode astRoot = parser.ParseCSTToAST(cstRoot);
        Console.WriteLine(astRoot.GetPrintable());

        SemanticAnalyzer analyzer = new();
        bool _ = analyzer.Analyze(astRoot, out string semanticAnalyzerCompletionMessage);

        Console.WriteLine("\nSYMBOL TABLE");
        Console.WriteLine(semanticAnalyzerCompletionMessage);
        Console.WriteLine();
        Console.WriteLine(analyzer.GetPrintable());

        Console.WriteLine("\nCODE GENERATION");
        StringBuilder codeBuilder = new();
        ILGenerator iLGenerator = new();
        astRoot.GenerateILCode(iLGenerator, analyzer.SymbolTable, codeBuilder, indentLevel: 0);

        string generatedIL = codeBuilder.ToString();
        //Console.WriteLine(generatedIL);

        File.WriteAllText(
            Path.Combine("..", "..", "..", relativeILFilePath),
            generatedIL
        );
    }
}