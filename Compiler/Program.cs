using System.ComponentModel;
using System.Text;
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
                filePath: @"..\..\..\SampleCode2.txt",
#else
                filePath: @"SampleCode.txt",
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
            var root = parser.ParseTokens(tokens);
            Console.WriteLine(root.GetPrintable());

            Console.WriteLine("CST -> ORIGINAL INPUT");
            StringBuilder builder = new();
            root.FlattenBackToInput(builder);
            Console.WriteLine(builder.ToString());

            // var grammar = GrammarLoader.Load(@"..\..\..\Parser\GrammarDefinition.bnf");
            // var parser = new RecursiveDescentParser([..tokens], grammar);
            // var root = parser.ParseFromGrammar();

            // Console.WriteLine(root.GetPrintable(0));
        }
    }
}