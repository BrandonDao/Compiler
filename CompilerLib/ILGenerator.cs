using CompilerLib.Parser.Nodes.Types;
using static CompilerLib.SymbolTable;

namespace CompilerLib
{
    public class ILGenerator
    {
        public enum OpCode
        {
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
            [PrimitiveTypeNames.Int32] = "int32"
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
        public string Emit(OpCode opCode, string returnType, string funcName, params string[] parameters)
            => opCode switch
            {
                OpCode.call => $"call {returnType} {funcName}{string.Join(", ", parameters)}",
                _ => throw new NotImplementedException(),
            };
    }
}