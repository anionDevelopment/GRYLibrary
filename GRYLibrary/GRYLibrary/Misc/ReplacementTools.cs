using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GRYLibrary.Core.Misc
{
    public static class ReplacementTools
    {
        public static string ReplaceVariables(string input, IDictionary<string, string> replacements, IDictionary<string, bool> booleanReplacements, IDictionary<string, Func<string>> variables)
        {
            string oldVersion = input;
            string newVersion = ReplaceVariablesOneTime(input, replacements, booleanReplacements, variables);
            while (oldVersion != newVersion)
            {
                oldVersion = newVersion;
                newVersion = ReplaceVariablesOneTime(newVersion, replacements, booleanReplacements, variables);
            }
            return newVersion;
        }

        private static string ReplaceStringVariable(string input, string replaceSource, string replaceTarget)
        {
            return input.Replace($"__[{replaceSource}]__", replaceTarget);
        }

        private static string ReplaceVariablesOneTime(string input, IDictionary<string, string> replacements, IDictionary<string, bool> booleanReplacements, IDictionary<string, Func<string>> variables)
        {
            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                input = ReplaceStringVariable(input, replacement.Key, replacement.Value);
            }
            foreach (KeyValuePair<string, bool> booleanReplacement in booleanReplacements)
            {
                input = ReplaceBooleanVariable(input, booleanReplacement.Key, booleanReplacement.Value);
                input = ReplaceBooleanVariable(input, $"!{booleanReplacement.Key}", !booleanReplacement.Value);
            }
            foreach (KeyValuePair<string, Func<string>> variable in variables)
            {
                input = ReplaceFuncVariable(input, variable.Key, variable.Value);
            }
            return input;
        }

        private static string ReplaceFuncVariable(string input, string key, Func<string> value)
        {
            string str = $"__[{key}]__";
            if (input.Contains(str))
            {
                input = input.Replace(str, value());
            }
            return input;
        }

        internal static string ReplaceBooleanVariable(string input, string variableName, bool enabled)
        {
            string result = input;
            if (enabled)
            {
                result = Regex.Replace(result, @$"__\[{variableName}\]__", string.Empty);
                result = Regex.Replace(result, @$"__\[\/{variableName}\]__", string.Empty);
            }
            else
            {
                result = Regex.Replace(result, @$"__\[{variableName}\]__(.|\n)*?__\[\/{variableName}\]__", string.Empty);
            }
            return result;
        }
    }
}