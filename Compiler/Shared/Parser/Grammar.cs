using System.Text.RegularExpressions;

namespace Compiler.Parser
{
    public class GrammarParseException(string message, int lineNumber) : Exception($"Grammar parse error on line {lineNumber}: {message}")
    {
        public int LineNumber { get; } = lineNumber;
    }

    public class GrammarRule(string name)
    {
        public string Name { get; set; } = name;
        public List<List<string>> Alternatives { get; set; } = [];
    }

    public class Grammar
    {
        public string EntryPoint { get; set; } = "Program";
        public Dictionary<string, GrammarRule> Rules { get; set; } = [];
    }

    public static partial class GrammarLoader
    {
        [GeneratedRegex(@"""[^""]*""|REGEX [^\s]+|[\w]+|[=;(){}',|]")]
        private static partial Regex RHSRegex();

        [GeneratedRegex(@"^([a-zA-Z_][a-zA-Z0-9_]*)\s*:")]
        private static partial Regex RuleRegex();

        private static bool LineHasUnquotedSemicolon(string line)
        {
            bool inQuote = false;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuote = !inQuote;
                }
                else if (line[i] == ';' && !inQuote)
                {
                    return true;
                }
            }
            return false;
        }

        private static int IndexOfUnquotedSemicolon(string line)
        {
            bool inQuote = false;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuote = !inQuote;
                }
                else if (line[i] == ';' && !inQuote)
                {
                    return i;
                }
            }
            return -1;
        }

        public static Grammar Load(string path)
        {
            var grammar = new Grammar();
            var lines = File.ReadAllLines(path);
            GrammarRule? currentRule = null;
            int lineNumber = 0;
            bool entryPointSet = false;
            bool ruleOpen = false;
            string? ruleName = null;
            List<string> pendingAlternatives = new List<string>();

            foreach (var rawLine in lines)
            {
                lineNumber++;
                var lineSpan = rawLine.AsSpan().Trim();
                if (lineSpan.IsEmpty)
                {
                    continue;
                }

                // Check for comments not on their own line
                int hashIdx = lineSpan.IndexOf('#');
                if (hashIdx >= 0)
                {
                    if (hashIdx != 0)
                    {
                        throw new GrammarParseException("Comments must be on their own line.", lineNumber);
                    }
                    continue;
                }

                // Parse entrypoint if present (must be the first non-empty, non-comment line)
                if (!entryPointSet && lineSpan.StartsWith("entrypoint:".AsSpan()))
                {
                    grammar.EntryPoint = lineSpan["entrypoint:".Length..].Trim().ToString();
                    entryPointSet = true;
                    continue;
                }

                // Semicolon on its own line: end of rule
                if (lineSpan.SequenceEqual(";".AsSpan()))
                {
                    if (!ruleOpen || currentRule == null)
                    {
                        throw new GrammarParseException("Semicolon found but no rule is open.", lineNumber);
                    }
                    if (pendingAlternatives.Count > 0)
                    {
                        foreach (var alt in pendingAlternatives)
                        {
                            var trimmed = alt.AsSpan().Trim();
                            if (!trimmed.IsEmpty)
                            {
                                currentRule.Alternatives.Add(ParseAlternative(trimmed));
                            }
                        }
                        pendingAlternatives.Clear();
                    }
                    ruleOpen = false;
                    currentRule = null;
                    ruleName = null;
                    continue;
                }

                // New rule
                var match = RuleRegex().Match(lineSpan.ToString());
                if (match.Success)
                {
                    ruleName = match.Groups[1].Value;
                    if (grammar.Rules.ContainsKey(ruleName))
                    {
                        throw new GrammarParseException($"Duplicate rule name: {ruleName}", lineNumber);
                    }
                    currentRule = new GrammarRule(ruleName);
                    grammar.Rules[ruleName] = currentRule;
                    ruleOpen = true;
                    pendingAlternatives.Clear();
                    var rest = lineSpan[match.Length..].Trim();
                    if (!rest.IsEmpty)
                    {
                        // Check for unquoted semicolon in the rest of the rule
                        int semiIdx = IndexOfUnquotedSemicolon(rest.ToString());
                        if (semiIdx >= 0)
                        {
                            string beforeSemi = rest[..semiIdx].ToString();
                            string afterSemi = rest[(semiIdx + 1)..].ToString();
                            if (!string.IsNullOrWhiteSpace(beforeSemi))
                            {
                                pendingAlternatives.Add(beforeSemi);
                            }
                            if (pendingAlternatives.Count > 0)
                            {
                                foreach (var alt in pendingAlternatives)
                                {
                                    var trimmed = alt.AsSpan().Trim();
                                    if (!trimmed.IsEmpty)
                                    {
                                        currentRule.Alternatives.Add(ParseAlternative(trimmed));
                                    }
                                }
                                pendingAlternatives.Clear();
                            }
                            ruleOpen = false;
                            currentRule = null;
                            ruleName = null;
                            if (!string.IsNullOrWhiteSpace(afterSemi))
                            {
                                throw new GrammarParseException("Unexpected content after rule-terminating semicolon.", lineNumber);
                            }
                            continue;
                        }
                        else
                        {
                            pendingAlternatives.Add(rest.ToString());
                        }
                    }
                    continue;
                }

                // Rule alternatives (while rule is open)
                if (ruleOpen && currentRule != null)
                {
                    string altLine = lineSpan.ToString();
                    // Check for unquoted semicolon in the line
                    int semiIdx = IndexOfUnquotedSemicolon(altLine);
                    if (semiIdx >= 0)
                    {
                        string beforeSemi = altLine.Substring(0, semiIdx);
                        string afterSemi = altLine[(semiIdx + 1)..];
                        if (!string.IsNullOrWhiteSpace(beforeSemi))
                        {
                            pendingAlternatives.Add(beforeSemi);
                        }
                        if (pendingAlternatives.Count > 0)
                        {
                            foreach (var alt in pendingAlternatives)
                            {
                                var trimmed = alt.AsSpan().Trim();
                                if (!trimmed.IsEmpty)
                                {
                                    currentRule.Alternatives.Add(ParseAlternative(trimmed));
                                }
                            }
                            pendingAlternatives.Clear();
                        }
                        ruleOpen = false;
                        currentRule = null;
                        ruleName = null;
                        if (!string.IsNullOrWhiteSpace(afterSemi))
                        {
                            throw new GrammarParseException("Unexpected content after rule-terminating semicolon.", lineNumber);
                        }
                        continue;
                    }
                    // No unquoted semicolon, just add as alternative
                    pendingAlternatives.Add(altLine);
                    continue;
                }

                throw new GrammarParseException("Line not recognized as part of any rule or alternative.", lineNumber);
            }
            // If file ends with rule still open, error
            if (ruleOpen)
            {
                throw new GrammarParseException($"Rule '{ruleName}' not terminated with semicolon.", lineNumber);
            }
            return grammar;
        }
        private static List<string> ParseAlternative(ReadOnlySpan<char> alt)
        {
            // Split by whitespace, but keep quoted strings and REGEX as single tokens
            var tokens = new List<string>();
            var regex = RHSRegex();
            foreach (Match m in regex.Matches(alt.ToString()))
            {
                tokens.Add(m.Value);
            }
            return tokens;
        }
    }
}
