using Compiler.Parser;

namespace Compiler
{
    internal class Program
    {
        static void Main()
        {
            RegexLexer lexer = new();
            var tokens = lexer.TokenizeFile(
#if DEBUG
                filePath: @"..\..\..\SampleCode.txt",
#else
                filePath: @"SampleCode.txt",
#endif
                onUnexpectedToken: (lineIdx, charIdx, val)
                    => Console.WriteLine($"LEXER: Unexpected token at line {lineIdx + 1}.{charIdx + 1}: \"{val}\"")
            );
            Console.WriteLine(RegexLexer.Detokenize(tokens));

            // var grammar = GrammarLoader.Load(@"..\..\..\Parser\GrammarDefinition.bnf");
            // var parser = new RecursiveDescentParser([..tokens], grammar);
            // var root = parser.ParseFromGrammar();

            // Console.WriteLine(root.GetPrintable(0));
        }
    }
}