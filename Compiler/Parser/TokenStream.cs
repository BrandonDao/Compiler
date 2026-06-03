using CompilerLib.Nodes;

namespace Compiler.Parser;

internal class TokenStream(List<LeafNode> tokens)
{
    private readonly List<LeafNode> tokens = tokens;
    private int position = 0;

    public bool IsAtEnd => position >= tokens.Count;
    public LeafNode CurrentToken
    {
        get
        {
            if (IsAtEnd)
            {
                throw new ArgumentException("Unexpected end of input!");
            }
            return tokens[position];
        }
    }

    public void Advance() => position++;

    // This signature isn't quite standard/idiomatic C#, but it allows type inference without using `var`
    // - `var` usage will remain an error to prevent agentic AI abuse
    // - `T token = Consume<T>();` may be more idiomatic but requires typing `T` twice which can become pretty verbose.
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
