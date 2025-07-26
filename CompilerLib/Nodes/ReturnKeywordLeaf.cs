using CompilerLib.Nodes.Punctuation;

namespace CompilerLib.Nodes
{
    public class ReturnKeywordLeaf(string value, int startLine, int startChar, int endLine, int endChar)
        : KeywordLeaf(value, startLine, startChar, endLine, endChar);

    public class ReturnStatementNode : SyntaxNode
    {
        public SyntaxNode? ReturnValue { get; private set; }

        public ReturnStatementNode(ReturnKeywordLeaf returnKeyword, SyntaxNode returnValue, SemicolonLeaf semicolon)
            : base([returnKeyword, returnValue, semicolon])
        {
            ReturnValue = returnValue;
        }
        public ReturnStatementNode(ReturnKeywordLeaf returnKeyword, SemicolonLeaf semicolon)
            : base([returnKeyword, semicolon])
        {
            ReturnValue = null;
        }

        public override SyntaxNode ToAST()
        {
            if (ReturnValue == null)
            {
                Children.Clear(); // Remove the return keyword and semicolon
            }
            else
            {
                Children.RemoveAt(2); // Remove last 2 children
                Children.RemoveAt(1);
                ReturnValue = ReturnValue.ToAST();
                Children[0] = ReturnValue; // Update the first child to be the return value
            }
            return this;
        }
    }
}