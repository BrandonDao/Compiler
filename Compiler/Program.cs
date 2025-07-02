using System.Text;
using Compiler.Parser;
using Compiler.SemanticAnalysis;
using CompilerLib;

namespace Compiler
{
    internal class Program
    {
        static void Main()
        {
            RegexLexer lexer = new();

            const string relativeCodeFilePath = @"SampleCode\SampleCode.txt";
            const string relativeILFilePath = @"GeneratedIL.txt";

            var tokens = lexer.TokenizeFile(
#if DEBUG
                filePath: Path.Combine("..", "..", "..", relativeCodeFilePath),
#else
                filePath: codeFilePath,
#endif
                onUnexpectedToken: (lineIdx, charIdx, val)
                    => Console.WriteLine($"LEXER: Unexpected token at line {lineIdx + 1}.{charIdx + 1}: \"{val}\"")
            );

            Console.WriteLine("TOKENIZATION");
            foreach (var token in tokens)
            {
                Console.Write(token.GetPrintable());
            }

            Console.WriteLine("\nDETOKENIZATION");
            Console.WriteLine(RegexLexer.Detokenize(tokens));

            Console.WriteLine("\nCONCRETE SYNTAX TREE");
            var parser = RecursiveDescentParser.Instance;
            var cstRoot = parser.ParseTokensToCST(tokens) ?? throw new ArgumentException("Failed to parse tokens into CST!");
            Console.WriteLine(cstRoot.GetPrintable());

            Console.WriteLine("CST -> ORIGINAL INPUT");
            StringBuilder builder = new();
            cstRoot.FlattenBackToInput(builder);
            Console.WriteLine(builder.ToString());

            Console.WriteLine("\nCST -> AST");
            var astRoot = parser.ParseCSTToAST(cstRoot) ?? throw new ArgumentException("Failed to convert CST to AST!");
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
            Console.WriteLine(generatedIL);

            File.WriteAllText(
                Path.Combine("..", "..", "..", relativeILFilePath),
                generatedIL
            );
        }
    }
}