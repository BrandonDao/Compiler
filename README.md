# Compiler

### Points Of Maintainance

* **Adding New Operators**
    * Create a new leaf node / token
    * Add it to the lexer
    * If infix:
        * Add it to `TryGetInfixPrecedence` so it's recognized as a parser
        * Update `IsLeftAssociative` if necessary 
        * Update `CreateBinaryOp`
    * If prefix:
        * Update `ParsePrefix`
