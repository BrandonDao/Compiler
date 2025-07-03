namespace CompilerLib.Parser.Nodes.Types
{
    public static class LanguageNames
    {
        public static class Primitives
        {
            public const string Int8 = "int8";
            public const string Int16 = "int16";
            public const string Int32 = "int32";
            public const string Int64 = "int64";
            public const string Bool = "bool";
        }
        public static class Keywords
        {
            public const string Void = "void";
            public const string Let = "let";
            public const string While = "while";
            public const string Func = "func";
            public const string Namespace = "namespace";
        }
        public static class Literals
        {
            public const string True = "true";
            public const string False = "false";
        }
    }
}