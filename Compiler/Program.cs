using Compiler.Lexer;

namespace Compiler
{
    internal class Program
    {
        static void Main()
        {
            RegexLexer lexer = new();
            var a = lexer.TokenizeFile(@"..\..\..\SampleCode.txt");

            foreach (var token in a)
            {
                Console.WriteLine(token);
            }
        }
    }
}