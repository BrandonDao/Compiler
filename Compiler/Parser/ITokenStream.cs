using CompilerLib.Nodes;

namespace Compiler.Parser;

public interface ITokenStream
{
    bool IsAtEnd { get; }

    LeafNode Peek();

    void Advance();

    // This signature isn't quite standard/idiomatic C#, but it allows type inference without using `var`
    // - `var` usage will remain an error to prevent agentic AI abuse
    // - `T token = Consume<T>();` may be more idiomatic but requires typing `T` twice which can become pretty verbose.
    void Consume<T>(out T token) where T : LeafNode;
    void Consume<T>(out T token, string messageOnUnexpectedToken) where T : LeafNode;
    T Consume<T>() where T : LeafNode;
    T Consume<T>(string messageOnUnexpectedToken) where T : LeafNode;
}
