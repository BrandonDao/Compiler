using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Compiler.Parser
{
    internal class SymbolTable
    {
        [DebuggerDisplay("[{EnclosingScopeID}.{SymbolID}] {Name,nq}:{Type,nq}")]
        public record class SymbolInfo(uint EnclosingScopeID, uint SymbolID, string Name, string Type); // ID *does* carry information about order

        [DebuggerDisplay("[{ID}] {Name,nq} (Parent: {(Parent != null ? $\"[{Parent.ID}] {Parent.Name}\" : \"None\"),nq})")]
        private class ScopeInfo(uint id, string name, ScopeInfo? parent)
        {
            public uint ID { get; } = id;
            public string Name { get; } = name;
            public ScopeInfo? Parent { get; } = parent;
            public Dictionary<string, SymbolInfo> SymbolInfoByName { get; set; } = [];
        }

        // ID *does not* carry information about order
        private readonly Dictionary<uint, ScopeInfo> scopeInfoByName = [];

        public SymbolTable()
        {
            // Add the global scope
            AddScope(scopeID: 0, "Global", parentScopeID: 0);
        }

        public void AddScope(uint scopeID, string name, uint parentScopeID)
        {
            if (scopeInfoByName.ContainsKey(scopeID)) throw new ArgumentException($"Scope with ID {scopeID} already exists!");

            ScopeInfo? parent = null;

            if (scopeID != 0 && !scopeInfoByName.TryGetValue(parentScopeID, out parent))
                throw new ArgumentException($"Parent scope with ID {parentScopeID} does not exist.");

            scopeInfoByName[scopeID] = new ScopeInfo(scopeID, name, parent);
        }
        public void AddSymbol(uint scopeID, int symbolID, string name, string type)
        {
            if (!scopeInfoByName.TryGetValue(scopeID, out var scopeInfo))
                throw new ArgumentException($"Scope with ID {scopeID} does not exist.");

            if (scopeInfo.SymbolInfoByName.ContainsKey(name))
                throw new ArgumentException($"Symbol '{name}' already exists in scope {scopeID}.");

            scopeInfo.SymbolInfoByName[name] = new SymbolInfo(scopeID, (uint)symbolID, name, type);
        }

        public bool ContainsSymbol(uint scopeID, string name)
            => TryGetSymbolInfo(scopeID, name, out _);
        public bool TryGetSymbolInfo(uint scopeID, string name, [NotNullWhen(true)] out SymbolInfo? info)
        {
            info = null;
            if (!scopeInfoByName.TryGetValue(scopeID, out var scopeInfo))  return false;

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
                result.AppendLine($"Scope [{scope.ID}] {scope.Name} (Parent: {(scope.Parent != null ? $"[{scope.Parent.ID}] {scope.Parent.Name}" : "None")})");
                foreach (var symbol in scope.SymbolInfoByName.Values)
                {
                    result.AppendLine($"\tSymbol [{symbol.EnclosingScopeID}.{symbol.SymbolID}] {symbol.Name}:{symbol.Type}");
                }
            }
            return result.ToString();
        }
    }
}