using System.Text;
using Compiler.Parser;

namespace Compiler
{
    internal class Program
    {
        static void Main()
        {
            RegexLexer lexer = new();

            const string codeFilePath = "SampleCode2.txt";
            var tokens = lexer.TokenizeFile(
#if DEBUG
                filePath: Path.Combine("..", "..", "..", codeFilePath),
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

            Console.WriteLine("DETOKENIZATION");
            Console.WriteLine(RegexLexer.Detokenize(tokens));

            Console.WriteLine("CONCRETE SYNTAX TREE");
            var parser = RecursiveDescentParser.Instance;
            var root = parser.ParseTokens(tokens) ?? throw new ArgumentException("Failed to parse tokens into CST!");
            Console.WriteLine(root.GetPrintable());

            Console.WriteLine("CST -> ORIGINAL INPUT");
            StringBuilder builder = new();
            root.FlattenBackToInput(builder);
            Console.WriteLine(builder.ToString());
        }
    }
}