using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GRYLibrary.Core.AOA.SerializeHelper
{
    public interface IGRYSerializable : IXmlSerializable
    {
        public ISet<Type> GetExtraTypesWhichAreRequiredForSerialization();
    }
}