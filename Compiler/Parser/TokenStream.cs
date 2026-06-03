using CompilerLib.Lexer;
using CompilerLib.Nodes;

namespace Compiler.Parser;

internal class TokenStream(List<LeafNode> tokens) : ITokenStream
{
    private readonly List<LeafNode> tokens = tokens;
    private int position = 0;

    public bool IsAtEnd => position >= tokens.Count;
    public LeafNode Peek() => IsAtEnd
        ? throw new InvalidOperationException("Unexpected end of input!")
        : tokens[position];

    public void Advance() => position++;

    public void Consume<T>(out T token) where T : LeafNode
        => token = Consume<T>();
    public void Consume<T>(out T token, string messageOnUnexpectedToken) where T : LeafNode
        => token = Consume<T>(messageOnUnexpectedToken);
    public T Consume<T>() where T : LeafNode
        => Consume<T>($"Expected token of type {typeof(T).Name}, not {tokens[position]}!");
    public T Consume<T>(string messageOnUnexpectedToken) where T : LeafNode
    {
        if (IsAtEnd)
        {
            throw new ArgumentException($"Unexpected end of input, expected token of type {typeof(T).Name}!");
        }
        if (tokens[position] is not T token)
        {
            throw new ArgumentException(messageOnUnexpectedToken);
        }
        position++;
        return token;
    }
}
