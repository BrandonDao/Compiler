using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Punctuation;

namespace Compiler.Parser
{
    public partial class RecursiveDescentParser : IParser
    {
        public static RecursiveDescentParser Instance { get; private set; } = new();

        private RecursiveDescentParser() { }

        public SyntaxNode? ParseTokens(List<LeafNode> tokens)
        {
            if (tokens.Count == 0) return null;

            int position = 0;
            tokens = HangWhitespace(tokens);
            return IntExpressionNode.ParseMulDiv(tokens, ref position);
        }

        public SyntaxNode ToAST(SyntaxNode root) => throw new NotImplementedException();

        private static List<LeafNode> HangWhitespace(List<LeafNode> tokens)
        {
            List<LeafNode> trimmedTokens = new(capacity: tokens.Count);

            if (tokens[0] is WhitespaceLeaf whitespaceToken)
            {
                whitespaceToken.IsLeading = true;
                tokens[1].Children.Add(tokens[0]);
            }

            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] is not WhitespaceLeaf)
                {
                    trimmedTokens.Add(tokens[i]);
                    continue;
                }

                tokens[i - 1].Children.Add(tokens[i]);
            }

            return trimmedTokens;
        }

        private Program ParseProgram(List<LeafNode> tokens, ref int position)
        {
            throw new NotImplementedException();
        }

        private VariableDefinition ParseVariableDefinition(List<LeafNode> tokens, ref int position)
        {
            throw new NotImplementedException();
        }
        private EqualsValue ParseEqualsValue(List<LeafNode> tokens, ref int position)
        {
            throw new NotImplementedException();
        }


        public class Program() : SyntaxNode;
        public class VariableDefinition() : SyntaxNode;
        public class EqualsValue() : SyntaxNode;


        public class Collapsible() : SyntaxNode;
        public class Epsilon() : Collapsible;
    }
}
