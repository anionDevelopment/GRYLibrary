using System;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Core.Misc
{
    public class TableGenerator
    {
        private TableGenerator() { }
        public static string[] Generate<T>(T[,] array, TableOutputType outputType, Func<T, string> toString, object[] headlines = null)
        {
            string[,] convertedArray = new string[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    convertedArray[i, j] = toString(array[i, j]);
                }
            }
            return Generate(convertedArray, outputType, headlines);
        }
        public static string[] Generate(string[,] array, TableOutputType outputType, object[] headlines = null)
        {
            if (headlines != null)
            {
                string[,] newArray = new string[array.GetLength(0) + 1, array.GetLength(1)];
                for (int i = 0; i < headlines.Length; i++)
                {
                    newArray[0, i] = headlines[i].ToString();
                }
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        newArray[i + 1, j] = array[i, j];
                    }
                }
                array = newArray;
                outputType.TableHasTitles = true;
            }
            return outputType.Accept(new TableOutputTypeVisitor(array));
        }
        private class TableOutputTypeVisitor : ITableOutputTypeVisitor<string[]>
        {
            public string[,] Array { get; set; }

            public TableOutputTypeVisitor(string[,] array)
            {
                this.Array = array;
            }

            public string[] Handle(ASCIITable tableOutputType)
            {
                List<string> result = [];
                if (!string.IsNullOrEmpty(tableOutputType.Title))
                {
                    result.Add(tableOutputType.Title);
                }
                int[] columnLengths = this.GetColumnLengths(this.Array, tableOutputType.MaximalWidth);
                result.Add(this.GetFirstLineForASCIITable(tableOutputType, columnLengths));
                result.Add(this.GetHeadlineLineForASCIITable(tableOutputType, columnLengths));
                if (tableOutputType.TableHasTitles)
                {
                    result.Add(this.GetHeadlineDividerLineForASCIITable(tableOutputType, columnLengths));
                }
                for (int lineNumber = 1; lineNumber < this.Array.GetLength(0); lineNumber++)
                {
                    result.Add(this.GetLineForASCIITable(tableOutputType, columnLengths, lineNumber));
                }
                result.Add(this.GetLastLineForASCIITable(tableOutputType, columnLengths));
                return [.. result];
            }

            public string[] Handle(HTMLTable tableOutputType)
            {
                throw new NotImplementedException();
            }

            public string[] Handle(CSV csv)
            {
                List<string> result = [];
                for (int i = 0; i < this.Array.GetLength(0); i++)
                {
                    List<string> lineItems = [];
                    for (int j = 0; j < this.Array.GetLength(1); j++)
                    {
                        lineItems.Add(this.Array[i, j]);
                    }
                    result.Add(string.Join(";", lineItems));
                }
                return [.. result];
            }
            #region Helper
            private string GetLineForASCIITable(ASCIITable tableOutputType, int[] columnLengths, int lineNumber)
            {
                string[] content = new string[columnLengths.Length];
                for (int column = 0; column < this.Array.GetLength(1); column++)
                {
                    content[column] = this.Array[lineNumber, column];
                }
                return this.GetLine(tableOutputType.Characters.VerticalLineCharacter, content, ' ', tableOutputType.Characters.VerticalLineCharacter, tableOutputType.Characters.VerticalLineCharacter, columnLengths);
            }

            private string GetHeadlineDividerLineForASCIITable(ASCIITable tableOutputType, int[] columnLengths)
            {
                return this.GetLine(tableOutputType.Characters.TRightCharacter, this.NTimes(tableOutputType.Characters.HorizontalLineCharacter.ToString(), columnLengths.Length), tableOutputType.Characters.HorizontalLineCharacter, tableOutputType.Characters.CrossCharacter, tableOutputType.Characters.TLeftCharacter, columnLengths);
            }

            private string GetLastLineForASCIITable(ASCIITable tableOutputType, int[] columnLengths)
            {
                return this.GetLine(tableOutputType.Characters.LeftLowerCornerCharacter, this.NTimes(tableOutputType.Characters.HorizontalLineCharacter.ToString(), columnLengths.Length), tableOutputType.Characters.HorizontalLineCharacter, tableOutputType.Characters.TUpCharacter, tableOutputType.Characters.RightLowerCornerCharacter, columnLengths);
            }

            private string GetHeadlineLineForASCIITable(ASCIITable tableOutputType, int[] columnLengths)
            {
                return this.GetLineForASCIITable(tableOutputType, columnLengths, 0);
            }

            private string GetFirstLineForASCIITable(ASCIITable tableOutputType, int[] columnLengths)
            {
                return this.GetLine(tableOutputType.Characters.LeftUpperCornerCharacter, this.NTimes(tableOutputType.Characters.HorizontalLineCharacter.ToString(), columnLengths.Length), tableOutputType.Characters.HorizontalLineCharacter, tableOutputType.Characters.TDownCharacter, tableOutputType.Characters.RightUpperCornerCharacter, columnLengths);
            }

            private string GetLine(char firstChar, string[] content, char fillCharForContent, char separator, char lastChar, int[] columnLengths)
            {
                return $"{firstChar}{this.GetContentOfLine(content, fillCharForContent, separator, columnLengths)}{lastChar}";
            }

            private string GetContentOfLine(string[] content, char fillCharForContent, char separator, int[] columnLengths)
            {
                for (int i = 0; i < content.Length; i++)
                {
                    content[i] = this.AdjustLength(content[i], columnLengths[i]).PadRight(columnLengths[i], fillCharForContent);
                }
                return string.Join(separator.ToString(), content);
            }

            private string AdjustLength(string value, int maximalLength)
            {
                int valueLength = value.Length;
                if (valueLength <= maximalLength)
                {
                    return value;
                }
                else
                {
                    int minimumLengthForCut = 8;
                    if (maximalLength < minimumLengthForCut)
                    {
                        return value[..maximalLength];
                    }
                    else
                    {
                        return value[..(maximalLength - 3)] + "...";
                    }
                }
            }

            private T[] NTimes<T>(T value, int amount)
            {
                return [.. Enumerable.Repeat(value, amount)];
            }

            private int[] GetColumnLengths(string[,] array, int maximalWidth)
            {
                int[] result = this.NTimes(0, array.GetLength(1));
                for (int line = 0; line < array.GetLength(0); line++)
                {
                    for (int column = 0; column < array.GetLength(1); column++)
                    {
                        string currentCellValue = array[line, column];
                        int currentCellValueLength = currentCellValue.Length;
                        if (result[column] == 0 || result[column] < currentCellValueLength)
                        {
                            result[column] = currentCellValueLength;
                        }
                        if (result[column] > maximalWidth)
                        {
                            result[column] = maximalWidth;
                        }
                    }
                }
                return result;
            }


            #endregion
        }
        public interface ITableCharacter
        {
            char HorizontalLineCharacter { get; }
            char VerticalLineCharacter { get; }
            char LeftUpperCornerCharacter { get; }
            char RightUpperCornerCharacter { get; }
            char LeftLowerCornerCharacter { get; }
            char RightLowerCornerCharacter { get; }
            char CrossCharacter { get; }
            char TDownCharacter { get; }
            char TRightCharacter { get; }
            char TLeftCharacter { get; }
            char TUpCharacter { get; }
        }
        public class OneLineTableCharacter : ITableCharacter
        {
            public char HorizontalLineCharacter => '─';
            public char VerticalLineCharacter => '│';
            public char LeftUpperCornerCharacter => '┌';
            public char RightUpperCornerCharacter => '┐';
            public char LeftLowerCornerCharacter => '└';
            public char RightLowerCornerCharacter => '┘';
            public char CrossCharacter => '┼';
            public char TDownCharacter => '┬';
            public char TRightCharacter => '├';
            public char TLeftCharacter => '┤';
            public char TUpCharacter => '┴';
        }
        public class DoubleLineTableCharacter : ITableCharacter
        {
            public char HorizontalLineCharacter => '═';
            public char VerticalLineCharacter => '║';
            public char LeftUpperCornerCharacter => '╔';
            public char RightUpperCornerCharacter => '╗';
            public char LeftLowerCornerCharacter => '╚';
            public char RightLowerCornerCharacter => '╝';
            public char CrossCharacter => '╬';
            public char TDownCharacter => '╦';
            public char TRightCharacter => '╠';
            public char TLeftCharacter => '╣';
            public char TUpCharacter => '╩';
        }
        public class RoundLineTableCharacter : ITableCharacter
        {
            public char HorizontalLineCharacter => '─';
            public char VerticalLineCharacter => '│';
            public char LeftUpperCornerCharacter => '╭';
            public char RightUpperCornerCharacter => '╮';
            public char LeftLowerCornerCharacter => '╰';
            public char RightLowerCornerCharacter => '╯';
            public char CrossCharacter => '┼';
            public char TDownCharacter => '┬';
            public char TRightCharacter => '├';
            public char TLeftCharacter => '┤';
            public char TUpCharacter => '┴';
        }
        public abstract class TableOutputType
        {
            public bool TableHasTitles { get; set; } = false;
            public abstract void Accept(ITableOutputTypeVisitor visitor);
            public abstract T Accept<T>(ITableOutputTypeVisitor<T> visitor);

        }
        public interface ITableOutputTypeVisitor
        {
            void Handle(ASCIITable tableOutputType);
            void Handle(CSV tableOutputType);
            void Handle(HTMLTable tableOutputType);
        }

        public interface ITableOutputTypeVisitor<T>
        {
            T Handle(ASCIITable tableOutputType);
            T Handle(CSV tableOutputType);
            T Handle(HTMLTable tableOutputType);
        }
        public sealed class ASCIITable : TableOutputType
        {
            public ITableCharacter Characters { get; set; } = new OneLineTableCharacter();

            public int MaximalWidth { get; set; } = 1000;
            public string Title { get; set; } = string.Empty;
            public override void Accept(ITableOutputTypeVisitor visitor)
            {
                visitor.Handle(this);
            }

            public override T Accept<T>(ITableOutputTypeVisitor<T> visitor)
            {
                return visitor.Handle(this);
            }
        }
        public sealed class HTMLTable : TableOutputType
        {
            public override void Accept(ITableOutputTypeVisitor visitor)
            {
                visitor.Handle(this);
            }

            public override T Accept<T>(ITableOutputTypeVisitor<T> visitor)
            {
                return visitor.Handle(this);
            }
        }
        public sealed class CSV : TableOutputType
        {
            public override void Accept(ITableOutputTypeVisitor visitor)
            {
                visitor.Handle(this);
            }

            public override T Accept<T>(ITableOutputTypeVisitor<T> visitor)
            {
                return visitor.Handle(this);
            }
        }
    }
}