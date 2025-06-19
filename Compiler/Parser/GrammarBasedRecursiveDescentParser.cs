// using CompilerLib.Parser.Nodes;

// namespace Compiler.Parser
// {
//     public interface IParser
//     {
//         public SyntaxNode ParseTokens(List<LeafNode> tokens);
//         public SyntaxNode ToAST(SyntaxNode root);
//     }

//     public class GrammarBasedRecursiveDescentParser(Grammar grammar) : IParser
//     {
//         Grammar grammar = grammar;
//         int position;

//         public SyntaxNode ParseTokens(List<LeafNode> tokens)
//             => ParseRule(grammar.Rules[grammar.EntryPointName]);

//         public SyntaxNode ToAST(SyntaxNode root) => throw new NotImplementedException();


//         private SyntaxNode ParseRule(GrammarRule rule)
//         {
//             throw new NotImplementedException();
//             int start = position;

            
//         }
//     }
// }
