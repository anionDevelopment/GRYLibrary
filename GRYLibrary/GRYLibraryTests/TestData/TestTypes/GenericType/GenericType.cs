using System.Collections.Generic;

namespace GRYLibrary.Tests.TestData.TestTypes.GenericType
{
    public class GenericType<T>
    {
        public IList<T> TList { get; set; }
        public HashSet<T> TSet { get; set; }
        public Dictionary<string, T> TDictionary1 { get; set; }
        public Dictionary<int, T> TDictionary2 { get; set; }
        public KeyValuePair<T, string> TKeyValuePair1 { get; set; }
        public KeyValuePair<string, T> TKeyValuePair2 { get; set; }
        public IEnumerable<T> TEnumerable { get; set; }
        public System.Collections.IEnumerable Enumerable { get; set; }
        public T TObject { get; set; }
    }
}