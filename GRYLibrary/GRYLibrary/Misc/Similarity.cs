using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Misc
{
    public static class Similarity
    {
        public static PercentValue CalculateCombinedSimilarity(string string1, string string2)
        {
            if (string1 == null || string2 == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                if (string1.Equals(string2))
                {
                    return PercentValue.HundredPercent;
                }
                else
                {
                    byte amountOfAlgorithms = 3;
                    decimal factor = 1 / (decimal)amountOfAlgorithms;
                    return new PercentValue(
                        (CalculateCosineSimilarity(string1, string2).Value * factor)
                      + (CalculateJaccardSimilarity(string1, string2).Value * factor)
                      + (CalculateLevenshteinSimilarity(string1, string2).Value * factor)
                    );
                }
            }
        }
        public static int CalculateLevenshteinDistance(string string1, string string2)
        {
            if (string.IsNullOrEmpty(string1) && string.IsNullOrEmpty(string2))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(string1))
            {
                return string2.Length;
            }
            if (string.IsNullOrEmpty(string2))
            {
                return string1.Length;
            }
            int lengthA = string1.Length;
            int lengthB = string2.Length;
            int[,] distance = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distance[i, 0] = i++)
            {
            }

            for (int j = 0; j <= lengthB; distance[0, j] = j++)
            {
            }

            for (int i = 1; i <= lengthA; i++)
            {
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = string2[j - 1] == string1[i - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[lengthA, lengthB];
        }
        public static PercentValue CalculateLevenshteinSimilarity(string string1, string string2)
        {
            int levenshteinDistance = CalculateLevenshteinDistance(string1, string2);
            if (levenshteinDistance == 0)
            {
                return PercentValue.HundredPercent;
            }
            int maxLength = Math.Max(string1.Length, string2.Length);
            if (levenshteinDistance == maxLength)
            {
                return PercentValue.ZeroPercent;
            }
            else
            {
                return new PercentValue(1 - (((double)levenshteinDistance) / maxLength));
            }
        }
        public static PercentValue CalculateCosineSimilarity(string string1, string string2)
        {
            int length1 = string1.Length;
            int length2 = string2.Length;
            if ((length1 == 0 && length2 > 0) || (length2 == 0 && length1 > 0))
            {
                return PercentValue.ZeroPercent;
            }
            IDictionary<string, int> a = CalculateSimilarityHelperConvert(CalculateSimilarityHelperGetCharFrequencyMap(string1));
            IDictionary<string, int> b = CalculateSimilarityHelperConvert(CalculateSimilarityHelperGetCharFrequencyMap(string2));
            HashSet<string> intersection = CalculateSimilarityHelperGetIntersectionOfCharSet(a.Keys, b.Keys);
            double dotProduct = 0, magnitudeA = 0, magnitudeB = 0;
            foreach (string item in intersection)
            {
                dotProduct += a[item] * b[item];
            }
            foreach (string k in a.Keys)
            {
                magnitudeA += Math.Pow(a[k], 2);
            }
            foreach (string k in b.Keys)
            {
                magnitudeB += Math.Pow(b[k], 2);
            }
            return new PercentValue(dotProduct / Math.Sqrt(magnitudeA * magnitudeB));
        }
        public static double CalculateJaccardIndex(string string1, string string2)
        {
            return CalculateSimilarityHelperGetIntersection(string1, string2).Count() / (double)CalculateSimilarityHelperGetUnion(string1, string2).Count();
        }

        public static PercentValue CalculateJaccardSimilarity(string string1, string string2)
        {
            return new PercentValue(CalculateJaccardIndex(string1, string2) * 2);
        }

        private static string CalculateSimilarityHelperGetIntersection(string string1, string string2)
        {
            IList<char> list = [];
            foreach (char character in string1)
            {
                if (string2.Contains(character))
                {
                    list.Add(character);
                }
            }
            string result = new([.. list]);
            return result;
        }
        private static IDictionary<char, int> CalculateSimilarityHelperGetCharFrequencyMap(string str)
        {
            Dictionary<char, int> result = [];
            foreach (char chr in str)
            {
                if (result.ContainsKey(chr))
                {
                    result[chr] = result[chr] + 1;
                }
                else
                {
                    result.Add(chr, 1);
                }
            }
            return result;
        }
        private static string CalculateSimilarityHelperGetUnion(string string1, string string2)
        {
            return new string((string1 + string2).ToCharArray());
        }

        private static HashSet<string> CalculateSimilarityHelperGetIntersectionOfCharSet(ICollection<string> keys1, ICollection<string> keys2)
        {
            HashSet<string> result = [];
            result.UnionWith(keys1);
            result.IntersectWith(keys2);
            return result;
        }
        private static IDictionary<string, int> CalculateSimilarityHelperConvert(IDictionary<char, int> dictionary)
        {
            IDictionary<string, int> result = new Dictionary<string, int>();
            foreach (KeyValuePair<char, int> obj in dictionary)
            {
                result.Add(obj.Key.ToString(), obj.Value);
            }
            return result;
        }
    }
}