// using Compiler.Lexer;
// using Compiler.Parser.Nodes;
// using Compiler.Parser.Nodes.Identifiers;
// using Compiler.Parser.Nodes.Keywords;
// using Compiler.Parser.Nodes.Operators;
// using Compiler.Parser.Nodes.Primitives;
// using Compiler.Parser.Nodes.Punctuation;
// using Compiler.Parser.Nodes.Whitespace;
// using Compiler.Shared.Lexer;
// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Diagnostics.CodeAnalysis;

// using Boolean = Compiler.Parser.Nodes.Primitives.Boolean;
// using Int16 = Compiler.Parser.Nodes.Primitives.Int16;
// using Int32 = Compiler.Parser.Nodes.Primitives.Int32;
// using Int64 = Compiler.Parser.Nodes.Primitives.Int64;

// namespace Compiler.Parser
// {
//     public class RecursiveDescentParser
//     {
//         private readonly Token[] tokens;
//         private readonly Grammar grammar;
//         private int position = 0;
//         private readonly Dictionary<string, Func<SyntaxNode>> ruleParsers;

//         public RecursiveDescentParser(Token[] tokens, Grammar grammar)
//         {
//             this.tokens = tokens;
//             this.grammar = grammar;
//             ruleParsers = new()
//             {
//                 { "Program", ParseProgram },
//                 // Add more hand-written mappings as you implement them
//             };
//         }

//         public SyntaxNode ParseFromGrammar()
//         {
//             return ParseRule(grammar.EntryPoint);
//         }

//         public SyntaxNode ParseRule(string ruleName)
//         {
//             if (ruleParsers.TryGetValue(ruleName, out var parser))
//                 return parser();

//             if (!grammar.Rules.TryGetValue(ruleName, out var rule))
//                 throw new InvalidOperationException($"Unknown rule: {ruleName}");

//             foreach (var alternative in rule.Alternatives)
//             {
//                 var savePos = position;
//                 var children = new List<SyntaxNode>();
//                 bool success = true;
//                 foreach (var symbol in alternative)
//                 {
//                     if (grammar.Rules.ContainsKey(symbol))
//                     {
//                         var node = ParseRule(symbol);
//                         if (node == null) { success = false; break; }
//                         children.Add(node);
//                     }
//                     else if (symbol == "empty")
//                     {
//                         continue;
//                     }
//                     else
//                     {
//                         if (!MatchTerminal(symbol, out var node))
//                         {
//                             success = false;
//                             break;
//                         }
//                         if (node != null) children.Add(node);
//                     }
//                 }
//                 if (success)
//                 {
//                     return new GenericSyntaxNode(ruleName, children);
//                 }
//                 position = savePos;
//             }
//             return null;
//         }

//         private bool MatchTerminal(string symbol, out SyntaxNode node)
//         {
//             node = null;
//             if (IsAtEnd()) return false;
//             var token = tokens[position];
//             // Remove quotes for literal match
//             var literal = symbol.Trim('"');
//             if (token.Value == literal)
//             {
//                 node = new TerminalNode(token);
//                 position++;
//                 return true;
//             }
//             // TODO: Add mapping for regex/terminal types if needed
//             return false;
//         }

//         public SyntaxNode ParseProgram()
//         {
//             ProgramNode program = new();
//             ParseWhitespace(program.Children, true);

//             TryParseAliasDirectives(program);

//             // while (!IsAtEnd())
//             // {
//             if (Check(TokenType.Func))
//             {
//                 program.Children.Add(ParseFunctionDeclaration());
//             }
//             // }

//             program.UpdateRange();
//             return program;
//         }

//         private void TryParseAliasDirectives(ProgramNode programNode)
//         {
//             if (!Match(TokenType.Alias, out Token? usingToken)) return;

//             AliasKeyword usingKeyword = new(usingToken);
//             ParseWhitespace(usingKeyword.Children, false);

//             if (!Match(TokenType.Identifier, out Token? aliasIdentifierToken))
//                 throw new InvalidOperationException($"Expected identifier after 'alias' keyword, found instead: {tokens[position]}!");

//             IdentifierName aliasIdentifier = new(aliasIdentifierToken);
//             ParseWhitespace(aliasIdentifier.Children, false);
//             aliasIdentifier.UpdateRange();

//             if (!Match(TokenType.AssignmentOperator, out Token? assignmentOpToken))
//                 throw new InvalidOperationException($"Expected assignment operator '=' in alias directive, found instead: {tokens[position]}!");

//             AssignmentOperator assignmentOperator = new(assignmentOpToken);
//             ParseWhitespace(assignmentOperator.Children, false);
//             assignmentOperator.UpdateRange();

//             (LeafWrapperNode originalTypeOrIdentifier, Token? originalIdentifierToken) = ParseType();

//             if (!Match(TokenType.Semicolon, out Token? semicolonToken))
//                 throw new InvalidOperationException($"Expected semicolon ';' at the end of alias directive, found instead: {tokens[position]}!");

//             Semicolon semicolon = new(semicolonToken);
//             ParseWhitespace(semicolon.Children, false);
//             semicolon.UpdateRange();

//             AliasDirective aliasDirective = new(aliasIdentifier.Name, originalIdentifierToken.Value);
//             aliasDirective.Children.Add(usingKeyword);
//             aliasDirective.Children.Add(aliasIdentifier);
//             aliasDirective.Children.Add(assignmentOperator);
//             aliasDirective.Children.Add(originalTypeOrIdentifier);
//             aliasDirective.Children.Add(semicolon);
//             aliasDirective.UpdateRange();

//             programNode.Children.Add(aliasDirective);
//             TryParseAliasDirectives(programNode);
//         }

//         private FunctionDeclaration ParseFunctionDeclaration()
//         {
//             Token funcToken = tokens[position++];
//             FuncKeyword funcKeyword = new(funcToken);
//             ParseWhitespace(funcKeyword.Children, false);

//             (LeafWrapperNode returnType, _) = ParseReturnType();

//             if (!Match(TokenType.Identifier, out Token? functionNameIdToken))
//                 throw new InvalidOperationException($"Expected identifier for function name, found instead: {tokens[position]}!");

//             IdentifierName functionNameId = new(functionNameIdToken);
//             ParseWhitespace(functionNameId.Children, false);
//             functionNameId.UpdateRange();

//             ParameterList parameterList = ParseParameterList();

//             Block block = ParseBlock();

//             return new FunctionDeclaration(functionNameId, returnType, parameterList, block);

//             ParameterList ParseParameterList()
//             {
//                 if (!Match(TokenType.OpenParenthesis, out Token? openParenToken))
//                     throw new InvalidOperationException($"Expected '(' to start parameter list, found instead: {tokens[position]}!");

//                 ParameterList parameterList = new();

//                 OpenParenthesis openParenthesis = new(openParenToken);
//                 ParseWhitespace(openParenthesis.Children, false);
//                 openParenthesis.UpdateRange();

//                 while (!Check(TokenType.CloseParenthesis))
//                 {
//                     Parameter parameter = ParseParameter();
//                     parameterList.Children.Add(parameter);

//                     if (Match(TokenType.Comma, out Token? commaToken))
//                     {
//                         Comma comma = new(commaToken);
//                         ParseWhitespace(comma.Children, false);
//                         comma.UpdateRange();
//                         parameterList.Children.Add(comma);
//                     }
//                     else if (Check(TokenType.CloseParenthesis))
//                     {
//                         Token closeParenToken = Advance();
//                         CloseParenthesis closeParenthesis = new(closeParenToken);
//                         ParseWhitespace(closeParenthesis.Children, false);
//                         closeParenthesis.UpdateRange();
//                         break;
//                     }
//                     else throw new InvalidOperationException($"Expected ',' or ')' after parameter, found instead: {tokens[position]}!");
//                 }

//                 return parameterList;

//                 Parameter ParseParameter()
//                 {
//                     (LeafWrapperNode type, _) = ParseType();
//                     ParseWhitespace(type.Children, false);
//                     type.UpdateRange();

//                     if (!Match(TokenType.Identifier, out Token? paramNameToken))
//                         throw new InvalidOperationException($"Expected identifier for parameter name, found instead: {tokens[position]}!");

//                     IdentifierName id = new(paramNameToken);
//                     ParseWhitespace(id.Children, false);
//                     id.UpdateRange();

//                     return new Parameter(id, type);
//                 }
//             }
//         }

//         private (LeafWrapperNode type, Token token) ParseType()
//         {
//             LeafWrapperNode originalType;
//             if (Match(TokenType.Identifier, out Token? token))
//             {
//                 originalType = new IdentifierName(token);
//             }
//             else if (Match(TokenType.Int8, out token))
//             {
//                 originalType = new Int8(token);
//             }
//             else if (Match(TokenType.Int16, out token))
//             {
//                 originalType = new Int16(token);
//             }
//             else if (Match(TokenType.Int32, out token))
//             {
//                 originalType = new Int32(token);
//             }
//             else if (Match(TokenType.Int64, out token))
//             {
//                 originalType = new Int64(token);
//             }
//             else if (Match(TokenType.Boolean, out token))
//             {
//                 originalType = new Boolean(token);
//             }
//             else throw new InvalidOperationException($"Expected type (identifier or primitive), found instead: {tokens[position]}!");

//             ParseWhitespace(originalType.Children, false);
//             originalType.UpdateRange();
//             return (originalType, token);
//         }
//         private (LeafWrapperNode type, Token token) ParseReturnType()
//         {
//             if (Match(TokenType.Void, out Token? token))
//             {
//                 LeafWrapperNode voidType = new VoidKeyword(token);
//                 ParseWhitespace(voidType.Children, false);
//                 voidType.UpdateRange();
//                 return (voidType, token);
//             }

//             return ParseType();
//         }

//         private Block ParseBlock()
//         {
// #if DEBUG
//             return new Block();
// #else
//             throw new NotImplementedException("Block parsing is not yet implemented.");
// #endif
//         }

//         private int ParseWhitespace(List<SyntaxNode> children, bool isLeading)
//         {
//             if (Match(TokenType.Whitespace, out Token? token))
//             {
//                 children.Add(new Whitespace(token, isLeading));
//                 return 1;
//             }
//             return 0;
//         }

//         // Utility methods
//         private bool Match(TokenType type, [NotNullWhen(true)] out Token? token)
//         {
//             if (Check(type))
//             {
//                 token = Advance();
//                 return true;
//             }
//             token = null;
//             return false;
//         }
//         private bool Check(TokenType type)
//         {
//             if (IsAtEnd()) return false;

//             return tokens[position].Type == type;
//         }
//         private Token Advance()
//         {
//             if (!IsAtEnd()) position++;

//             return tokens[position - 1];
//         }
//         private bool IsAtEnd() => position >= tokens.Length;

//         // Placeholder node for dynamically parsed rules
//         public class GenericSyntaxNode : SyntaxNode
//         {
//             public string RuleName { get; }
//             public new List<SyntaxNode> Children { get; }
//             public GenericSyntaxNode(string ruleName, List<SyntaxNode> children)
//             {
//                 RuleName = ruleName;
//                 Children = children;
//             }
//         }

//         // Placeholder node for matched terminals
//         public class TerminalNode : SyntaxNode
//         {
//             public Token Token { get; }
//             public TerminalNode(Token token)
//             {
//                 Token = token;
//             }
//         }
//     }
// }
