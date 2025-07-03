using CompilerLib.Parser.Nodes;
using CompilerLib.Parser.Nodes.Functions;
using CompilerLib.Parser.Nodes.Operators;
using CompilerLib.Parser.Nodes.Types;
using static CompilerLib.SymbolTable;

namespace CompilerLib
{
    public class ILGenerator
    {
        public enum OpCode
        {
            nop = 0x00,
            #region Load Constants
            ldc_i4 = 0x20,
            // ldc_i4_0 = 0x16,
            // ldc_i4_1 = 0x17,
            // ldc_i4_2 = 0x18,
            // ldc_i4_3 = 0x19,
            // ldc_i4_4 = 0x1A,
            // ldc_i4_5 = 0x1B,
            // ldc_i4_6 = 0x1C,
            // ldc_i4_7 = 0x1D,
            // ldc_i4_8 = 0x1E,
            #endregion Load Constants

            call = 0x28,
            ret = 0x2A,

            #region Math
            add = 0x58,
            sub = 0x59,
            mul = 0x5A,
            div = 0x5B,
            rem = 0x5C,
            #endregion Math

            #region Locals
            ldloc = 0xFE0C,
            // ldloc_0 = 0x06,
            // ldloc_1 = 0x07,
            // ldloc_2 = 0x08,
            // ldloc_3 = 0x09,
            stloc = 0xFE0E,
            // stloc_0 = 0x0A,
            // stloc_1 = 0x0B,
            // stloc_2 = 0x0C,
            // stloc_3 = 0x0D,
            #endregion Locals
        }

        public static Dictionary<string, string> PrimitiveNameMap { get; } = new()
        {
            [LanguageNames.Primitives.Int32] = "int32",
            [LanguageNames.Primitives.Bool] = "bool"
        };


        public int MaxStack { get; private set; } = 0;
        private int currentStack = 0;

        public void ResetStackTracking(int maxStack = 0, int currentStack = 0)
        {
            MaxStack = maxStack;
            this.currentStack = currentStack;
        }
        private void PushStack()
        {
            currentStack++;
            MaxStack = Math.Max(MaxStack, currentStack);
        }
        private void PopStack() => currentStack--;

        public string Emit(OpCode opCode)
        {
            switch (opCode)
            {
                case OpCode.nop: return "nop";
                case OpCode.add: PopStack(); return "add";
                case OpCode.sub: PopStack(); return "sub";
                case OpCode.mul: PopStack(); return "mul";
                case OpCode.div: PopStack(); return "div";
                case OpCode.rem: PopStack(); return "rem";
                default: throw new NotImplementedException();
            }
        }
        public string Emit(OpCode opCode, string value)
        {
            if (opCode == OpCode.ldc_i4)
            {
                PushStack();
                return value switch
                {
                    "0" => "ldc.i4.0",
                    "1" => "ldc.i4.1",
                    "2" => "ldc.i4.2",
                    "3" => "ldc.i4.3",
                    "4" => "ldc.i4.4",
                    "5" => "ldc.i4.5",
                    "6" => "ldc.i4.6",
                    "7" => "ldc.i4.7",
                    "8" => "ldc.i4.8",
                    _ => $"ldc.i4 {value}"
                };
            }
            throw new NotImplementedException();
        }
        public string Emit(OpCode opCode, int value)
        {
            switch (opCode)
            {
                case OpCode.stloc:
                    PopStack();
                    return value switch
                    {
                        0 => "stloc.0",
                        1 => "stloc.1",
                        2 => "stloc.2",
                        3 => "stloc.3",
                        _ => $"stloc {value}",
                    };
                case OpCode.ldloc:
                    PushStack();
                    return value switch
                    {
                        0 => "ldloc.0",
                        1 => "ldloc.1",
                        2 => "ldloc.2",
                        3 => "ldloc.3",
                        _ => $"ldloc {value}",
                    };
                default:
                    throw new NotImplementedException();
            }
        }
        public string Emit(OpCode opCode, string funcName, string returnType, IEnumerable<string> parameters)
            => opCode switch
            {
                OpCode.call => $"call {returnType} Program::'{funcName}'({string.Join(", ", parameters)})",
                _ => throw new NotImplementedException(),
            };



        public static void ResolveOperand(ILGenerator ilGen, SymbolTable symbolTable, uint scopeID, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex, SyntaxNode operand)
        {
            if (operand is IdentifierLeaf identifier)
            {
                LoadIdentifier(ilGen, statementInfos, indentLevel, localIdToIndex, identifier);
            }
            else if (operand is ValueOperationNode valueOpNode)
            {
                valueOpNode.GenerateCode(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex);
            }
            else if (operand is LiteralLeaf literal)
            {
                if (literal is IntLiteralLeaf intLiteral)
                {
                    statementInfos.Add((ilGen.Emit(OpCode.ldc_i4, intLiteral.Value), indentLevel));
                }
                else throw new NotImplementedException();
            }
            else if (operand is FunctionCallExpressionNode funcCallExpr)
            {
                if (!symbolTable.TryGetFunctionInfo(scopeID, funcCallExpr.Identifier.Value, out FunctionInfo? funcInfo))
                    throw new InvalidOperationException($"Function '{funcCallExpr.Identifier}' not found in symbol table!");

                List<string> parameterTypes = [];
                foreach (var param in funcInfo.ParameterInfos)
                {
                    if (!PrimitiveNameMap.TryGetValue(param.Type, out string? typeName))
                    {
                        typeName = param.Type;
                    }
                    parameterTypes.Add(typeName);
                }
                foreach (SyntaxNode argument in funcCallExpr.ArgumentList.Children)
                {
                    ResolveOperand(ilGen, symbolTable, scopeID, statementInfos, indentLevel, localIdToIndex, argument);
                }
                statementInfos.Add((ilGen.Emit(OpCode.call, funcInfo.SymbolInfo.Name, funcInfo.SymbolInfo.Type, parameterTypes), indentLevel));
            }
            else throw new NotImplementedException($"Unsupported operand type: {operand.GetType().Name}");
        }
        public static void LoadIdentifier(ILGenerator ilGen, List<(string, int)> statementInfos, int indentLevel, Dictionary<string, int> localIdToIndex, IdentifierLeaf identifier)
        {
            if (!localIdToIndex.TryGetValue(identifier.Value, out int index))
                throw new NotImplementedException($"Non-local identifiers unsupported!");

            statementInfos.Add((ilGen.Emit(OpCode.ldloc, index), indentLevel));
        }
    }
}