# Compiler *(Still a WIP!)*

### A Brief Overview
A compiler for my custom language, codegen targetting CIL. The language is inspired by C# and TypeScript, and this compiler mainly serves as a learning project. This particular language will probably never see the light of day given it's heavily overshadowed by more popular OOP languages, but I plan to use the concepts which are directly transferrable to other projects.

The language grammar is close to LL(1) which made a handwritten parser simple enough, but I recently started refactoring the convoluted code with some more elegant approaches *(Pratt Parsing, more helper functions, etc)*. I also want to try using a tool like ANTLR to generate a parser for me, to see how my handwritten one compares.

Once the refactoring is done, I plan on adding C++ style templates and C#-style inheritance afterward.

# Developer Notes
### Points Of Maintainance

* **Adding New Operators**
    * Create a new leaf node / token
    * Add it to the lexer
    * If infix:
        * Add it to `TryGetInfixPrecedence` so it's recognized as an operator
        * Update `IsLeftAssociative` if necessary 
        * Update `CreateBinaryOp`
    * If prefix:
        * Update `ParsePrefix`
