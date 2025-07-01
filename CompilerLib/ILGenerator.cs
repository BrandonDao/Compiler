namespace CompilerLib
{
    public static class ILGenerator
    {
        public static Dictionary<string, string> PrimitiveNameMap { get; } = new()
        {
            ["int32"] = "int32"
        };

        public enum OpCode
        {
            ldc_i4 = 0x20,
            // stloc_0 = 0x0A,
            // stloc_1 = 0x0B,
            // stloc_2 = 0x0C,
            // stloc_3 = 0x0D,
            stloc = 0xFE0E,
        }

        public static string Emit(OpCode opCode, string value)
            => opCode switch
            {
                OpCode.ldc_i4 => $"ldc.i4 {value}",
                _ => throw new NotImplementedException(),
            };
        public static string Emit(OpCode opCode, int value)
            => opCode switch
            {
                OpCode.stloc => value switch
                {
                    0 => "stloc.0",
                    1 => "stloc.1",
                    2 => "stloc.2",
                    3 => "stloc.3",
                    _ => $"stloc {value}",
                },
                _ => throw new NotImplementedException(),
            };
    }
}