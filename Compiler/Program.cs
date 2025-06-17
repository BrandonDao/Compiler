using Compiler.Lexer;
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

            RecursiveDescentParser parser = new([.. tokens]); // to array
            var root = new Parser.Nodes.ProgramNode();
            parser.ParseAliasDirective(root);

            Console.WriteLine(root.GetPrintable(0));
        }
    }
}