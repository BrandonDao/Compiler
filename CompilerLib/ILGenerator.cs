using CompilerLib.Parser.Nodes.Types;

namespace CompilerLib
{
    public class ILGenerator
    {
        public enum OpCode
        {
            ldc_i4 = 0x20,
            // stloc_0 = 0x0A,
            // stloc_1 = 0x0B,
            // stloc_2 = 0x0C,
            // stloc_3 = 0x0D,
            stloc = 0xFE0E,
        }

        public static Dictionary<string, string> PrimitiveNameMap { get; } = new()
        {
            [PrimitiveTypeNames.Int32] = "int32"
        };


        public int MaxStack { get;  private set; } = 0;
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

        public string Emit(OpCode opCode, string value)
        {
            switch (opCode)
            {
                case OpCode.ldc_i4: PushStack(); return $"ldc.i4 {value}";
                default: throw new NotImplementedException();
            }
        }

        public string Emit(OpCode opCode, int value)
        {
            if (opCode == OpCode.stloc)
            {
                switch (value)
                {
                    case 0: PopStack(); return "stloc.0";
                    case 1: PopStack(); return "stloc.1";
                    case 2: PopStack(); return "stloc.2";
                    case 3: PopStack(); return "stloc.3";
                    default: PopStack(); return $"stloc {value}";
                }
            }
            else throw new NotImplementedException();
        }
    }
}