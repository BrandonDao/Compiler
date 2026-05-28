# Compiler

### Points Of Maintainance

* **Adding New Operators**
    * Create a new leaf node / token
    * Add it to the lexer
    * Add it to `TryGetOperatorPrecedence` so it's recognized as a parser
    * If binary:
        * Update `IsLeftAssociative` if necessary 
        * Update `CreateBinaryOp`
    * If unary:
        * Update `ParsePrimary`
