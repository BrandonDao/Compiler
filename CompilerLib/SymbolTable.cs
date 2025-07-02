using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CompilerLib.Parser.Nodes;

namespace CompilerLib
{
    public class SymbolTable
    {
        [DebuggerDisplay("[{EnclosingScope.ID}.{SymbolPosition}] {Name,nq}:{Type,nq}")]
        public record class SymbolInfo(ScopeInfo EnclosingScope, int SymbolPosition, string Name, string Type);

        [DebuggerDisplay("{(IsLocal ? \"LOCAL\" : string.Empty)} [{ID}] {Name,nq} (Parent: {(Parent != null ? $\"[{Parent.ID}] {Parent.Name}\" : \"None\"),nq})")]
        public class ScopeInfo(uint id, string name, bool isLocal, ScopeInfo? parent)
        {
            public uint ID { get; } = id;
            public string Name { get; } = name;
            public bool IsLocal { get; } = isLocal;
            public ScopeInfo? Parent { get; } = parent;
            public Dictionary<string, SymbolInfo> SymbolInfoByName { get; set; } = [];
            public Dictionary<string, FunctionInfo> FunctionInfoByName { get; set; } = [];
        }
        public class FunctionInfo(SymbolInfo symbolInfo, List<SymbolInfo> parameterInfos, ScopeInfo childScopeInfo)
        {
            public SymbolInfo SymbolInfo { get; } = symbolInfo;
            public ScopeInfo ChildScopeInfo { get; } = childScopeInfo;
            public List<SymbolInfo> ParameterInfos { get; } = parameterInfos;
        }

        // ID *does not* carry information about order
        private readonly Dictionary<uint, ScopeInfo> scopeInfoByName = [];

        public SymbolTable()
        {
            // Add the global scope
            AddScope(scopeID: 0, "Global", isLocal: false, parentScopeID: 0);
        }

        public ScopeInfo AddScope(uint scopeID, string name, bool isLocal, uint parentScopeID)
        {
            if (scopeInfoByName.ContainsKey(scopeID)) throw new ArgumentException($"Scope with ID {scopeID} already exists!");

            ScopeInfo? parent = null;

            if (scopeID != 0 && !scopeInfoByName.TryGetValue(parentScopeID, out parent))
                throw new ArgumentException($"Parent scope with ID {parentScopeID} does not exist.");

            ScopeInfo scopeInfo = new(scopeID, name, isLocal, parent);
            scopeInfoByName[scopeID] = scopeInfo;
            return scopeInfo;
        }

        public FunctionInfo AddFunction(uint childScopeID, uint parentScopeID, string name, string returnType, List<SyntaxNode> parameters)
        {
            if (!scopeInfoByName.TryGetValue(parentScopeID, out var parentScopeInfo))
                throw new ArgumentException($"Scope with ID {parentScopeID} does not exist.");

            if (parentScopeInfo.FunctionInfoByName.ContainsKey(name))
                throw new ArgumentException($"Function with ID {name} already exists!");

            if (scopeInfoByName.ContainsKey(childScopeID))
                throw new ArgumentException($"Scope with ID already exists!");

            ScopeInfo childScopeInfo = AddScope(childScopeID, name, isLocal: true, parentScopeID);
            SymbolInfo funcSymbolInfo = new(parentScopeInfo, SymbolPosition: -1, name, returnType);

            List<SymbolInfo> parameterInfos = new(capacity: parameters.Count);
            foreach (var potentialParam in parameters)
            {
                if (potentialParam is not VariableNameTypeNode param)
                    throw new InvalidOperationException("AST Conversion failed! Expected VariableNameTypeNode for function parameter, not " + potentialParam.GetType().Name);

                parameterInfos.Add(AddSymbol(childScopeID, symbolPosition: -1, param.Name, param.Type));
            }

            FunctionInfo functionInfo = new(funcSymbolInfo, parameterInfos, childScopeInfo);
            parentScopeInfo.FunctionInfoByName.Add(name, functionInfo);
            return functionInfo;
        }

        public SymbolInfo AddSymbol(uint scopeID, int symbolPosition, string name, string type)
        {
            if (!scopeInfoByName.TryGetValue(scopeID, out var scopeInfo))
                throw new ArgumentException($"Scope with ID {scopeID} does not exist.");

            if (scopeInfo.SymbolInfoByName.ContainsKey(name))
                throw new ArgumentException($"Symbol '{name}' already exists in scope {scopeID}.");

            SymbolInfo symbolInfo = new(scopeInfo, symbolPosition, name, type);
            scopeInfo.SymbolInfoByName[name] = symbolInfo;
            return symbolInfo;
        }

        public bool ContainsFunction(uint scopeID, string name)
            => TryGetFunctionInfo(scopeID, name, out _);
        public bool TryGetFunctionInfo(uint scopeID, string name, [NotNullWhen(true)] out FunctionInfo? info)
        {
            info = null;
            if (!scopeInfoByName.TryGetValue(scopeID, out var scopeInfo)) return false;

            foreach (var scope in GetScopeHierarchy(scopeInfo))
            {
                if (scope.FunctionInfoByName.TryGetValue(name, out info)) return true;
            }
            return false;
        }

        public bool ContainsSymbol(uint scopeID, string name)
            => TryGetSymbolInfo(scopeID, name, out _);
        public bool TryGetSymbolInfo(uint scopeID, string name, [NotNullWhen(true)] out SymbolInfo? info)
        {
            info = null;
            if (!scopeInfoByName.TryGetValue(scopeID, out var scopeInfo)) return false;

            foreach (var scope in GetScopeHierarchy(scopeInfo))
            {
                if (scope.SymbolInfoByName.TryGetValue(name, out info)) return true;
            }
            return false;
        }

        private static IEnumerable<ScopeInfo> GetScopeHierarchy(ScopeInfo scopeInfo)
        {
            ScopeInfo? curr = scopeInfo;
            while (curr != null)
            {
                yield return curr;
                curr = curr.Parent;
            }
        }

        public string GetPrintable()
        {
            var result = new System.Text.StringBuilder();
            foreach (var scope in scopeInfoByName.Values)
            {
                result.AppendLine($"{(scope.IsLocal ? "Local" : string.Empty)} Scope [{scope.ID}] {scope.Name} (Parent: {(scope.Parent != null ? $"[{scope.Parent.ID}] {scope.Parent.Name}" : "None")})");
                foreach (var symbol in scope.SymbolInfoByName.Values)
                {
                    result.AppendLine($"\tSymbol [{symbol.EnclosingScope.ID}.{symbol.SymbolPosition}] {symbol.Name}:{symbol.Type}");
                }
                foreach (var function in scope.FunctionInfoByName.Values)
                {
                    result.AppendLine($"\tFunction [{function.SymbolInfo.EnclosingScope.ID}.{function.SymbolInfo.SymbolPosition}] {function.SymbolInfo.Name}:{function.SymbolInfo.Type}");
                    result.AppendLine($"\t\tChild Scope: [{function.ChildScopeInfo.ID}] {function.ChildScopeInfo.Name}");
                    foreach (var param in function.ParameterInfos)
                    {
                        result.AppendLine($"\t\tParameter: {param.Name}:{param.Type}");
                    }
                }
            }
            return result.ToString();
        }
    }
}