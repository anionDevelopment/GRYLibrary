using GRYLibrary.Core.AOA.EqualsHelper;
using System;
using System.Reflection;
using System.Text;

namespace GRYLibrary.Core.AOA.SerializeHelper
{
    public class SerializationConfiguration
    {
        public Func<PropertyInfo, bool> PropertySelector { get; set; }
        public Func<FieldInfo, bool> FieldSelector { get; set; }
        public Encoding Encoding { get; set; }
        public bool Indent { get; set; } = true;
        internal PropertyEqualsCalculatorConfiguration PropertyEqualsCalculatorConfiguration { get; set; } = new PropertyEqualsCalculatorConfiguration();
    }
}