using System.Collections.Generic;
using System.Text;

namespace GRYLibrary.Core.ExecutePrograms.Detached
{
    /// <summary>
    /// Splits an argument-string into single arguments using the same quoting-rules that .NET applies to
    /// <see cref="System.Diagnostics.ProcessStartInfo.Arguments"/>. This keeps the argument-handling of a detached
    /// execution consistent with the owned execution, which relies on those rules.
    /// </summary>
    internal static class CommandLineArgumentParser
    {
        /// <summary>
        /// Parses <paramref name="argument"/> into its single arguments.
        /// </summary>
        internal static IList<string> Parse(string argument)
        {
            List<string> result = new();
            if (argument == null)
            {
                return result;
            }
            int index = 0;
            while (index < argument.Length)
            {
                while (index < argument.Length && (argument[index] == ' ' || argument[index] == '\t'))
                {
                    index++;
                }
                if (index == argument.Length)
                {
                    break;
                }
                StringBuilder currentArgument = new();
                bool inQuotes = false;
                while (index < argument.Length)
                {
                    char currentCharacter = argument[index];
                    if (currentCharacter == '\\')
                    {
                        int numberOfBackslashes = 0;
                        while (index < argument.Length && argument[index] == '\\')
                        {
                            numberOfBackslashes++;
                            index++;
                        }
                        if (index < argument.Length && argument[index] == '"')
                        {
                            currentArgument.Append('\\', numberOfBackslashes / 2);
                            if (numberOfBackslashes % 2 == 0)
                            {
                                inQuotes = !inQuotes;
                            }
                            else
                            {
                                currentArgument.Append('"');
                            }
                            index++;
                        }
                        else
                        {
                            currentArgument.Append('\\', numberOfBackslashes);
                        }
                        continue;
                    }
                    if (currentCharacter == '"')
                    {
                        inQuotes = !inQuotes;
                        index++;
                        continue;
                    }
                    if (!inQuotes && (currentCharacter == ' ' || currentCharacter == '\t'))
                    {
                        index++;
                        break;
                    }
                    currentArgument.Append(currentCharacter);
                    index++;
                }
                result.Add(currentArgument.ToString());
            }
            return result;
        }
    }
}
