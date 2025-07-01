using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
        }

        // ID *does not* carry information about order
        private readonly Dictionary<uint, ScopeInfo> scopeInfoByName = [];

        public SymbolTable()
        {
            // Add the global scope
            AddScope(scopeID: 0, "Global", isLocal: false, parentScopeID: 0);
        }

        public void AddScope(uint scopeID, string name, bool isLocal, uint parentScopeID)
        {
            if (scopeInfoByName.ContainsKey(scopeID)) throw new ArgumentException($"Scope with ID {scopeID} already exists!");

            ScopeInfo? parent = null;

            if (scopeID != 0 && !scopeInfoByName.TryGetValue(parentScopeID, out parent))
                throw new ArgumentException($"Parent scope with ID {parentScopeID} does not exist.");

            scopeInfoByName[scopeID] = new ScopeInfo(scopeID, name, isLocal, parent);
        }

        public void AddSymbol(uint scopeID, int symbolPosition, string name, string type)
        {
            if (!scopeInfoByName.TryGetValue(scopeID, out var scopeInfo))
                throw new ArgumentException($"Scope with ID {scopeID} does not exist.");

            if (scopeInfo.SymbolInfoByName.ContainsKey(name))
                throw new ArgumentException($"Symbol '{name}' already exists in scope {scopeID}.");

            scopeInfo.SymbolInfoByName[name] = new SymbolInfo(scopeInfo, symbolPosition, name, type);
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
            }
            return result.ToString();
        }
    }
}