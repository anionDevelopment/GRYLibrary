using GRYLibrary.Core.AOA;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.OperatingSystem;
using GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems;
using GRYLibrary.Core.XMLSerializer;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Win32;
using NJsonSchema.Validation;
using Sprache;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Xsl;
using static GRYLibrary.Core.Misc.TableGenerator;

namespace GRYLibrary.Core.Misc
{
    public static partial class Utilities
    {
        #region Constants
        public const string Line = "--------";
        public const string LongLine = Line + Line;
        public const string EmptyString = "";
        public const string SpecialCharacterTestString = "<SpecialCharacterTest>äöüßÄÖÜÆÑçéý html-test:<span>span-tags with angle brackets should be visible</span> &← /\\*#^°'`´\" ?|§@$€%-_²⁶₇¬∀∈∑∜∫∰≈≪ﬁ.Доброе утро صبح به خیر शुभ प्रभात 좋은 아침 സുപ്രഭാതം おはようございます ហ្គុនមូហ្កិន</SpecialCharacterTest>";

        [GeneratedRegex(@"^[0-9a-f]+$")]
        private static partial Regex OneOrMoreHexSigns();
        #endregion

        public static (T[], T[]) Split<T>(T[] source, int index)
        {
            int len2 = source.Length - index;
            T[] first = new T[index];
            T[] last = new T[len2];
            Array.Copy(source, 0, first, 0, index);
            Array.Copy(source, index, last, 0, len2);
            return (first, last);
        }
        public static bool IsAdministrator()
        {
            return OperatingSystem.OperatingSystem.GetCurrentOperatingSystem().Accept(new IsAdministratorVisitor());
        }

        private class IsAdministratorVisitor : IOperatingSystemVisitor<bool>
        {
            public bool Handle(OSX operatingSystem)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            public bool Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            public bool Handle(Linux operatingSystem)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        public static PercentValue ToPercentValue(this float value)
        {
            return new PercentValue(value);
        }

        public static PercentValue ToPercentValue(this int value)
        {
            return new PercentValue((decimal)value);
        }

        public static PercentValue ToPercentValue(this double value)
        {
            return new PercentValue(value);
        }

        public static PercentValue ToPercentValue(this decimal value)
        {
            return new PercentValue(value);
        }

        public static void Repeat<T>(this Action<uint> action, uint amountOfExecutions)
        {
            for (uint i = 0; i < amountOfExecutions; i++)
            {
                action(i);
            }
        }

        public static string NormalizePath(string path)
        {
            return NormalizePath(path, OperatingSystem.OperatingSystem.GetCurrentOperatingSystem());
        }

        /// <summary>
        /// Normalizes the path-separators in <paramref name="path"/> for the explicitly given <paramref name="operatingSystem"/> instead of the current one.
        /// Because the target-operating-system is passed in (and not derived from the host), the separator-conversion is deterministic and identical on
        /// every host, which makes it testable across platforms.
        /// </summary>
        public static string NormalizePath(string path, OperatingSystem.OperatingSystem operatingSystem)
        {
            return operatingSystem.Accept(new NormalizePathVisitor(path));
        }

        private class NormalizePathVisitor : IOperatingSystemVisitor<string>
        {
            // These must be literal characters and not Path.DirectorySeparatorChar/Path.AltDirectorySeparatorChar: those are platform-dependent and on
            // Linux/macOS BOTH evaluate to '/', which would turn the separator-replacement below into a no-op (so backslashes in a path would survive).
            public readonly char WindowsPathSeparatorChar = '\\';
            public readonly char LinuxAndOSXPathSeparatorChar = '/';
            private readonly string _Path;

            public NormalizePathVisitor(string path)
            {
                this._Path = path;
            }

            public string Handle(OSX operatingSystem)
            {
                return this._Path.Replace(this.WindowsPathSeparatorChar, this.LinuxAndOSXPathSeparatorChar);
            }

            public string Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                return this._Path.Replace(this.LinuxAndOSXPathSeparatorChar, this.WindowsPathSeparatorChar);
            }

            public string Handle(Linux operatingSystem)
            {
                return this._Path.Replace(this.WindowsPathSeparatorChar, this.LinuxAndOSXPathSeparatorChar);
            }
        }

        /// <summary>
        /// Checks if the given <paramref name="subList"/> is contained in <paramref name="list"/>.
        /// </summary>
        /// <remarks>
        /// For performance-reasons this function will be reduced to a string-representation comparison.
        /// For this reason it is required to specify a <paramref name="serializableFunction"/> thich returns a string-representation for a list-item.
        /// It is also required to pass a <paramref name="separator"/> which will never occurr in any string-representation of any list-item.
        /// </remarks>
        /// <returns>
        /// Returns true if and only if the given <paramref name="subList"/> is contained in <paramref name="list"/> in the correct order.
        /// </returns>
        public static bool ContainsSublist<T>(this IList<T> list, IList<T> subList, Func<T, string> serializableFunction, string separator = "-")
        {
            if (list == null || subList == null)
            {
                throw new ArgumentException($"Parameter {nameof(list)} and {nameof(subList)} may not be null");
            }
            if (subList.Count > list.Count)
            {
                return false;
            }
            if (subList.Count == 0)
            {
                return true;
            }
            string listAsString = string.Join(separator, list.Select(item => serializableFunction(item)));
            string subListAsString = string.Join(separator, subList.Select(item => serializableFunction(item)));
            return listAsString.Contains(subListAsString);
        }
        public static IList<T> NTimes<T>(this IEnumerable<T> input, uint n)
        {
            List<T> result = [];
            int i = 0;
            while (i < n)
            {
                i += 1;
                result.AddRange(input);
            }
            return result;
        }
        public static uint SwapEndianness(uint value)
        {
            return ((value & 0x000000ff) << 24)
                 + ((value & 0x0000ff00) << 08)
                 + ((value & 0x00ff0000) >> 08)
                 + ((value & 0xff000000) >> 24);
        }

        public static byte[] GetRandomByteArray(long length = 65536)
        {
            byte[] result = new byte[length];
            new Random().NextBytes(result);
            return result;
        }

        /// <summary>
        /// This is the inverse function of <see cref="ConcatBytesArraysWithLengthInformation"/>
        /// </summary>
        public static byte[][] GetBytesArraysFromConcatBytesArraysWithLengthInformation(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is the inverse function of <see cref="GetBytesArraysFromConcatBytesArraysWithLengthInformation"/>
        /// </summary>
        public static byte[] ConcatBytesArraysWithLengthInformation(params byte[][] byteArrays)
        {
            byte[] result = Array.Empty<byte>();
            foreach (byte[] byteArray in byteArrays)
            {
                result = Concat(result, UnsignedInteger32BitToByteArray((uint)byteArray.Length), byteArray);
            }
            return result;
        }

        public static uint[] ByteArrayToUnsignedInteger32BitArray(byte[] byteArray)
        {
            if ((byteArray.Length % 4) != 0)
            {
                throw new ArgumentException($"The argument for parameter {nameof(byteArray)} must have a length which is a multiple of 4.");
            }
            uint[] result = new uint[byteArray.Length / 4];
            for (int i = 0; i < byteArray.Length / 4; i++)
            {
                result[i] = ByteArrayToUnsignedInteger32Bit([byteArray[4 * i], byteArray[(4 * i) + 1], byteArray[(4 * i) + 2], byteArray[(4 * i) + 3]]);
            }
            return result;
        }

        public static string Format(ValidationError error)
        {
            Dictionary<string, string> values = new()
            {
                { nameof(error.Kind), error.Kind.ToString() },
                { nameof(error.Path), error.Path },
                { nameof(error.Property), error.Property }
            };
            if (error.HasLineInfo)
            {
                values.Add(nameof(error.LineNumber), error.LineNumber.ToString());
                values.Add(nameof(error.LinePosition), error.LinePosition.ToString());
            }
            return FormatKeyValuePairs(values);
        }

        public static string FormatKeyValuePairs(Dictionary<string, string> values)
        {
            return "{" + string.Join(", ", values.Select(kvp => $"'{kvp.Key}':'{kvp.Value}'")) + "}";
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = new();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
        public static bool EqualsIgnoringOrder<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            return Enumerable.SequenceEqual(list1.OrderBy(item => item), list2.OrderBy(item => item));
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWhichHaveGetterAndSetter(Type type)
        {
            List<PropertyInfo> result = [];
            foreach (PropertyInfo property in type.GetType().GetProperties())
            {
                if (property.CanWrite && property.CanRead)
                {
                    result.Add(property);
                }
            }
            return result;
        }

        public static IEnumerable<string> GetFilesOfFolderRecursively(string folder)
        {
            List<string> result = [];
            foreach (string subfolder in Directory.GetDirectories(folder))
            {
                result.AddRange(GetFilesOfFolderRecursively(subfolder));
            }
            foreach (string file in Directory.GetFiles(folder))
            {
                result.Add(file);
            }
            return result;
        }

        public static bool IsValidXML(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                return false;
            }
            try
            {
                XDocument.Parse(xmlString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int Count(this IEnumerable enumerable)
        {
            int result = 0;
            IEnumerator enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                result += 1;
            }
            return result;
        }
        /// <summary>
        /// This function does nothing.
        /// The purpose of this function is to say explicitly that nothing should be done at the point where this function is called.
        /// </summary>
        public static void NoOperation()
        {
            //nothing to do
        }
        public static void ReplaceUnderscoresInFolderTransitively(string folder, IDictionary<string, string> replacements)
        {
            void replaceInFile(string file, object obj)
            {
                string newFileWithPath = RenameFileIfRequired(file, replacements);
                ReplaceUnderscoresInFile(newFileWithPath, replacements);
            }
            void replaceInDirectory(string directory, object obj)
            {
                RenameFolderIfRequired(directory, replacements);
            }
            ForEachFileAndDirectoryTransitively(folder, replaceInDirectory, replaceInFile);
        }

        private static string RenameFileIfRequired(string file, IDictionary<string, string> replacements)
        {
            string originalFilename = Path.GetFileName(file);
            string newFilename = ReplaceUnderscores(originalFilename, replacements);
            string result = Path.Combine(Path.GetDirectoryName(file), newFilename);
            if (!newFilename.Equals(originalFilename))
            {
                File.Move(file, result);
            }
            return result;
        }

        private static string RenameFolderIfRequired(string folder, IDictionary<string, string> replacements)
        {
            string originalFoldername = new DirectoryInfo(folder).Name;
            string newFoldername = ReplaceUnderscores(originalFoldername, replacements);
            string result = Path.Combine(Path.GetDirectoryName(folder), newFoldername);
            if (!newFoldername.Equals(originalFoldername))
            {
                if (Directory.Exists(result))
                {
                    MoveContentOfFoldersAcrossVolumes(folder, result);
                    DeleteFolder(folder);
                }
                else
                {
                    Directory.Move(folder, result);
                }
            }
            return result;
        }

        public static string ReplaceUnderscores(string @string, IDictionary<string, string> replacements)
        {
            foreach (System.Collections.Generic.KeyValuePair<string, string> replacement in replacements)
            {
                @string = @string.Replace($"__{replacement.Key}__", replacement.Value);
            }
            return @string;
        }
        public static bool ObjectIsPrimitive(object @object)
        {
            return TypeIsPrimitive(@object.GetType());
        }

        public static bool TypeIsPrimitive(Type type)
        {
            if (type.IsGenericType)
            {
                return false;
            }
            else
            {
                if (type.IsPrimitive)
                {
                    return true;
                }
                if (typeof(string).Equals(type))
                {
                    return true;
                }
                if (type.IsValueType)
                {
                    return true;
                }
                if (TypeRepresentsType(type))
                {
                    return true;
                }
                return false;
            }
        }

        public static bool TypeRepresentsType(Type type)
        {
            return type.FullName is "System.Reflection.Emit.EnumBuilder"
                or "System.Reflection.Emit.GenericTypeParameterBuilder"
                or "System.Reflection.Emit.TypeBuilder"
                or "System.Reflection.TypeInfo"
                or "System.RuntimeType"
                or "System.Type";
        }

        public static bool IsAssignableFrom(object @object, Type genericTypeToCompare)
        {
            return TypeIsAssignableFrom(@object.GetType(), genericTypeToCompare);
        }

        public static bool TypeIsAssignableFrom(Type typeForCheck, Type parentType)
        {
            ISet<Type> typesToCheck = GetTypeWithParentTypesAndInterfaces(typeForCheck);
            bool result = typesToCheck.Contains(parentType, TypeComparerIgnoringGenerics);
            return result;
        }
        public static ISet<Type> GetTypeWithParentTypesAndInterfaces(Type type)
        {
            HashSet<Type> result = [type];
            result.UnionWith(type.GetInterfaces());
            if (type.BaseType != null)
            {
                result.UnionWith(GetTypeWithParentTypesAndInterfaces(type.BaseType));
            }
            return result;
        }
        public static IEqualityComparer<Type> TypeComparerIgnoringGenerics { get; } = new TypeComparerIgnoringGenericsType();
        private class TypeComparerIgnoringGenericsType : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                if (!x.Name.Equals(y.Name))
                {
                    return false;
                }
                if (!x.Namespace.Equals(y.Namespace))
                {
                    return false;
                }
                if (!x.Assembly.Equals(y.Assembly))
                {
                    return false;
                }
                return true;
            }

            public int GetHashCode(Type obj)
            {
                return obj.GetHashCode();
            }
        }
        public static void ReplaceUnderscoresInFile(string file, IDictionary<string, string> replacements)
        {
            ReplaceUnderscoresInFile(file, replacements, new UTF8Encoding(false));
        }

        public static void ReplaceUnderscoresInFile(string file, IDictionary<string, string> replacements, Encoding encoding)
        {
            string originalContent = File.ReadAllText(file, encoding);
            string replacedContent = ReplaceUnderscores(originalContent, replacements);
            if (!originalContent.Equals(replacedContent))
            {
                File.WriteAllText(file, replacedContent, encoding);
            }
        }
        public static void WriteToConsoleAsASCIITable(IList<IList<string>> columns)
        {
            string[] table = Generate(JaggedArrayToTwoDimensionalArray(EnumerableOfEnumerableToJaggedArray(columns)), new ASCIITable());
            foreach (string line in table)
            {
                Console.WriteLine(line);
            }
        }
        public static IList<IList<string>> CSVStringToEntryList(string csvString)
        {
            List<IList<string>> result = [];
            if (csvString != EmptyString)
            {
                foreach (string line in csvString.Split(Environment.NewLine))
                {
                    List<string> lineAsList = [.. line.Split(";")];
                    result.Add(lineAsList);
                }
            }
            return result;
        }
        public static string EntryListToCSVString(IList<IList<string>> entryList)
        {
            return string.Join(Environment.NewLine, entryList.Select(line => string.Join(";", line)));
        }

        public static string PadCSV(string csvString)
        {
            return EntryListToCSVString(PadTable(CSVStringToEntryList(csvString)));
        }

        public static IList<IList<string>> PadTable(IList<IList<string>> table)
        {
            List<IList<string>> result = [];
            if (table.Count == 0)
            {
                return result;
            }
            AssertCondition(table.Select(lines => lines.Count).ToHashSet().Count == 1, @break: true);
            int columnAmount = table.First().Count;
            uint[] columnWidths = [.. Enumerable.Repeat((uint)0, columnAmount)];

            //collect column-widths
            foreach (IList<string> line in table)
            {
                for (int i = 0; i < columnAmount; i++)
                {
                    if (columnWidths[i] < line[i].Length)
                    {
                        columnWidths[i] = (uint)line[i].Length;
                    }
                }
            }
            foreach (IList<string> line in table)
            {
                List<string> cells = [];
                for (int i = 0; i < columnAmount; i++)
                {
                    cells.Add(line[i].PadRight((int)columnWidths[i], ' '));
                }
                result.Add(cells);
            }
            return result;
        }

        public static T[][] EnumerableOfEnumerableToJaggedArray<T>(IEnumerable<IEnumerable<T>> items)
        {
            return [.. items.Select(Enumerable.ToArray)];
        }

        public static T[,] JaggedArrayToTwoDimensionalArray<T>(T[][] items)
        {
            int amountOfItemsInFirstDimension = items.Length;
            int amountOfItemsInSecondDimension = items.GroupBy(tArray => tArray.Length).Single().Key;
            T[,] result = new T[amountOfItemsInFirstDimension, amountOfItemsInSecondDimension];
            for (int i = 0; i < amountOfItemsInFirstDimension; ++i)
            {
                for (int j = 0; j < amountOfItemsInSecondDimension; ++j)
                {
                    result[i, j] = items[i][j];
                }
            }

            return result;
        }

        /// <returns>
        /// Returns a new <see cref="Guid"/> whose value in the last block is incremented
        /// </returns>
        public static Guid IncrementGuid(Guid id, long valueToIncrement = 1)
        {
            return IncrementGuid(id, new BigInteger(valueToIncrement));
        }

        public static Guid IncrementGuid(Guid id, BigInteger valueToIncrement)
        {
            BigInteger value = BigInteger.Parse(id.ToString("N"), NumberStyles.HexNumber);
            return Guid.Parse((value + valueToIncrement).ToString("X").PadLeft(32, '0'));
        }

        public static IEnumerable<IEnumerable<T>> JaggedArrayToEnumerableOfEnumerable<T>(T[][] items)
        {
            List<List<T>> result = [];
            foreach (T[] innerArray in items)
            {
                List<T> innerList = [.. innerArray];
                result.Add(innerList);
            }
            return result;
        }
        public static T[][] TwoDimensionalArrayToJaggedArray<T>(T[,] items)
        {
            int rowsFirstIndex = items.GetLowerBound(0);
            int rowsLastIndex = items.GetUpperBound(0);
            int numberOfRows = rowsLastIndex + 1;
            int columnsFirstIndex = items.GetLowerBound(1);
            int columnsLastIndex = items.GetUpperBound(1);
            int numberOfColumns = columnsLastIndex + 1;
            T[][] result = new T[numberOfRows][];
            for (int i = rowsFirstIndex; i <= rowsLastIndex; i++)
            {
                result[i] = new T[numberOfColumns];
                for (int j = columnsFirstIndex; j <= columnsLastIndex; j++)
                {
                    result[i][j] = items[i, j];
                }
            }
            return result;
        }
        public static void EnsureFileExists(string path, bool createDirectoryIfRequired = false)
        {
            path = ResolveToFullPath(path);
            string directory = Path.GetDirectoryName(path);
            if (createDirectoryIfRequired && !string.IsNullOrWhiteSpace(directory))
            {
                EnsureDirectoryExists(directory);
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
        }
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static void EnsureDirectoryExistsAndIfEmpty(string path)
        {
            if (Directory.Exists(path))
            {
                DeleteContentOfFolder(path);
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string DurationToUserFriendlyString(TimeSpan timespan, uint desiredMilliSecondsDigitsCount = 0)
        {
            string result = $"{Math.Floor(timespan.TotalHours).ToString().PadLeft(2, '0')}:{timespan.Minutes.ToString().PadLeft(2, '0')}:{timespan.Seconds.ToString().PadLeft(2, '0')}";
            if (desiredMilliSecondsDigitsCount > 0)
            {
                string milliSecondsAsString = timespan.Milliseconds.ToString();
                uint milliSecondsDigits = (uint)milliSecondsAsString.Length;

                string milliSecondsWithCorrectLength;
                if (desiredMilliSecondsDigitsCount < milliSecondsDigits)
                {
                    milliSecondsWithCorrectLength = milliSecondsAsString[..(int)desiredMilliSecondsDigitsCount];
                }
                else if (milliSecondsDigits < desiredMilliSecondsDigitsCount)
                {
                    milliSecondsWithCorrectLength = milliSecondsAsString.PadRight((int)desiredMilliSecondsDigitsCount, '0');
                }
                else
                {
                    milliSecondsWithCorrectLength = milliSecondsAsString;
                }
                result = $"{result}.{milliSecondsWithCorrectLength}";
            }
            return result;
        }

        public const string FormatForDateTimesInFullFormatISO8601 = "yyyy-MM-ddTHH:mm:sszzz";
        public const string FormatForDateTimesInFullFormatSimple = "yyyy-MM-dd HH:mm:ss";
        public const string FormatForDateTimesInFullFormatWithTimeZone = "yyyy-MM-dd HH:mm:ss zzz";
        public static DateTimeOffset? ParseTimestampNullable(string? timestamp, bool addMillisecondsInLogTimestamps)
        {
            if (timestamp == null)
            {
                return null;
            }
            else
            {
                return ParseTimestamp(timestamp, addMillisecondsInLogTimestamps);
            }
        }
        public static string? FormatTimestampNullable(DateTimeOffset? timestamp, bool addMillisecondsInLogTimestamps)
        {
            if (timestamp == null)
            {
                return null;
            }
            else
            {
                return FormatTimestamp(timestamp.Value, addMillisecondsInLogTimestamps);
            }
        }
        public static DateTimeOffset ParseTimestamp(string timestamp, bool addMillisecondsInLogTimestamps)
        {
            if (addMillisecondsInLogTimestamps)
            {
                return DateTimeOffset.Parse(timestamp);//2023-05-01T11:44:53.4931284+02:00
            }
            else
            {
                return DateTimeOffset.Parse(timestamp);//2023-05-01T11:44:53+02:00
            }
        }
        public static string FormatTimestamp(DateTimeOffset timestamp, bool addMillisecondsInLogTimestamps)
        {
            if (addMillisecondsInLogTimestamps)
            {
                return timestamp.ToString("o");//2023-05-01T11:44:53.4931284+02:00
            }
            else
            {
                return timestamp.ToString(FormatForDateTimesInFullFormatISO8601);//2023-05-01T11:44:53+02:00
            }
        }

        public static string DateTimeToISO8601String(DateTimeOffset dateTimeOffset, bool addMilliseconds = true, bool addTimeZone = true)
        {
            string format;
            if (addTimeZone)
            {
                if (addMilliseconds)
                {
                    format = "yyyy-MM-dd'T'HH:mm:ss,fffzzz";
                }
                else
                {
                    format = "yyyy-MM-dd'T'HH:mm:sszzz";
                }
            }
            else
            {
                if (addMilliseconds)
                {
                    format = "yyyy-MM-dd'T'HH:mm:ss,fff";
                }
                else
                {
                    format = "yyyy-MM-dd'T'HH:mm:ss";
                }
            }
            return dateTimeOffset.ToString(format, CultureInfo.InvariantCulture);
        }
        public static string DateTimeToUserFriendlyString(DateTimeOffset dateTimeOffset)
        {
            return DateTimeToISO8601String(dateTimeOffset, false);
        }

        public static string DateTimeToUserFriendlyString(GRYDateTime dateTime)
        {
            return DateTimeToISO8601String(dateTime.ToDateTime(), false);
        }

        public static string DateTimeForFilename(DateTimeOffset dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        }

        public static string DateToUserFriendlyString(DateOnly date)
        {
            return $"{date.Year.ToString().PadLeft(4, '0')}-{date.Month.ToString().PadLeft(2, '0')}-{date.Day.ToString().PadLeft(2, '0')}";
        }

        public static string TimeToUserFriendlyString(TimeOnly time)
        {
            return $"{time.Hour.ToString().PadLeft(2, '0')}:{time.Minute.ToString().PadLeft(2, '0')}:{time.Second.ToString().PadLeft(2, '0')}";
        }

        /// <summary>
        /// This function parses a datetime-string.
        /// </summary>
        /// <param name="input">This string is expected to be in the format "MM/dd/yyyy hh:mm:ss tt".</param>
        /// <remarks>
        /// The difference in comparison to <see cref="DateTime.ParseExact(string, string, IFormatProvider?)"/> is that this function does not require leading zeros.
        /// So "5" is allowed as hour for example.
        /// </remarks>
        /// <example>
        /// "4/3/2017 7:4:53 PM" is a valid string (representing the date "2017-03-04T19:4:53" in ISO8601-format).
        /// </example>
        public static DateTime ParseDateAmericanFormat(string input)
        {
            string regexStr = @"^(\d?\d)\/(\d?\d)\/(\d?\d?\d?\d) (\d?\d):(\d?\d):(\d?\d) (AM|PM)$";
            Regex regex = new Regex(regexStr);
            Match match = regex.Match(input);
            if (match.Captures.Count < 1)
            {
                throw new ArgumentException($"Input \"{input}\" does not match regex \"{regexStr}\".");
            }
            else if (match.Captures.Count == 1)
            {
                static string Pad(string value, int length)
                {
                    return value.PadLeft(length, '0');
                }
                Match c = (Match)match.Captures[0];
                string s = $"{Pad(c.Groups[1].Value, 2)}/{Pad(c.Groups[2].Value, 2)}/{Pad(c.Groups[3].Value, 4)} {Pad(c.Groups[4].Value, 2)}:{Pad(c.Groups[5].Value, 2)}:{Pad(c.Groups[6].Value, 2)} {c.Groups[7].Value}";
                return DateTime.ParseExact(s, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            }
            else
            {
                throw new InternalAlgorithmException($"Error while parsing date \"{input}\".");
            }
        }


        public static void EnsureDirectoryDoesNotExist(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static void EnsureFileDoesNotExist(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static bool IsSymbolicLink(string file)
        {
            if (File.Exists(file))
            {
                return FileHasAttribute(file, FileAttributes.ReparsePoint);
            }
            else
            {
                return false;
            }
        }

        public static bool FileHasAttribute(string file, FileAttributes attribute)
        {
            return (File.GetAttributes(file) & attribute) == attribute;
        }

        public static string TypeArrayToString(Type[] types)
        {
            return string.Format("{{{0}}}", string.Join(", ", types.Select((type) => type.Name)));
        }

        public static void CopyFolderAcrossVolumes(string sourceFolder, string destinationFolder)
        {
            EnsureDirectoryExists(destinationFolder);
            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                string name = Path.GetFileName(file);
                string destination = Path.Combine(destinationFolder, name);
                File.Copy(file, destination);
            }
            foreach (string folder in Directory.GetDirectories(sourceFolder))
            {
                string name = Path.GetFileName(folder);
                string destination = Path.Combine(destinationFolder, name);
                CopyFolderAcrossVolumes(folder, destination);
            }
        }

        public static void DeleteAllEmptyFolderTransitively(string folder, bool deleteFolderItselfIfAlsoEmpty = false)
        {
            ForEachFileAndDirectoryTransitively(folder, (string directory, object argument) =>
            {
                if (DirectoryIsEmpty(directory))
                {
                    Directory.Delete(directory);
                }
            }, (string file, object argument) => { }, false, null, null);
            if (deleteFolderItselfIfAlsoEmpty && DirectoryIsEmpty(folder))
            {
                Directory.Delete(folder);
            }
        }

        public static void MoveFolderAcrossVolumes(string sourceFolder, string destinationFolder, bool deleteSourceFolderCompletely = true)
        {
            CopyFolderAcrossVolumes(sourceFolder, destinationFolder);
            DeleteFolder(sourceFolder, deleteSourceFolderCompletely);
        }

        public static void DeleteFolder(string folder, bool deleteSourceFolderCompletely = true)
        {
            if (deleteSourceFolderCompletely)
            {
                Directory.Delete(folder, true);
            }
            else
            {
                DeleteContentOfFolder(folder);
            }
        }
        public static string TwoDimensionalArrayToString<T>(T[,] array)
        {
            return string.Join(",", array.OfType<T>().Select((value, index) => new { value, index }).GroupBy(x => x.index / array.GetLength(1)).Select(x => $"{{{string.Join(",", x.Select(y => y.value))}}}"));
        }

        public static bool TwoDimensionalArrayEquals<T>(T[,] array1, T[,] array2)
        {
            return array1.Rank == array2.Rank && Enumerable.Range(0, array1.Rank).All(dimension => array1.GetLength(dimension) == array2.GetLength(dimension)) && array1.Cast<T>().SequenceEqual(array2.Cast<T>());
        }

        public static void DeleteContentOfFolder(string folder)
        {
            DirectoryInfo directoryInfo = new(folder);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
            {
                subDirectoryInfo.Delete(true);
            }
        }

        internal static bool TryResolvePathByPathVariable(string program, out string programWithFullPath)
        {
            (bool, string) result = OperatingSystem.OperatingSystem.GetCurrentOperatingSystem().Accept(new TryResolvePathByPathVariableVisitor(program));
            programWithFullPath = result.Item2;
            return result.Item1;
        }

        private class TryResolvePathByPathVariableVisitor : IOperatingSystemVisitor<(bool/*Success*/, string/*programWithFullPath*/)>
        {
            private readonly string _Programname;

            public TryResolvePathByPathVariableVisitor(string programname)
            {
                this._Programname = programname;
            }

            public (bool, string) Handle(OSX operatingSystem)
            {
                throw new NotImplementedException();
            }

            public (bool, string) Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                string program = null;
                string[] knownExtension = [".exe", ".cmd", ".bat"];
                string paths = Environment.ExpandEnvironmentVariables("%PATH%");
                bool @break = false;
                foreach (string path in paths.Split(';'))
                {
                    foreach (string combined in GetCombinations(path, knownExtension, this._Programname))
                    {
                        if (File.Exists(combined))
                        {
                            program = combined;
                            @break = true;
                            break;
                        }
                    }
                    if (@break)
                    {
                        break;
                    }
                }
                return (program != null, program);
            }

            public (bool, string) Handle(Linux operatingSystem)
            {
                string program = null;
                string paths = Environment.ExpandEnvironmentVariables("%PATH%");// "$PATH" not used because of https://github.com/dotnet/runtime/issues/25792
                foreach (string path in paths.Split(':'))
                {
                    string combined = Path.Combine(path, this._Programname);
                    if (File.Exists(combined) && SpecialFileInformation.FileIsExecutable(program))
                    {
                        program = combined;
                        break;
                    }
                }
                return (program != null, program);
            }
        }
        private static IEnumerable<string> GetCombinations(string path, string[] knownExtensions, string program)
        {
            string programToLower = program.ToLower();
            foreach (string extension in knownExtensions)
            {
                if (programToLower.EndsWith(extension))
                {
                    return new string[] { Path.Combine(path, programToLower) };
                }
            }
            List<string> result = [];
            foreach (string extension in knownExtensions)
            {
                result.Add(Path.Combine(path, program + extension));
            }
            return result;
        }

        public static void MoveContentOfFoldersAcrossVolumes(string sourceFolder, string targetFolder)
        {
            MoveContentOfFoldersAcrossVolumes(sourceFolder, targetFolder, FileSelector.FilesInFolder(sourceFolder, true));
        }

        public static void MoveContentOfFoldersAcrossVolumes(string sourceFolder, string targetFolder, FileSelector fileSelector, bool deleteAlreadyExistingFilesWithoutCopy = false)
        {
            MoveContentOfFoldersAcrossVolumes(sourceFolder, targetFolder, fileSelector, (exception) => { }, deleteAlreadyExistingFilesWithoutCopy);
        }

        public static void MoveContentOfFoldersAcrossVolumes(string sourceFolder, string targetFolder, Func<string, bool> fileSelectorPredicate, bool deleteAlreadyExistingFilesWithoutCopy = false)
        {
            MoveContentOfFoldersAcrossVolumes(sourceFolder, targetFolder, fileSelectorPredicate, (exception) => { }, deleteAlreadyExistingFilesWithoutCopy);
        }

        public static void MoveContentOfFoldersAcrossVolumes(string sourceFolder, string targetFolder, FileSelector fileSelector, Action<Exception> errorHandler, bool deleteAlreadyExistingFilesWithoutCopy = false)
        {
            MoveContentOfFoldersAcrossVolumes(sourceFolder, targetFolder, (file) => fileSelector.Files.Contains(file), errorHandler, deleteAlreadyExistingFilesWithoutCopy);
        }

        /// <summary>
        /// Moves the content of <paramref name="sourceFolder"/> to <paramref name="targetFolder"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="deleteAlreadyExistingFilesWithoutCopy"/>==true then the files in <paramref name="sourceFolder"/> which do already exist in <paramref name="targetFolder"/> will be deleted without copying them and without any backup. (Only filepath/-name will be compared. The content of the file does not matter for this comparison.)
        /// This function preserves the directory-structure of <paramref name="sourceFolder"/>.
        /// This function ignores empty directories in <paramref name="sourceFolder"/>.
        /// </remarks>
        public static void MoveContentOfFoldersAcrossVolumes(string sourceFolder, string targetFolder, Func<string, bool> fileSelectorPredicate, Action<Exception>? errorHandler, bool deleteAlreadyExistingFilesWithoutCopy = false)
        {
            void fileAction(string sourceFile, object @object)
            {
                try
                {
                    if (fileSelectorPredicate(sourceFile))
                    {
                        string sourceFolderTrimmed = sourceFolder.Trim().TrimStart('/', '\\').TrimEnd('/', '\\');
                        string fileName = Path.GetFileName(sourceFile);
                        string fullTargetFolder = Path.Combine(targetFolder, Path.GetDirectoryName(sourceFile)[sourceFolderTrimmed.Length..].TrimStart('/', '\\'));
                        EnsureDirectoryExists(fullTargetFolder);
                        string targetFile = Path.Combine(fullTargetFolder, fileName);
                        if (File.Exists(targetFile))
                        {
                            if (deleteAlreadyExistingFilesWithoutCopy)
                            {
                                File.Delete(sourceFile);
                            }
                        }
                        else
                        {
                            File.Copy(sourceFile, targetFile);
                            File.Delete(sourceFile);
                        }
                    }
                }
                catch (Exception exception)
                {
                    errorHandler?.Invoke(exception);
                }
            }
            ForEachFileAndDirectoryTransitively(sourceFolder, (directory, obj) => { /*TODO ensure directory exists in target-folder*/}, fileAction, false, null, null);
            if (deleteAlreadyExistingFilesWithoutCopy)
            {
                RemoveContentOfFolder(sourceFolder);
            }
            else
            {
                DeleteAllEmptyFolderTransitively(sourceFolder, false);
            }
        }

        public static void ForEachFileAndDirectoryTransitively(string directory, Action<string, object>? directoryAction, Action<string, object>? fileAction, bool ignoreErrors = false, object? argumentForFileAction = null, object? argumentForDirectoryAction = null)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                try
                {
                    fileAction?.Invoke(file, argumentForFileAction);
                }
                catch
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }
                }
            }
            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                ForEachFileAndDirectoryTransitively(subDirectory, directoryAction, fileAction, ignoreErrors, argumentForFileAction, argumentForDirectoryAction);
                try
                {
                    directoryAction?.Invoke(subDirectory, argumentForDirectoryAction);
                }
                catch
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }
                }
            }
        }

        public static void RemoveContentOfFolder(string folder)
        {
            DirectoryInfo directoryInfo = new(folder);
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                fileInfo.Delete();
            }
            foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
            {
                subDirectoryInfo.Delete(true);
            }
        }

        /// <summary>
        /// Starts all <see cref="Func{TResult}"/>-objects in <paramref name="functions"/> concurrent and return all results which did not throw an exception.
        /// </summary>
        /// <returns>The results of all finished <paramref name="functions"/>-methods with their results.</returns>
        public static ISet<Tuple<Func<T>, T, Exception>> RunAllConcurrentAndReturnAllResults<T>(this ISet<Func<T>> functions, int maximalDegreeOfParallelism = 4)
        {
            ConcurrentBag<Tuple<Func<T>, T, Exception>> result = [];
            Parallel.ForEach(functions, new ParallelOptions { MaxDegreeOfParallelism = maximalDegreeOfParallelism }, (function) =>
            {
                try
                {
                    result.Add(new Tuple<Func<T>, T, Exception>(function, function(), null));
                }
                catch (Exception exception)
                {
                    result.Add(new Tuple<Func<T>, T, Exception>(function, default, exception));
                }
            });
            return new HashSet<Tuple<Func<T>, T, Exception>>(result);
        }

        /// <summary>
        /// Starts all <see cref="ThreadStart"/>-objects in <paramref name="functions"/> concurrent and return the result of the first execution which does not throw an exception.
        /// Warning: This function is not implemented yet.
        /// </summary>
        /// <returns>The result of the first finished <paramref name="functions"/>-method.</returns>
        /// <exception cref="ArgumentException">If <paramref name="functions"/> is empty.</exception>
        /// <exception cref="Exception">If every <paramref name="functions"/>-method throws an exception.</exception>
        public static T RunAllConcurrentAndReturnFirstResult<T>(this ISet<Func<T>> functions, int maximalDegreeOfParallelism = 4, string actionName = "RunAllAndReturnFalse")
        {
            return new RunAllConcurrentAndReturnFirstResultHelper<T>(maximalDegreeOfParallelism).RunAllConcurrentAndReturnFirstResult(functions, actionName);
        }

        private class RunAllConcurrentAndReturnFirstResultHelper<T>
        {
            private T _Result = default;
            private bool _ResultSet = false;
            public readonly object _LockObject = new();
            private int _AmountOfRunningFunctions = 0;
            private readonly int _MaximalDegreeOfParallelism = 4;

            public RunAllConcurrentAndReturnFirstResultHelper(int maximalDegreeOfParallelism)
            {
                this._MaximalDegreeOfParallelism = maximalDegreeOfParallelism;
            }

            private T Result
            {
                get
                {
                    lock (this._LockObject)
                    {
                        return this._Result;
                    }
                }
                set
                {
                    lock (this._LockObject)
                    {
                        if (!this.ResultSet)
                        {
                            this._Result = value;
                            this.ResultSet = true;
                        }
                    }
                }
            }
            private bool ResultSet
            {
                get
                {
                    lock (this._LockObject)
                    {
                        return this._ResultSet;
                    }
                }
                set
                {
                    lock (this._LockObject)
                    {
                        this._ResultSet = value;
                    }
                }
            }
            public T RunAllConcurrentAndReturnFirstResult(ISet<Func<T>> functions, string actionName)
            {
                if (functions.Count == 0)
                {
                    throw new ArgumentException($"Argument '{nameof(functions)}' does not contain any function.");
                }
                Parallel.ForEach(functions, new ParallelOptions { MaxDegreeOfParallelism = this._MaximalDegreeOfParallelism }, new Action<Func<T>, ParallelLoopState>((Func<T> function, ParallelLoopState state) =>
                {
                    try
                    {
                        Interlocked.Increment(ref this._AmountOfRunningFunctions);
                        T result = function();
                        this.Result = result;
                        state.Stop();
                    }
                    finally
                    {
                        Interlocked.Decrement(ref this._AmountOfRunningFunctions);
                    }
                }));
                WaitUntilConditionIsTrue(() => (this.ResultSet || this._AmountOfRunningFunctions == 0, null), actionName);
                if (this._AmountOfRunningFunctions == 0 && !this.ResultSet)
                {
                    throw new Exception("No result was calculated");
                }
                else
                {
                    return this.Result;
                }
            }
        }
        public static void WaitUntilConditionIsTrue(Func<bool> condition, string actionName)
        {
            WaitUntilConditionIsTrue(condition, TimeSpan.FromDays(14), actionName);
        }
        public static void WaitUntilConditionIsTrue(Func<bool> condition, TimeSpan timeout, string actionName)
        {
            WaitUntilConditionIsTrueAsync(condition, timeout, actionName).Wait();
        }
        public static async Task WaitUntilConditionIsTrueAsync(Func<bool> condition, string actionName)
        {
            await WaitUntilConditionIsTrueAsync(condition, TimeSpan.FromDays(14), actionName);
        }
        public static async Task WaitUntilConditionIsTrueAsync(Func<bool> condition, TimeSpan timeout, string actionName)
        {
            await WaitUntilConditionIsTrueAsync(() => (condition(), null), timeout, actionName);
        }
        public static void WaitUntilConditionIsTrue(Func<(bool, Exception?)> condition, string actionName)
        {
            WaitUntilConditionIsTrue(condition, TimeSpan.FromDays(14), actionName);
        }
        public static void WaitUntilConditionIsTrue(Func<(bool, Exception?)> condition, TimeSpan timeout, string actionName)
        {
            WaitUntilConditionIsTrueAsync(condition, timeout, actionName).Wait();
        }
        public static async Task WaitUntilConditionIsTrueAsync(Func<(bool, Exception?)> condition, string actionName)
        {
            await WaitUntilConditionIsTrueAsync(condition, TimeSpan.FromDays(14), actionName);
        }
        public static async Task WaitUntilConditionIsTrueAsync(Func<(bool, Exception?)> condition, TimeSpan timeout, string actionName)
        {
            Exception? lastException = null;
            if (!RunWithTimeout(() =>
            {
                (bool, Exception?) lastResult = condition();
                lastException = lastResult.Item2;
                while (!lastResult.Item1)
                {
                    Thread.Sleep(50);
                    lastResult = condition();
                    lastException = lastResult.Item2;
                }
            }, timeout))
            {
                throw new TimeoutException($"Action {actionName} resulted not in true within the timeout of {GRYLibrary.Core.Misc.Utilities.DurationToUserFriendlyString(timeout)}.", lastException);
            }
        }
        public static ISet<string> ToCaseInsensitiveSet(this ISet<string> input)
        {
            ISet<WriteableTuple<string, string>> tupleList = new HashSet<WriteableTuple<string, string>>(input.Select((item) => new WriteableTuple<string, string>(item, item.ToLower())));
            return new HashSet<string>(tupleList.Select((item) => item.Item1));
        }
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> dictionary = new ExpandoObject();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
            {
                dictionary.Add(property.Name, property.GetValue(value));
            }
            return dictionary as ExpandoObject;
        }

        public static T DeepClone<T>(this T @object)
        {
            return Generic.GenericDeserialize<T>(Generic.GenericSerialize(@object));
        }

        /// <summary>
        /// Casts an object to the given type if possible.
        /// This can be useful for example to to cast 'Action&lt;Object&gt;' to 'Action' or 'Func&lt;string&gt;' to 'Func&lt;Object&gt;' to fulfil interface-compatibility.
        /// </summary>
        internal /*TODO change to public when it works properly*/ static object Cast2(object @object, Type targetType)
        {
            return Cast(@object, targetType, DefaultConversions);
        }

        private static readonly IList<object> _DefaultConversions = [];
        public static IList<object> DefaultConversions => [.. _DefaultConversions];
        public static object Cast(object @object, Type targetType, IList<object> customConversions)
        {
            Type typeOfObject = @object.GetType();
            if (typeOfObject.Equals(targetType))
            {
                return @object;
            }
            foreach (object customConversion in customConversions)
            {
                if (TypeComparerIgnoringGenerics.Equals(typeOfObject, (Type)customConversion/*.GetTypeWhichIsApplicable()*/))
                {
                    // return customConversion.Convert(@object,typeOfObject);
                }
            }
            MethodInfo method = typeof(Utilities).GetMethod(nameof(CastHelper), BindingFlags.NonPublic | BindingFlags.Static);
            method = method.MakeGenericMethod([targetType]);
            return method.Invoke(null, [@object]);

        }
        private static T CastHelper<T>(object @object)
        {
            return (T)(dynamic)@object;
        }

        public static long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        }
        public static SimpleObjectPersistence<T> PersistToDisk<T>(this T @object, string file) where T : new()
        {
            SimpleObjectPersistence<T> result = SimpleObjectPersistence<T>.CreateByObjectAndFile(@object, file);
            result.SaveObjectToFile();
            return result;
        }
        public static SimpleObjectPersistence<T> LoadFromDisk<T>(this string file) where T : new()
        {
            SimpleObjectPersistence<T> result = SimpleObjectPersistence<T>.CreateByFile(file);
            result.LoadObjectFromFile();
            return result;
        }
        /// <returns>Returns the command line arguments of the current executed program.</returns>
        /// <remarks>It is guaranteed that the result does not have leading or trailing whitespaces.</remarks>
        public static string GetCommandLineArguments()
        {
            string rawCmd = Environment.CommandLine;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Count() == 1)
            {
                return string.Empty;
            }
            string exe = args[0];
            string quotedExe = "\"" + exe + "\"";
            if (rawCmd.StartsWith(exe))
            {
                rawCmd = rawCmd[(exe.Length + 1)..];
            }
            else if (rawCmd.StartsWith(quotedExe))
            {
                rawCmd = rawCmd[(quotedExe.Length + 1)..];
            }
            return rawCmd.Trim();
        }

        public static bool FileEndsWithEmptyLine(string file)
        {
            return File.ReadAllBytes(file).Last().Equals(10);
        }

        public static bool FileIsEmpty(string file)
        {
            return File.ReadAllBytes(file).Count().Equals(0);
        }

        public static bool AppendFileDoesNotNeedNewLineCharacter(string file)
        {
            return FileIsEmpty(file) || FileEndsWithEmptyLine(file);
        }

        public static bool AppendFileDoesNeedNewLineCharacter(string file)
        {
            return !AppendFileDoesNotNeedNewLineCharacter(file);
        }

        public static void AppendLineToFile(string file, string content, Encoding encoding)
        {
            string stringToAppend;
            if (AppendFileDoesNeedNewLineCharacter(file))
            {
                stringToAppend = "\n";
            }
            else
            {
                stringToAppend = string.Empty;
            }
            stringToAppend = stringToAppend + content;
            File.AppendAllText(file, stringToAppend, encoding);
        }
        private class IsAbsoluteLocalFilePathVisitor : IOperatingSystemVisitor<bool>
        {
            private readonly string _Path;

            public IsAbsoluteLocalFilePathVisitor(string path)
            {
                this._Path = path;
            }

            public bool Handle(OSX operatingSystem)
            {
                return this._Path.StartsWith('/');
            }

            public bool Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                if (this._Path.StartsWith('/') || this._Path.StartsWith('\\'))
                {
                    return true;
                }
                else
                {
                    int colonCount = this._Path.Count(c => c == ':');
                    List<char> invalidFileNameChars = [.. Path.GetInvalidFileNameChars()];
                    invalidFileNameChars.Remove(':');
                    invalidFileNameChars.Remove('\\');
                    invalidFileNameChars.Remove('/');

                    bool c1 = colonCount > 1;
                    bool c2 = colonCount == 1 && (this._Path.Length <= 1 || (this._Path.Length > 1 && this._Path[1] != ':'));
                    bool c3 = this._Path.Any(c =>
                    {
                        if (invalidFileNameChars.Contains(c))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                    bool c4 = this._Path.Contains("//") || this._Path.Contains(@"\\");

                    bool isInvalid = c1 || c2 || c3 || c4;

                    if (isInvalid)
                    {
                        throw new ArgumentException($"'{this._Path}' is invalid as path.");
                    }
                    else
                    {
                        return colonCount == 1;
                    }
                }
            }

            public bool Handle(Linux operatingSystem)
            {
                return this._Path.StartsWith('/');
            }
        }
        public static bool IsAbsoluteLocalFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("No path given.");
            }
            else
            {
                if (path.Length != path.TrimStart().Length)
                {
                    throw new NotSupportedException($"Leading whitespaces like in '{path}' are not supported");
                }
                return OperatingSystem.OperatingSystem.GetCurrentOperatingSystem().Accept(new IsAbsoluteLocalFilePathVisitor(path));
            }
        }
        public static bool IsRelativeLocalFilePath(string path)
        {
            return !IsAbsoluteLocalFilePath(path);
        }

        public static string GetAbsolutePath(string basePath, string relativePath)
        {
            if (basePath == null && relativePath == null)
            {
                Path.GetFullPath(".");
            }
            if (relativePath == null)
            {
                return basePath.Trim();
            }
            if (basePath == null)
            {
                basePath = Path.GetFullPath(".");
            }
            relativePath = relativePath.Trim();
            basePath = basePath.Trim();
            string finalPath;
            if (!Path.IsPathRooted(relativePath) || @"\".Equals(Path.GetPathRoot(relativePath)))
            {
                if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    finalPath = Path.Combine(Path.GetPathRoot(basePath), relativePath.TrimStart(Path.DirectorySeparatorChar));
                }
                else
                {
                    finalPath = Path.Combine(basePath, relativePath);
                }
            }
            else
            {
                finalPath = relativePath;
            }
            return Path.GetFullPath(finalPath);
        }
        public static bool DirectoryIsEmpty(string path)
        {
            return (Directory.GetFiles(path).Length == 0) && (Directory.GetDirectories(path).Length == 0);
        }

        public static bool DirectoryDoesNotContainFiles(string path)
        {
            if (Directory.GetFiles(path).Length > 0)
            {
                return false;
            }
            foreach (string subFolder in Directory.GetDirectories(path))
            {
                if (!DirectoryDoesNotContainFiles(subFolder))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool DirectoryDoesNotContainFolder(string path)
        {
            return Directory.GetFiles(path).Length > 0;
        }

        public static void ClearFile(string file)
        {
            File.WriteAllText(file, string.Empty, Encoding.ASCII);
        }

        private const char SingleQuote = '\'';
        private const char DoubleQuote = '"';
        private const char Slash = '/';
        private const char Backslash = '\\';
        public static string EnsurePathStartsWithSlash(this string path)
        {
            if (path.StartsWith(Slash.ToString()))
            {
                return path;
            }
            else
            {
                return Slash + path;
            }
        }
        public static string EnsurePathStartsWithBackslash(this string path)
        {
            if (path.StartsWith(Slash.ToString()))
            {
                return path;
            }
            else
            {
                return Backslash + path;
            }
        }
        public static string EnsurePathStartsWithoutSlash(this string path)
        {
            if (path.StartsWith(Slash.ToString()))
            {
                return path.TrimStart(Slash);
            }
            else
            {
                return path;
            }
        }
        public static string EnsurePathStartsWithoutBackslash(this string path)
        {
            if (path.StartsWith(Backslash.ToString()))
            {
                return path.TrimStart(Slash);
            }
            else
            {
                return path;
            }
        }
        public static string EnsurePathEndsWithSlash(this string path)
        {
            if (path.EndsWith(Slash.ToString()))
            {
                return path;
            }
            else
            {
                return path + Slash;
            }
        }
        public static string EnsurePathEndsWithBackslash(this string path)
        {
            if (path.EndsWith(Backslash.ToString()))
            {
                return path;
            }
            else
            {
                return path + Backslash;
            }
        }
        public static string EnsurePathEndsWithoutSlash(this string path)
        {
            if (path.EndsWith(Slash.ToString()))
            {
                return path.TrimEnd(Slash);
            }
            else
            {
                return path;
            }
        }
        public static string EnsurePathEndsWithoutBackslash(this string path)
        {
            if (path.EndsWith(Backslash.ToString()))
            {
                return path.TrimEnd(Backslash);
            }
            else
            {
                return path;
            }
        }
        public static string EnsurePathStartsWithoutSlashOrBackslash(this string path)
        {
            return path.EnsurePathStartsWithoutSlash().EnsurePathStartsWithoutBackslash();
        }

        public static string EnsurePathEndsWithoutSlashOrBackslash(this string path)
        {
            return path.EnsurePathEndsWithoutSlash().EnsurePathEndsWithoutBackslash();
        }

        public static string EnsurePathHasNoLeadingOrTrailingQuotes(this string path)
        {
            string result = path;
            bool changed = true;
            while (changed)
            {
                string old = result;
                result = result.TrimStart(SingleQuote).TrimEnd(SingleQuote).TrimStart(DoubleQuote).TrimEnd(DoubleQuote);
                changed = old != result;
            }
            return result;
        }

        public static bool StartsWith<T>(T[] entireArray, T[] start)
        {
            if (start.Count() > entireArray.Count())
            {
                return false;
            }
            for (int i = 0; i < start.Length; i++)
            {
                if (!entireArray[i].Equals(start[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static byte[] StringToByteArray(string @string)
        {
            return new UTF8Encoding(false).GetBytes(@string);
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            return new UTF8Encoding(false).GetString(bytes);
        }

        public static string ByteArrayToHexString(byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", string.Empty);
        }

        public static byte[] HexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 == 1)
            {
                throw new ArgumentException($"Parameter {nameof(hexString)} may not have an odd amount of characters");
            }
            hexString = hexString.ToUpper();
            byte[] result = new byte[hexString.Length >> 1];
            for (int i = 0; i < hexString.Length >> 1; ++i)
            {
                result[i] = (byte)((GetHexValue(hexString[i << 1]) << 4) + GetHexValue(hexString[(i << 1) + 1]));
            }
            return result;
        }

        private static int GetHexValue(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : 55);
        }

        public static string BigIntegerToHexString(BigInteger input)
        {
            return input.ToString("X");
        }

        public static BigInteger HexStringToBigInteger(string input)
        {
            return BigInteger.Parse(input.ToUpper(), NumberStyles.HexNumber);
        }

        public static T[] Concat<T>(params T[][] arrays)
        {
            T[] result = Array.Empty<T>();
            foreach (T[] array in arrays)
            {
                result = Concat2Arrays(result, array);
            }
            return result;
        }
        private static T[] Concat2Arrays<T>(T[] array1, T[] array2)
        {
            T[] result = new T[array1.Length + array2.Length];
            array1.CopyTo(result, 0);
            array2.CopyTo(result, array1.Length);
            return result;
        }
        public static bool StringToBoolean(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            value = value.Trim().ToLower();
            return value is "1"
                or "y"
                or "yes"
                or "true";
        }
        public static string[] SplitOnNewLineCharacter(string input)
        {
            return [.. input.Split([Environment.NewLine], StringSplitOptions.None).Select(line => line.Replace("\r", string.Empty).Replace("\n", string.Empty))];
        }
        public static void AssertCondition(bool condition, Func<string> messageForFailedAssertion, bool @break = false)
        {
            if (!condition)
            {
                if (@break && Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                string message = messageForFailedAssertion();
                throw new AssertionException("Assertion failed. Condition is false" + (string.IsNullOrWhiteSpace(message) ? "." : ": " + message));
            }
        }
        /// <summary>
        /// Throws an exception if <paramref name="condition"/>==false.
        /// </summary>
        /// <remarks>
        /// The purpose of this function is to verify assertions at runtime.
        /// It is not supposed to be used for unit-test-assertions.
        /// </remarks>
        public static void AssertCondition(bool condition, string messageForFailedAssertion = EmptyString, bool @break = false)
        {
            AssertCondition(condition, () => messageForFailedAssertion, @break);
        }
        public static T AssertNotNull<T>(T? variable, string variableName, bool @break = false)
        {
            AssertCondition(variable != null, $"Variable '{variableName}' is null.", @break);
            return variable!;
        }
        public static void FormatCSVFile(string file, string separator = ";", bool firstLineContainsHeadlines = false)
        {
            FormatCSVFile(file, new UTF8Encoding(false), separator, firstLineContainsHeadlines);
        }

        public static void FormatCSVFile(string file, Encoding encoding, string separator = ";", bool firstLineContainsHeadlines = false)
        {
            UpdateCSVFileEntry(file, encoding, (line) => line, separator, firstLineContainsHeadlines);
        }

        public static void UpdateCSVFileEntry(string file, Func<string[], string[]> updateFunction, string separator = ";", bool firstLineContainsHeadlines = false)
        {
            UpdateCSVFileEntry(file, new UTF8Encoding(false), updateFunction, separator, firstLineContainsHeadlines);
        }

        public static void UpdateCSVFileEntry(string file, Encoding encoding, Func<string[], string[]> updateFunction, string separator = ";", bool firstLineContainsHeadlines = false)
        {
            IList<string[]> content = ReadCSVFile(file, encoding, out string[] headlines, separator, firstLineContainsHeadlines);
            content = [.. content.Select(line => updateFunction(line))];
            WriteCSVFile(file, content, headlines, separator);
        }
        public static void WriteCSVFile(string file, IList<string[]> content, string[] headLines, string separator = ";")
        {
            WriteCSVFile(file, new UTF8Encoding(false), content, headLines, separator);
        }

        public static void WriteCSVFile(string file, Encoding encoding, IList<string[]> content, string[] headLines, string separator = ";")
        {
            List<string[]> contentAdjusted = [.. content];
            if (headLines != null)
            {
                contentAdjusted.Insert(0, headLines);
            }
            EscapeForCSV(headLines);
            foreach (string[] line in content)
            {
                EscapeForCSV(headLines);
            }
            // TODO insert padding
            File.WriteAllLines(file, contentAdjusted.Select(item => string.Join(separator, item)), encoding);
        }

        internal static void EscapeForCSV(string[] line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                line[i] = EscapeForCSV(line[i]);
            }
        }
        internal static string EscapeForCSV(string item)
        {
            if (item.Contains('"'))
            {
                item = item.Replace("\"", "\"\"");
                item = $"\"{item}\"";
            }
            return item;
        }

        public static IList<string[]> ReadCSVFile(string file, out string[] headLines, string separator = ";", bool firstLineContainsHeadlines = false, bool trimValues = true, bool treatHashAsComment = false)
        {
            return ReadCSVFile(file, new UTF8Encoding(false), out headLines, separator, firstLineContainsHeadlines, trimValues, treatHashAsComment);
        }

        public static IList<string[]> ReadCSVFile(string file, Encoding encoding, out string[] headLines, string separator = ";", bool firstLineContainsHeadlines = false, bool trimValues = true, bool treatHashAsComment = false)
        {
            List<string[]> outterList = [];
            string[] lines = File.ReadAllLines(file, encoding);
            List<string> headlineValues = [];
            bool isFirstLine = true;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!(treatHashAsComment && line.StartsWith('#')))
                {
                    if (isFirstLine && firstLineContainsHeadlines)
                    {
                        headlineValues.AddRange(line.Split([separator], StringSplitOptions.None).Select(item => NormalizeCSVItemForReading(item, trimValues)));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            List<string> innerList = [];
                            if (line.Contains(separator))
                            {
                                innerList.AddRange(line.Split([separator], StringSplitOptions.None).Select(item => NormalizeCSVItemForReading(item, trimValues)));
                            }
                            else
                            {
                                innerList.Add(line);
                            }
                            outterList.Add([.. innerList]);
                        }
                    }
                    isFirstLine = false;
                }
            }
            if (firstLineContainsHeadlines)
            {
                headLines = [.. headlineValues];
            }
            else
            {
                headLines = null;
            }
            return outterList;
        }

        private static string NormalizeCSVItemForReading(string value, bool trimValues)
        {
            if (trimValues)
            {
                value = value.Trim();
            }
            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                value = value[1..];
                value = value[..^1];
                value = value.Replace("\"\"", "\"");
                value = value.Trim();
            }
            return value;
        }

        /// <summary>
        /// Executes <paramref name="action"/>. When <paramref name="action"/> longer takes than <paramref name="timeout"/> then <paramref name="action"/> will be aborted.
        /// </summary>
        /// <returns>
        /// Returns true if and only if the action was terminated in the given timespan.
        /// So this function returns true if and only if the action was not completed within the given <paramref name="timeout"/>.
        /// </returns>

        public static bool RunWithTimeout(this Action action, TimeSpan timeout, Action<Exception>? errorHandler = default)
        {
            Thread workerThread = new(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    errorHandler?.Invoke(exception);
                }
            });
            workerThread.Start();
            bool terminatedInGivenTimeSpan = workerThread.Join(timeout);
            if (!terminatedInGivenTimeSpan)
            {
                workerThread.Interrupt();
            }
            return terminatedInGivenTimeSpan;
        }

        public static void WaitUntilPortIsAvailable(string address, ushort port, TimeSpan timeout)
        {
            RunWithTimeout(() => WaitUntilPortIsAvailable(address, port), timeout);
        }

        public static void WaitUntilPortIsAvailable(string address, ushort port)
        {
            while (!PortIsAvailable(address, port))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        public static bool PortIsAvailable(string address, ushort port)
        {
            using TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(address, port);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool WebsiteIsAvailable(string link, bool ensureSuccessStatusCode = true)
        {
            return WebsiteIsAvailable(link, ensureSuccessStatusCode, new Dictionary<string, string>());
        }

        public static bool WebsiteIsAvailable(string link, bool ensureSuccessStatusCode, IDictionary<string, string> extraHeaders)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                foreach (System.Collections.Generic.KeyValuePair<string, string> header in extraHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                HttpResponseMessage response = httpClient.GetAsync(link).WaitAndGetResult();
                if (ensureSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string ResolveToFullPath(this string path)
        {
            return ResolveToFullPath(path, Directory.GetCurrentDirectory());
        }

        /// <summary>
        /// This function transforms <paramref name="path"/> into an absolute path.
        /// It does not matter if you pass a relative or absolute path: This function checks that.
        /// </summary>
        /// <returns>
        /// Returns an absolute path.
        /// </returns>
        public static string ResolveToFullPath(this string path, string baseDirectory)
        {
            path = path.Trim();
            if (Path.IsPathFullyQualified(path))
            {
                return NormalizePath(path);
            }
            else
            {
                return NormalizePath(Path.GetFullPath(new Uri(Path.Combine(baseDirectory, path)).LocalPath));
            }
        }
        public static XmlDocument XDocumentToXMLDocument(this XDocument xDocument)
        {
            XmlDocument xmlDocument = new();
            using XmlReader xmlReader = xDocument.CreateReader();
            xmlDocument.Load(xmlReader);
            return xmlDocument;
        }

        public static XDocument XMLDocumentToXDocument(this XmlDocument xmlDocument)
        {
            using XmlNodeReader xmlNodeReader = new(xmlDocument);
            xmlNodeReader.MoveToContent();
            return XDocument.Load(xmlNodeReader);
        }
        /// <summary>
        /// This function takes a given <paramref name="xml"/>-string and validates it against a given <paramref name="xsd"/>-string.
        /// </summary>
        /// <returns>
        /// This function returns true if and only if <paramref name="errorMessages"/> is empty.
        /// If this function returns true it means, that <paramref name="xml"/> is structured according to <paramref name="xsd"/>.
        /// </returns>
        public static bool ValidateXMLAgainstXSD(string xml, string xsd, out IList<object> errorMessages)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xml);
            XmlSchema xsdDocument = XmlSchema.Read(new StringReader(xsd), null);
            return ValidateXMLAgainstXSD(xmlDocument, xsdDocument, out errorMessages);
        }
        public static bool ValidateXMLAgainstXSD(string xml, XmlSchema xsdDocument, out IList<object> errorMessages)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xml);
            return ValidateXMLAgainstXSD(xmlDocument, xsdDocument, out errorMessages);
        }
        public static bool ValidateXMLAgainstXSD(XmlDocument xmlDocument, string xsd, out IList<object> errorMessages)
        {
            XmlSchema xsdDocument = XmlSchema.Read(new StringReader(xsd), null);
            return ValidateXMLAgainstXSD(xmlDocument, xsdDocument, out errorMessages);
        }
        public static bool ValidateXMLAgainstXSD(XmlDocument xmlDocument, XmlSchema xsdDocument, out IList<object> errorMessages)
        {
            errorMessages = [];
            try
            {
                XmlSchemaSet schemaSet = new();
                schemaSet.Add(xsdDocument);

                List<object> events = [];
                XDocument xDocument = xmlDocument.XMLDocumentToXDocument();
                xDocument.Validate(schemaSet, (o, eventArgument) => events.Add(eventArgument));
                foreach (object @event in events)
                {
                    errorMessages.Add(@event);
                }
            }
            catch (Exception exception)
            {
                errorMessages.Add(exception);
            }
            return errorMessages.Count == 0;
        }

        public static readonly XmlWriterSettings XMLWriterDefaultSettings = new() { Indent = true, Encoding = new UTF8Encoding(false), OmitXmlDeclaration = false, IndentChars = "    " };

        public static XmlDocument ApplyXSLTToXML(string xml, string xslt)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xml);
            XslCompiledTransform xsltDocument = new();
            xsltDocument.Load(XmlReader.Create(new StringReader(xslt)));
            return ApplyXSLTToXML(xmlDocument, xsltDocument, XMLWriterDefaultSettings);
        }
        public static XmlDocument ApplyXSLTToXML(string xml, string xslt, XmlWriterSettings applyXSLTToXMLXMLWriterDefaultSettings)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xml);
            XslCompiledTransform xsltDocument = new();
            xsltDocument.Load(XmlReader.Create(new StringReader(xslt)));
            return ApplyXSLTToXML(xmlDocument, xsltDocument, applyXSLTToXMLXMLWriterDefaultSettings);
        }
        public static XmlDocument ApplyXSLTToXML(XmlDocument xmlDocument, string xslt)
        {
            XslCompiledTransform xsltDocument = new();
            xsltDocument.Load(XmlReader.Create(new StringReader(xslt)));
            return ApplyXSLTToXML(xmlDocument, xsltDocument, XMLWriterDefaultSettings);
        }
        public static XmlDocument ApplyXSLTToXML(XmlDocument xmlDocument, string xslt, XmlWriterSettings applyXSLTToXMLXMLWriterDefaultSettings)
        {
            XslCompiledTransform xsltDocument = new();
            xsltDocument.Load(XmlReader.Create(new StringReader(xslt)));
            return ApplyXSLTToXML(xmlDocument, xsltDocument, applyXSLTToXMLXMLWriterDefaultSettings);
        }
        public static XmlDocument ApplyXSLTToXML(string xml, XslCompiledTransform xsltDocument)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xml);
            return ApplyXSLTToXML(xmlDocument, xsltDocument, XMLWriterDefaultSettings);
        }
        public static XmlDocument ApplyXSLTToXML(string xml, XslCompiledTransform xsltDocument, XmlWriterSettings applyXSLTToXMLXMLWriterDefaultSettings)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xml);
            return ApplyXSLTToXML(xmlDocument, xsltDocument, applyXSLTToXMLXMLWriterDefaultSettings);
        }
        public static XmlDocument ApplyXSLTToXML(XmlDocument xmlDocument, XslCompiledTransform xsltDocument, XmlWriterSettings applyXSLTToXMLXMLWriterDefaultSettings)
        {
            using StringWriter stringWriter = new();
            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, applyXSLTToXMLXMLWriterDefaultSettings);
            using XmlReader xmlReader = new XmlNodeReader(xmlDocument);
            xsltDocument.Transform(xmlReader, xmlWriter);
            XmlDocument result = new();
            result.LoadXml(stringWriter.ToString());
            return result;

        }
        public static readonly Encoding FormatXMLFile_DefaultEncoding = new UTF8Encoding(false);
        public static void FormatXMLFile(string file)
        {
            FormatXMLFile(file, FormatXMLFile_DefaultEncoding, XMLWriterDefaultSettings);
        }

        public static void FormatXMLFile(string file, Encoding encoding)
        {
            FormatXMLFile(file, encoding, XMLWriterDefaultSettings);
        }

        public static void FormatXMLFile(string file, XmlWriterSettings settings)
        {
            FormatXMLFile(file, FormatXMLFile_DefaultEncoding, settings);
        }

        public static void FormatXMLFile(string file, Encoding encoding, XmlWriterSettings settings)
        {
            File.WriteAllText(file, FormatXMLString(File.ReadAllText(file, encoding), settings), encoding);
        }

        public static string FormatXMLString(string xmlString)
        {
            return FormatXMLString(xmlString, XMLWriterDefaultSettings);
        }

        public static string FormatXMLString(string xmlString, XmlWriterSettings settings)
        {
            using MemoryStream memoryStream = new();
            using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings);
            XmlDocument document = new();
            document.LoadXml(xmlString);
            xmlWriter.Flush();
            memoryStream.Flush();
            memoryStream.Position = 0;
            using StreamReader streamReader = new(memoryStream);
            return streamReader.ReadToEnd();
        }
        public static uint BinaryStringToUint(string binaryString)
        {
            return (uint)Convert.ToInt32(binaryString, 2);
        }

        public static string UintToBinaryString(uint binaryString, bool padLeft = true)
        {
            string result = Convert.ToString(binaryString, 2);
            if (padLeft)
            {
                result = result.PadLeft(32, '0');
            }
            return result;
        }
        public static BigInteger BinaryStringToBigInteger(string binaryString)
        {
            throw new NotImplementedException();
        }

        public static string BigIntegerToBinaryString(BigInteger binaryString)
        {
            throw new NotImplementedException();
        }

        public static string XmlToString(XmlDocument xmlDocument)
        {
            return XmlToString(xmlDocument, new UTF8Encoding(false), XMLWriterDefaultSettings);
        }

        public static string XmlToString(XmlDocument xmlDocument, Encoding encoding)
        {
            return XmlToString(xmlDocument, encoding, XMLWriterDefaultSettings);
        }

        public static string XmlToString(XmlDocument xmlDocument, XmlWriterSettings xmlWriterSettings)
        {
            return XmlToString(xmlDocument, new UTF8Encoding(false), xmlWriterSettings);
        }

        public static string XmlToString(XmlDocument xmlDocument, Encoding encoding, XmlWriterSettings xmlWriterSettings)
        {
            using StringWriterWithEncoding stringWriter = new(encoding);
            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);
            xmlDocument.Save(xmlWriter);
            // Specification: line-endings are always LF. Carriage-return-characters get removed so that the output is platform-independent.
            return stringWriter.ToString().Replace("\r", string.Empty);
        }
        public static bool IsSelfSIgned(X509Certificate certificate)
        {
            return certificate.Subject.Equals(certificate.Issuer);
        }

        public static void AddMountPointForVolume(Guid volumeId, string mountPoint)
        {
            if (mountPoint.Length > 3)
            {
                EnsureDirectoryExists(mountPoint);
            }
            using ExternalProgramExecutor externalProgramExecutor = new("mountvol", $"{mountPoint} \\\\?\\Volume{{{volumeId}}}\\");
            externalProgramExecutor.LogObject = GRYLog.Create();
            externalProgramExecutor.LogObject.Configuration.Enabled = false;
            externalProgramExecutor.Run();
            if (externalProgramExecutor.ExitCode != 0)
            {
                throw new Exception($"Exitcode of mountvol was {externalProgramExecutor.ExitCode}. StdOut:" + string.Join(Environment.NewLine, externalProgramExecutor.AllStdOutLines) + "; StdErr:" + string.Join(Environment.NewLine, externalProgramExecutor.AllStdErrLines));
            }
        }
        public static ISet<Guid> GetAvailableVolumeIds()
        {
            using ExternalProgramExecutor externalProgramExecutor = new("mountvol", string.Empty);
            externalProgramExecutor.LogObject = GRYLog.Create();
            externalProgramExecutor.LogObject.Configuration.Enabled = false;
            externalProgramExecutor.Run();
            if (externalProgramExecutor.ExitCode != 0)
            {
                throw new Exception($"Exitcode of mountvol was {externalProgramExecutor.ExitCode}. StdErr:" + string.Join(Environment.NewLine, externalProgramExecutor.AllStdErrLines));
            }
            HashSet<Guid> result = [];
            for (int i = 0; i < externalProgramExecutor.AllStdOutLines.Length - 1; i++)
            {
                string rawLine = externalProgramExecutor.AllStdOutLines[i];
                try
                {
                    string line = rawLine.Trim(); //line looks like "\\?\Volume{80aa12de-7392-4051-8cd2-f28bf56dc9d3}\"
                    string prefix = @"\\?\Volume{";
                    if (line.StartsWith(prefix))
                    {
                        line = line[prefix.Length..]; //remove "\\?\Volume{"
                        line = line[0..^2]; //remove "}\"
                        string nextLine = externalProgramExecutor.AllStdOutLines[i + 1].Trim();
                        if (Directory.Exists(nextLine) || nextLine.StartsWith("***"))
                        {
                            result.Add(Guid.Parse(line));
                        }
                    }
                }
                catch
                {
                    NoOperation();
                }
            }
            return result;
        }
        public static ISet<string> GetAllMountPointsOfAllAvailableVolumes()
        {
            HashSet<string> result = [];
            foreach (Guid volumeId in GetAvailableVolumeIds())
            {
                result.UnionWith(GetMountPoints(volumeId));
            }
            return result;
        }
        public static ISet<string> GetMountPoints(Guid volumeId)
        {
            //TODO this function must be implemented depending on os
            HashSet<string> result = [];
            using ExternalProgramExecutor externalProgramExecutor = new("mountvol", string.Empty);
            externalProgramExecutor.LogObject = GRYLog.Create();
            externalProgramExecutor.LogObject.Configuration.Enabled = false;
            externalProgramExecutor.Run();
            if (externalProgramExecutor.ExitCode != 0)
            {
                throw new Exception($"Exitcode of mountvol was {externalProgramExecutor.ExitCode}. StdErr:" + string.Join(Environment.NewLine, externalProgramExecutor.AllStdErrLines));
            }

            for (int indexOfCurrentLine = 0; indexOfCurrentLine < externalProgramExecutor.AllStdOutLines.Length - 1; indexOfCurrentLine++)
            {
                string line = externalProgramExecutor.AllStdOutLines[indexOfCurrentLine].Trim();
                if (line.StartsWith($"\\\\?\\Volume{{{volumeId}}}\\"))
                {
                    int indexOfNextLine = indexOfCurrentLine + 1;
                    bool tryNextLine = true;
                    while (tryNextLine)
                    {
                        if (indexOfNextLine <= externalProgramExecutor.AllStdOutLines.Length - 1)
                        {
                            string nextLine = externalProgramExecutor.AllStdOutLines[indexOfNextLine].Trim();
                            if (!nextLine.StartsWith(@"\\?\") && Directory.Exists(nextLine))
                            {
                                result.Add(nextLine);
                            }
                            else
                            {
                                tryNextLine = false;
                            }
                            indexOfNextLine = indexOfNextLine + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }
            return result;
        }
        public static void RemoveAllMountPointsOfVolume(Guid volumeId)
        {
            foreach (string mountPoint in GetMountPoints(volumeId))
            {
                RemoveMountPointOfVolume(mountPoint);
            }
        }
        public static void RemoveMountPointOfVolume(string mountPoint)
        {
            using ExternalProgramExecutor externalProgramExecutor = new("mountvol", $"{mountPoint} /d");
            externalProgramExecutor.Configuration.WaitingState = new RunSynchronously() { ThrowErrorIfExitCodeIsNotZero = false };
            externalProgramExecutor.Run();
            if (externalProgramExecutor.ExitCode != 0)
            {
                throw new Exception($"Exitcode of mountvol was {externalProgramExecutor.ExitCode}. StdErr:{Environment.NewLine}" + string.Join(Environment.NewLine, externalProgramExecutor.AllStdErrLines));
            }
            if (mountPoint.Length > 3)//if mountpoint is something like "C:\Folder\MyMountPoint" (and not something like "H:\") then remove the folder "MyMountPoint"
            {
                EnsureDirectoryDoesNotExist(mountPoint);
            }
        }
        public static Guid GetVolumeIdByMountPoint(string mountPoint)
        {
            if (!mountPoint.EndsWith('\\'))
            {
                mountPoint += '\\';
            }
            foreach (Guid volumeId in GetAvailableVolumeIds())
            {
                foreach (string currentMountPoint in GetMountPoints(volumeId))
                {
                    if (currentMountPoint.Equals(mountPoint))
                    {
                        return volumeId;
                    }
                }
            }
            throw new KeyNotFoundException($"No volume could be found which provides the volume accessible at {mountPoint}");
        }
        public static Func<ModelBindingContext, Task> GenericModelBinder(Func<string, object> parser, string targetTypeName, bool allowDefault)
        {
            return (ModelBindingContext bindingContext) =>
            {
                //see https://learn.microsoft.com/de-de/aspnet/core/mvc/advanced/custom-model-binding?view=aspnetcore-7.0
                ArgumentNullException.ThrowIfNull(bindingContext);
                string modelName = bindingContext.ModelName;
                ValueProviderResult values = bindingContext.ValueProvider.GetValue("moment");
                ValueProviderResult values2 = bindingContext.ValueProvider.GetValue("{moment}");
                ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
                if (valueProviderResult == ValueProviderResult.None)
                {
                    return Task.CompletedTask;
                }
                bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
                string value = valueProviderResult.FirstValue;
                if (string.IsNullOrEmpty(value))
                {
                    if (allowDefault)
                    {
                        bindingContext.Result = default;
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(modelName, $"No value given to parse to {targetTypeName}.");
                    }
                }
                else
                {
                    try
                    {
                        object model = parser(value);
                        bindingContext.Result = ModelBindingResult.Success(model);
                    }
                    catch
                    {
                        bindingContext.ModelState.TryAddModelError(modelName, $"\"{value}\" can not be parsed to {targetTypeName}.");
                    }
                }
                return Task.CompletedTask;
            };
        }

        public static T[] PadLeft<T>(T[] array, int length)
        {
            return PadLeft(array, default, length);
        }

        public static T[] PadLeft<T>(T[] array, T fillItem, int length)
        {
            return PadHelper(array, length, fillItem, true)!;
        }

        public static T?[] PadRight<T>(T[] array, int length)
        {
            return PadRight(array, default, length);
        }

        public static T?[] PadRight<T>(T[] array, T? fillItem, int length)
        {
            return PadHelper(array, length, fillItem, false);
        }

        private static T?[] PadHelper<T>(T[] array, int length, T? fillItem, bool PadLeft)
        {
            T[] result = array;
            while (array.Length <= length)
            {
                if (PadLeft)
                {
                    Concat([Equals(default(T), fillItem!) ? default(T) : fillItem], result);
                }
                else
                {
                    Concat(result, [fillItem]);
                }
            }
            return result;
        }
        /// <param name="value">
        /// must contain exacltly 4 bytes.
        /// </param>
        public static uint ByteArrayToUnsignedInteger32Bit(byte[] value, Endianness endianness = Endianness.BigEndian)
        {
            if (value.Length != 4)
            {
                throw new ArgumentException($"Length of parameter {nameof(value)} must be 4.");
            }
            if (endianness == Endianness.BigEndian)
            {
                return (((uint)value[0]) << 24)
                     + (((uint)value[1]) << 16)
                     + (((uint)value[2]) << 08)
                     + (((uint)value[3]) << 00);
            }
            if (endianness == Endianness.MixedEndian)
            {
                return (((uint)value[1]) << 24)
                     + (((uint)value[0]) << 16)
                     + (((uint)value[3]) << 08)
                     + (((uint)value[2]) << 00);
            }
            if (endianness == Endianness.LittleEndian)
            {
                return (((uint)value[4]) << 24)
                     + (((uint)value[3]) << 16)
                     + (((uint)value[2]) << 08)
                     + (((uint)value[1]) << 00);
            }
            throw new ArgumentException($"Unknown or unsupported value given for parameter {nameof(endianness)}");
        }
        /// <returns>
        /// Returns an array with exactly 4 bytes.
        /// </returns>
        public static byte[] UnsignedInteger32BitToByteArray(uint value, Endianness endianness = Endianness.BigEndian)
        {
            byte[] result = new byte[4];
            if (endianness == Endianness.BigEndian)
            {
                result[0] = (byte)((value & 0xff000000) >> 24);
                result[1] = (byte)((value & 0x00ff0000) >> 16);
                result[2] = (byte)((value & 0x0000ff00) >> 08);
                result[3] = (byte)((value & 0x000000ff) >> 00);
                return result;
            }
            if (endianness == Endianness.MixedEndian)
            {
                result[0] = (byte)((value & 0x00ff0000) >> 24);
                result[1] = (byte)((value & 0xff000000) >> 16);
                result[2] = (byte)((value & 0x000000ff) >> 08);
                result[3] = (byte)((value & 0x0000ff00) >> 00);
                return result;
            }
            if (endianness == Endianness.LittleEndian)
            {
                result[0] = (byte)((value & 0x000000ff) >> 24);
                result[1] = (byte)((value & 0x0000ff00) >> 16);
                result[2] = (byte)((value & 0x00ff0000) >> 08);
                result[3] = (byte)((value & 0xff000000) >> 00);
                return result;
            }
            throw new ArgumentException($"Unknown or unsupported value given for parameter {nameof(endianness)}");
        }
        /// <param name="value">
        /// must contain exacltly 8 bytes.
        /// </param>
        public static ulong ByteArrayToUnsignedInteger64Bit(byte[] value, Endianness endianness = Endianness.BigEndian)
        {
            if (value.Length != 8)
            {
                throw new ArgumentException($"Length of parameter {nameof(value)} must be 8.");
            }
            if (endianness == Endianness.BigEndian)
            {
                return (((ulong)value[0]) << 56)
                     + (((ulong)value[1]) << 48)
                     + (((ulong)value[2]) << 40)
                     + (((ulong)value[3]) << 32)
                     + (((ulong)value[4]) << 24)
                     + (((ulong)value[5]) << 16)
                     + (((ulong)value[6]) << 08)
                     + (((ulong)value[7]) << 00);
            }
            if (endianness == Endianness.MixedEndian)
            {
                return (((ulong)value[1]) << 56)
                     + (((ulong)value[0]) << 48)
                     + (((ulong)value[3]) << 40)
                     + (((ulong)value[2]) << 32)
                     + (((ulong)value[5]) << 24)
                     + (((ulong)value[4]) << 16)
                     + (((ulong)value[7]) << 08)
                     + (((ulong)value[6]) << 00);
            }
            if (endianness == Endianness.LittleEndian)
            {
                return (((ulong)value[7]) << 56)
                     + (((ulong)value[6]) << 48)
                     + (((ulong)value[5]) << 40)
                     + (((ulong)value[4]) << 32)
                     + (((ulong)value[3]) << 24)
                     + (((ulong)value[2]) << 16)
                     + (((ulong)value[1]) << 08)
                     + (((ulong)value[0]) << 00);
            }
            throw new ArgumentException($"Unknown or unsupported value given for parameter {nameof(endianness)}");
        }
        /// <returns>
        /// Returns an array with exactly 8 bytes.
        /// </returns>
        public static byte[] UnsignedInteger64BitToByteArray(ulong value, Endianness endianness = Endianness.BigEndian)
        {
            byte[] result = new byte[8];
            if (endianness == Endianness.BigEndian)
            {
                result[0] = (byte)((value & 0xff00000000000000) >> 56);
                result[1] = (byte)((value & 0x00ff000000000000) >> 48);
                result[2] = (byte)((value & 0x0000ff0000000000) >> 40);
                result[3] = (byte)((value & 0x000000ff00000000) >> 32);
                result[4] = (byte)((value & 0x00000000ff000000) >> 24);
                result[5] = (byte)((value & 0x0000000000ff0000) >> 16);
                result[6] = (byte)((value & 0x000000000000ff00) >> 08);
                result[7] = (byte)((value & 0x00000000000000ff) >> 00);
                return result;
            }
            if (endianness == Endianness.MixedEndian)
            {
                result[0] = (byte)((value & 0x00ff000000000000) >> 56);
                result[1] = (byte)((value & 0xff00000000000000) >> 48);
                result[2] = (byte)((value & 0x000000ff00000000) >> 40);
                result[3] = (byte)((value & 0x0000ff0000000000) >> 32);
                result[4] = (byte)((value & 0x0000000000ff0000) >> 24);
                result[5] = (byte)((value & 0x00000000ff000000) >> 16);
                result[6] = (byte)((value & 0x00000000000000ff) >> 08);
                result[7] = (byte)((value & 0x000000000000ff00) >> 00);
                return result;
            }
            if (endianness == Endianness.LittleEndian)
            {
                result[0] = (byte)((value & 0x00000000000000ff) >> 56);
                result[1] = (byte)((value & 0x000000000000ff00) >> 48);
                result[2] = (byte)((value & 0x0000000000ff0000) >> 40);
                result[3] = (byte)((value & 0x00000000ff000000) >> 32);
                result[4] = (byte)((value & 0x000000ff00000000) >> 24);
                result[5] = (byte)((value & 0x0000ff0000000000) >> 16);
                result[6] = (byte)((value & 0x00ff000000000000) >> 08);
                result[7] = (byte)((value & 0xff00000000000000) >> 00);
                return result;
            }
            throw new ArgumentException($"Unknown or unsupported value given for parameter {nameof(endianness)}");
        }
        public enum Endianness
        {
            BigEndian = 0,
            MixedEndian = 1,
            LittleEndian = 2,
        }
        public static string NullSafeToString(object @object)
        {
            if (@object == null)
            {
                return "null";
            }
            else
            {
                return @object.ToString();
            }
        }

        #region Nullsafe-equals-helper
        public static bool NullSafeEquals(this object @this, object obj)
        {
            return NullSafeHelper(@this, obj, (obj1, obj2) => obj1.Equals(obj2));
        }

        public static bool NullSafeSetEquals<T>(this ISet<T> @this, ISet<T> obj, bool treatEmptyAsNull = false)
        {
            return NullSafeHelper(TreatNullHelper(@this, treatEmptyAsNull), TreatNullHelper(obj, treatEmptyAsNull), (obj1, obj2) => obj1.ToHashSet().SetEquals(obj2));
        }

        public static bool NullSafeListEquals<T>(this IList<T> @this, IList<T> obj, bool treatEmptyAsNull = false)
        {
            return NullSafeHelper(TreatNullHelper(@this, treatEmptyAsNull), TreatNullHelper(obj, treatEmptyAsNull), (obj1, obj2) => obj1.SequenceEqual(obj2));
        }

        public static bool NullSafeEnumerableEquals<T>(this IEnumerable<T> @this, IEnumerable<T> obj, bool treatEmptyAsNull = false)
        {
            @this = TreatNullHelper(@this, treatEmptyAsNull);
            obj = TreatNullHelper(obj, treatEmptyAsNull);
            return NullSafeHelper(@this, obj, (obj1, obj2) =>
            {
                if (obj1.Count() != obj2.Count())
                {
                    return false;
                }
                List<T> obj1Copy = new(obj1);
                List<T> obj2Copy = new(obj2);
                for (int i = 0; i < obj1.Count(); i++)
                {
                    if (!RemoveItemOnlyOnce(obj2Copy, obj1Copy[i]))
                    {
                        return false;
                    }
                }
                return true;
            });
        }
        private static IEnumerable<T> TreatNullHelper<T>(IEnumerable<T> items, bool treatEmptyAsNull)
        {
            if (treatEmptyAsNull && items != null)
            {
                if (items.Any())
                {
                    return items;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return items;
            }
        }
        private static bool NullSafeHelper<T>(T object1, T object2, Func<T, T, bool> f)
        {
            bool thisIsNull = object1 == null;
            bool objIsNull = object2 == null;
            if (thisIsNull ^ objIsNull)
            {
                return false;
            }
            else
            {
                if (thisIsNull && objIsNull)
                {
                    return true;
                }
                else
                {
                    return f(object1, object2);
                }
            }
        }
        public static bool RemoveItemOnlyOnce<T>(this IList<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index > -1)
            {
                list.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public static DateTimeOffset GetTimeFromInternetUtC()
        {
            return GetTimeFromInternet(TimeZoneInfo.Utc);
        }

        public static DateTimeOffset GetTimeFromInternetCurrentTimeZone()
        {
            return GetTimeFromInternet(TimeZoneInfo.Local);
        }

        public static DateTimeOffset GetTimeFromInternet(TimeZoneInfo timezone)
        {
            return GetTimeFromInternet(timezone, "yy-MM-dd HH:mm:ss", "time.nist.gov", 13, 7, 17);
        }

        public static DateTimeOffset GetTimeFromInternet(TimeZoneInfo timezone, string format, string domain, int port, int begin, int length)
        {
            using TcpClient tcpClient = new TcpClient(domain, port);
            using StreamReader streamReader = new(tcpClient.GetStream());
            DateTimeOffset originalDateTime = DateTime.ParseExact(streamReader.ReadToEnd().Substring(begin, length), format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            return TimeZoneInfo.ConvertTime(originalDateTime, timezone);
        }
        /// <returns>
        /// If Development-configuration: This function returns <see cref="DateTime.Now"/> using the timezone of the current machine.
        /// Any else configuration: This function returns <see cref="DateTime.UtcNow"/> which is more appropriate for productive usage.
        /// </returns>
        public static DateTimeOffset GetNow() =>
#if Development
            DateTimeOffset.Now;
#else
            DateTimeOffset.UtcNow;
#endif

        public static string DateTimeToString(DateTime value)
        {
            return value.ToString("O");
        }

        public static DateTime DateTimeParse(string value)
        {
            return DateTime.ParseExact(value, "O", CultureInfo.InvariantCulture);
        }

        public static byte[] StreamToByteArray(Stream input)
        {
            using MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
        public static bool ByteArrayEquals(ReadOnlySpan<byte> array1, ReadOnlySpan<byte> array2)
        {
            return array1.SequenceEqual(array2);
        }

        public static SerializableDictionary<TKey, TValue> ToSerializableDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            SerializableDictionary<TKey, TValue> result = [];
            foreach (System.Collections.Generic.KeyValuePair<TKey, TValue> kvp in dictionary)
            {
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }

        public static T WaitAndGetResult<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
        public static bool IsDefault(object @object)
        {
            if (@object == null)
            {
                return true;
            }
            else
            {
                return EqualityComparer<object>.Default.Equals(@object, GetDefault(@object.GetType()));
            }
        }
        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }
        }
        public static Tuple<string, string, string> ResolvePathOfProgram(string program, string argument, string workingDirectory)
        {
            // adapt working directory if required
            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }

            // resolve program
            if (HasPath(program))
            {
                if (IsRelativeLocalFilePath(program))
                {
                    program = ResolveToFullPath(program, workingDirectory);
                }
            }
            else
            {
                string cwdWithProgram = Path.Combine(workingDirectory, program);
                if (File.Exists(cwdWithProgram))
                {
                    program = cwdWithProgram;
                }
                else
                {
                    if (TryResolvePathByPathVariable(program, out string programWithFullPath))
                    {
                        program = programWithFullPath;
                    }
                    else
                    {
                        throw new ArgumentException($"Program '{program}' can not be found");
                    }
                }
            }

            // check program
            if (File.Exists(program))
            {
                if (SpecialFileInformation.FileIsExecutable(program))
                {
                    // nothing to do
                }
                else
                {
                    // adapt argument
                    if (OperatingSystem.OperatingSystem.GetCurrentOperatingSystem() is GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows)
                    {
                        argument = $"\"{program}\" {argument}";
                        program = SpecialFileInformation.GetDefaultProgramToOpenFile(Path.GetExtension(program));
                    }
                    else
                    {
                        throw new ArgumentException($"Program '{program}' is not executable");
                    }
                }
            }
            else
            {
                throw new FileNotFoundException($"Program '{program}' does not exist");
            }
            return new Tuple<string, string, string>(program, argument, workingDirectory);
        }
        private static bool HasPath(string str)
        {
            return str.Contains('/') || str.Contains('\\');
        }

        public static string GetAssertionFailMessage(object expectedObject, object actualObject, int maxLengthPerObject = 1000)
        {
            return $"Equal failed. Expected: <{Environment.NewLine}{Generic.GenericToString(expectedObject, maxLengthPerObject)}{Environment.NewLine}> Actual: <{Environment.NewLine}{Generic.GenericToString(actualObject, maxLengthPerObject)}{Environment.NewLine}>";
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }
        public static void ForEach(this IEnumerable source, Action<object> action)
        {
            foreach (object item in source)
            {
                action(item);
            }
        }

        public static bool ImprovedReferenceEquals(object item1, object item2)
        {
            bool itemHasValueType = HasValueType(item1);
            if (itemHasValueType != HasValueType(item2))
            {
                return false;
            }
            bool item1IsDefault = IsDefault(item1);
            bool item2IsDefault = IsDefault(item2);
            if (item1IsDefault && item2IsDefault)
            {
                return true;
            }
            if (item1IsDefault && !item2IsDefault)
            {
                return false;
            }
            if (!item1IsDefault && item2IsDefault)
            {
                return false;
            }
            if (!itemHasValueType)
            {
                return ReferenceEquals(item1, item2);
            }
            Type type = item1.GetType();
            if (!TypeComparerIgnoringGenerics.Equals(type, item2.GetType()))//TODO ignore generics here when type is keyvaluepair
            {
                return false;
            }
            if (EnumerableTools.TypeIsKeyValuePair(type))
            {
                System.Collections.Generic.KeyValuePair<object, object> kvp1 = EnumerableTools.ObjectToKeyValuePairUnsafe<object, object>(item1);
                System.Collections.Generic.KeyValuePair<object, object> kvp2 = EnumerableTools.ObjectToKeyValuePairUnsafe<object, object>(item2);
                return ImprovedReferenceEquals(kvp1.Key, kvp2.Key) && ImprovedReferenceEquals(kvp1.Value, kvp2.Value);
            }
            else
            {
                return item1.Equals(item2);
            }
        }

        private static (string, string) GetBaseTypeInformation(Type type)
        {
            string name = type.Name;
            if (name.Contains('`'))
            {
                name = name.Split('`')[0];
            }
            return (name, type.Namespace);
        }

        public static bool HasValueType(object @object)
        {
            if (@object == null)
            {
                return false;
            }
            else
            {
                return @object.GetType().IsValueType;
            }
        }

        public static string GetNameOfCurrentExecutable()
        {
            return Process.GetCurrentProcess().ProcessName;
        }

        public static bool IsNegative(this TimeSpan timeSpan)
        {
            return timeSpan.Ticks < 0;
        }

        public static bool IsPositive(this TimeSpan timeSpan)
        {
            return timeSpan.Ticks > 0;
        }

        public static string ToOnlyFirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            if (input.Length == 1)
            {
                return input.ToUpper();
            }
            return input.First().ToString().ToUpper() + input[1..].ToLower();
        }
        private static readonly char[] Whitespace = [' '];
        private static readonly char[] WhitespaceAndPartialWordIndicators = [' ', '_', '-'];
        public static string ToOnlyFirstCharOfEveryWordToUpper(this string input)
        {
            return ToOnlyFirstCharOfEveryWordToUpper(input, (lastCharacter) => Whitespace.Contains(lastCharacter));
        }

        public static string ToOnlyFirstCharOfEveryWordOrPartialWordToUpper(this string input)
        {
            return ToOnlyFirstCharOfEveryWordToUpper(input, (lastCharacter) => WhitespaceAndPartialWordIndicators.Contains(lastCharacter));
        }

        public static string ToOnlyFirstCharOfEveryNewLetterSequenceToUpper(this string input)
        {
            return ToOnlyFirstCharOfEveryWordToUpper(input, (lastCharacter) => !char.IsLetter(lastCharacter));
        }

        public static string ToOnlyFirstCharOfEveryWordToUpper(this string input, Func<char, bool> printCharUppercaseDependentOnPreviousChar)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            char[] splitted = input.ToCharArray();
            char lastChar = default;
            for (int i = 0; i < splitted.Length; i++)
            {
                if (i == 0)
                {
                    splitted[i] = splitted[i].ToString().ToUpper().First();
                }
                if (i > 0)
                {
                    if (printCharUppercaseDependentOnPreviousChar(lastChar))
                    {
                        splitted[i] = splitted[i].ToString().ToUpper().First();
                    }
                    else
                    {
                        splitted[i] = splitted[i].ToString().ToLower().First();
                    }
                }
                lastChar = splitted[i];
            }
            return new string(splitted);
        }
        public static bool IsAllUpper(this string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetter(input[i]) && !char.IsUpper(input[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsAllLower(this string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetter(input[i]) && !char.IsLower(input[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private static readonly char[] ToPascalCaseSeparator = ['-', '_'];
        public static string ToPascalCase(this string input)
        {
            IEnumerable<string> words = input
                .Split(ToPascalCaseSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word[..1].ToUpper() + word[1..].ToLower());

            return string.Concat(words);
        }
        public static string ToCamelCase(this string input)
        {
            string pascalCase = input.ToPascalCase();
            return char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
        }

        private static readonly Regex _OneOrMoreHexSigns = OneOrMoreHexSigns();
        public static bool IsHexString(string result)
        {
            return _OneOrMoreHexSigns.IsMatch(result.ToLower());
        }

        public static bool IsHexDigit(this char @char)
        {
            return @char is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');
        }

        public static bool DarkModeEnabled
        {
            get => OperatingSystem.OperatingSystem.GetCurrentOperatingSystem().Accept(new GetDarkModeEnabledVisitor());
            set => OperatingSystem.OperatingSystem.GetCurrentOperatingSystem().Accept(new SetDarkModeEnabledVisitor(value));
        }
        public static (IObservable<T>, Action) FuncToObservable<T>(Func<T> valueFunction, TimeSpan updateInterval)
        {
            Subject<T> subject = new();
            bool enabled = true;
            SupervisedThread thread = SupervisedThread.Create(() =>
            {
                while (enabled)
                {
                    try
                    {
                        Thread.Sleep(updateInterval);
                        if (subject.HasObservers)
                        {
                            subject.OnNext(valueFunction());
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
                subject.OnCompleted();
                subject.Dispose();
            });
            thread.Start();
            return (subject.AsObservable().DistinctUntilChanged(), () => enabled = false);
        }
        public static Encoding GetEncodingByIdentifier(string encodingIdentifier)
        {
            if (encodingIdentifier == "utf-8")
            {
                return new UTF8Encoding(false);
            }
            if (encodingIdentifier == "utf-8-bom")
            {
                return new UTF8Encoding(true);
            }
            if (encodingIdentifier == "utf-16")
            {
                return new UnicodeEncoding(false, false);
            }
            if (encodingIdentifier == "utf-16-bom")
            {
                return new UnicodeEncoding(false, true);
            }
            if (encodingIdentifier == "utf-16-be")
            {
                return new UnicodeEncoding(true, false);
            }
            if (encodingIdentifier == "utf-16-be-bom")
            {
                return new UnicodeEncoding(true, true);
            }
            if (encodingIdentifier == "utf-32")
            {
                return new UTF32Encoding(false, false);
            }
            if (encodingIdentifier == "utf-32-bom")
            {
                return new UTF32Encoding(false, true);
            }
            if (encodingIdentifier == "utf-32-be")
            {
                return new UTF32Encoding(true, false);
            }
            if (encodingIdentifier == "utf-32-be-bom")
            {
                return new UTF32Encoding(true, true);
            }
            return Encoding.GetEncoding(encodingIdentifier);
        }

        private class SetDarkModeEnabledVisitor : IOperatingSystemVisitor
        {
            private readonly bool _Enabled;

            public SetDarkModeEnabledVisitor(bool enabled)
            {
                this._Enabled = enabled;
            }

            public void Handle(OSX operatingSystem)
            {
                throw new NotImplementedException();
            }

            public void Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", true);

                    key.SetValue("AppsUseLightTheme", this._Enabled ? 0 : 1);
                    key.SetValue("SystemUsesLightTheme", this._Enabled ? 0 : 1);
                    bool actual = DarkModeEnabled;
                }
            }

            public void Handle(Linux operatingSystem)
            {
                throw new NotImplementedException();
            }
        }
        private class GetDarkModeEnabledVisitor : IOperatingSystemVisitor<bool>
        {
            public bool Handle(OSX operatingSystem)
            {
                throw new NotSupportedException();
            }

            public bool Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                        bool AppsUseDarkTheme = ((int)key.GetValue("AppsUseLightTheme")) == 0;
                        bool SystemUsesDarkTheme = ((int)key.GetValue("SystemUsesLightTheme")) == 0;
                        return AppsUseDarkTheme && SystemUsesDarkTheme;
                    }
                    catch
                    {
                        throw;
                    }
                }
                return false;
            }

            public bool Handle(Linux operatingSystem)
            {
                throw new NotSupportedException();
            }
        }
        public static NullReferenceException CreateNullReferenceExceptionDueToParameter(string parameterName)
        {
            return new NullReferenceException($"Parameter {parameterName} is null");
        }

        public static T CreateOrLoadJSONConfigurationFile<T, TBase>(string configurationFile, T initialValue, out bool fileWasCreatedNew) where T : TBase, new()
        {
            return CreateOrLoadConfigurationFile<T, TBase>(configurationFile, initialValue,
                (configurationFile, initialValue) =>
                {
                    dynamic expando = new ExpandoObject();
                    ((IDictionary<string, object>)expando)[typeof(T).Name] = initialValue;
                    string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(expando, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(configurationFile, serialized, new UTF8Encoding(false));
                }, (configurationFile) =>
                {
                    IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddJsonFile(configurationFile).Build();
                    return configurationRoot.GetRequiredSection(typeof(T).Name).Get<T>();
                }, out fileWasCreatedNew);
        }
#pragma warning disable IDE0060 //not used parameter "knownTypes"
        public static T CreateOrLoadXMLConfigurationFile<T, TBase>(string configurationFile, T initialValue, ISet<Type> knownTypes, out bool fileWasCreatedNew) where T : TBase, new()
#pragma warning restore IDE0060 
        {
            //TODO use knownTypes
            SimpleObjectPersistence<T> simpleObjectPersistence = new SimpleObjectPersistence<T>();
            //TODO simpleObjectPersistence.Serializer.KnownTypes.UnionWith(knownTypes);
            return CreateOrLoadConfigurationFile<T, TBase>(configurationFile, initialValue,
                (configurationFile, initialValue) =>
                {
                    simpleObjectPersistence.File = configurationFile;
                    simpleObjectPersistence.Object = initialValue;
                    simpleObjectPersistence.SaveObjectToFile();
                }, (configurationFile) =>
                {
                    simpleObjectPersistence.File = configurationFile;
                    simpleObjectPersistence.LoadObjectFromFile();
                    return simpleObjectPersistence.Object;
                }, out fileWasCreatedNew);
        }
        /// <summary>
        /// This function loads a configuration from disk if possible and if not then the initial configuration will be saved to disk and returned.
        /// </summary>
        public static T CreateOrLoadConfigurationFile<T, TBase>(string configurationFile, T initialValue, Action<string, T> createInitialFile, Func<string, T> loadExistingFile, out bool fileWasCreatedNew) where T : TBase, new()
        {
            T configuration;
            if (File.Exists(configurationFile))
            {
                configuration = loadExistingFile(configurationFile);
                fileWasCreatedNew = false;
            }
            else
            {
                configurationFile = ResolveToFullPath(configurationFile);
                EnsureDirectoryExists(Path.GetDirectoryName(configurationFile));
                configuration = initialValue;
                createInitialFile(configurationFile, initialValue);
                fileWasCreatedNew = true;
            }
            return configuration;
        }
        public static ExecutionMode GetExecutionMode(string[] commandlineArguments)
        {
            if (Assembly.GetEntryAssembly().GetName().Name == "dotnet-swagger")
            {
                return Analysis.Instance;
            }
            if (commandlineArguments.Contains("--TestRun"))
            {
                return TestRun.Instance;
            }
            // RealRun is opt-in (production needs explicit "--RealRun true"); anything else
            // falls back to TestRun. This keeps integration-test harnesses from accidentally
            // booting the HTTPS-with-cert production path.
            for (int i = 0; i < commandlineArguments.Length - 1; i++)
            {
                if (commandlineArguments[i] == "--RealRun" && string.Equals(commandlineArguments[i + 1], "true", System.StringComparison.OrdinalIgnoreCase))
                {
                    return RunProgram.Instance;
                }
            }
            return TestRun.Instance;
        }

        public static string DecimalToString(decimal amount)
        {
            string result = amount.ToString(CultureInfo.InvariantCulture);
            if (result.Contains('.'))
            {
                while (result.EndsWith('0'))
                {
                    result = result[..^1];
                }
                if (result.EndsWith('.'))
                {
                    result = result + "0";
                }
            }
            else
            {
                result = result + ".0";
            }
            return result;
        }

        public static bool ContainsDuplicates<T>(IEnumerable<T> value)
        {
            int count = value.Count();
            int hashSetCount = value.ToHashSet().Count;
            return count != hashSetCount;
        }
        #region Format exception
        private const string _LogIndentation = "    ";
        public static string GetExceptionMessage(Exception exception, string message = null, bool wrapInOneLine = false, uint indentationLevel = 1, string exceptionTitle = "Exception-information")
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "An exception occurred.";
            }
            if (!(message.EndsWith('.') | message.EndsWith('?') | message.EndsWith(':') | message.EndsWith('!')))
            {
                message += ".";
            }
            string result = $"{exceptionTitle}: ";
            if (exception == null)
            {
                result += "null";
            }
            else
            {
                result += $"'{message}', Exception-type: {exception.GetType().FullName}, Exception-message: '{exception.Message}'";
                if (true)
                {
                    result += @$"
(Exception-details:
{Indent(FormatStackTrace(exception), indentationLevel)},
{Indent(FormatStackInnerException(exception, indentationLevel), indentationLevel)}
)";
                }
            }
            if (wrapInOneLine)
            {
                result = result
                    .Replace("\r", " ")
                    .Replace("\n", " ")
                    .Replace("  ", " ");
            }
            result = result.Trim();
            return result;
        }
        private static string Indent(IList<string> lines, uint indentationLevel)
        {
            string fullIndentation = string.Concat(Enumerable.Repeat(_LogIndentation, (int)indentationLevel));
            return string.Join(Environment.NewLine, lines.Select(line => fullIndentation + line));
        }

        private static IList<string> FormatStackTrace(Exception exception)
        {
            List<string> result = [];
            if (exception.StackTrace == null)
            {
                result.Add("Stack-trace: null");
            }
            else
            {
                result.Add("Stack-trace:");
                result.AddRange(SplitOnNewLineCharacter(exception.StackTrace));
            }
            return result;
        }

        private static IList<string> FormatStackInnerException(Exception exception, uint indentationLevel, bool wrapInOneLine = false)
        {
            return [.. SplitOnNewLineCharacter(GetExceptionMessage(exception.InnerException, null, wrapInOneLine, indentationLevel + 1, "Inner exception"))];
        }
        #endregion

        public static bool CheckCancellationToken(IList<string> messages, CancellationToken cancellationToken, out (HealthStatus, IList<string>) result)
        {
            CheckCancellationToken(messages, cancellationToken, out (bool, IList<string>) resultInternal);
            result = (resultInternal.Item1 ? HealthStatus.Healthy : HealthStatus.Unhealthy, resultInternal.Item2);
            return resultInternal.Item1;
        }
        /// <param name="result">
        /// Will contain (false, messages + "Task was cancelled") if the task was cancelled.
        /// Otherwise the value will be set to null.
        /// </param>
        /// <returns>
        /// Returns true if and only if the task was cancelled by the cancellationtoken. 
        /// </returns>
        public static bool CheckCancellationToken(IList<string> messages, CancellationToken cancellationToken, out (bool, IList<string>) result)
        {
            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                result = (false, messages.Concat(new List<string>() { "Task was cancelled" }).ToList());
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
        private static readonly Random _Random = new Random();

        public static T RandomChoice<T>(params T[] choices)
        {
            return choices[_Random.Next(choices.Count())];
        }

        public static string HTMLUnescape(string @value)
        {
            string result = WebUtility.HtmlEncode(@value);
            result = result.Replace("\n", "<br />");
            return result;
        }

        internal static bool HasDangerousCharacters(string @value)
        {
            if (@value.Where(char.IsControl).Any())
            {
                return true;
            }
            //add more characters if desired
            return false;
        }

        internal static bool IsValidHTML(string @value)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(@value);
            return !htmlDoc.ParseErrors.Any();
        }

        internal static void IgnoreExceptions(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                NoOperation();
            }
        }

        public static bool RunningInContainer
        {
            get
            {
                return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            }
        }
        public static string GetRandomHexCharacter(int digits, IRandomnessProvider random)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }
        public static string GetRandomAlphaHexCharacter(IRandomnessProvider random)
        {
            int number = random.Next(7);
            string result = (10 + number).ToString("X1");
            return result;
        }
        public static T GetValue<T>(T? value)
        {
            return GetValue(value, null);
        }
        public static T GetValue<T>(T? value, string? information)
        {
            if (value == null)
            {
                string message = "Variable does not have a value.";
                if (!string.IsNullOrWhiteSpace(information))
                {
                    message += $" {information}";
                }
                throw new NullReferenceException(message);
            }
            else
            {
                return value!;
            }
        }

        public static bool ContainerIsHealthy(string containerNameToWaitToBeHealthy)
        {
            try
            {
                using ExternalProgramExecutor externalProgramExecutor2 = new ExternalProgramExecutor("docker", $"inspect  {containerNameToWaitToBeHealthy}");
                externalProgramExecutor2.Run();
                string output = String.Join(string.Empty, externalProgramExecutor2.AllStdOutLines);
                using JsonDocument doc = JsonDocument.Parse(output);
                JsonElement root = doc.RootElement[0];
                if (root.TryGetProperty("State", out JsonElement state) &&
                    state.TryGetProperty("Health", out JsonElement health) &&
                    health.TryGetProperty("Status", out JsonElement status))
                {
                    string? healthy = status.GetString();
                    return healthy == "healthy";
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public static void RunEspoc(string processesFile, string? logFile)//TODO extract to grylib
        {
            int currentProcessId = Environment.ProcessId;
            Process process = new Process();
            process.StartInfo.FileName = "scespoc";
            string arguments = $"--processid {currentProcessId} --file \"{processesFile}\"";
            if (logFile != null)
            {
                arguments = arguments + $" --logfile \"{logFile}\"";
            }
            process.StartInfo.Arguments = arguments;
            GRYLibrary.Core.Misc.Utilities.EnsureFileExists(processesFile);
            GRYLibrary.Core.Misc.Utilities.AssertCondition(process.Start());
            Thread.Sleep(TimeSpan.FromSeconds(1));
            GRYLibrary.Core.Misc.Utilities.AssertCondition(!process.HasExited, "Watchdog exited unexpectedly.");
        }

    }
}