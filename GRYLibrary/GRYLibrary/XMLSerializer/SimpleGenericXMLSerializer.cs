using ExtendedXmlSerializer.Configuration;
using ExtendedXmlSerializer;
using System.IO;
using System.Xml;

namespace GRYLibrary.Core.XMLSerializer
{
    /// <summary>
    /// Represents a very easy usable XML-Serializer.
    /// </summary>
    /// <typeparam name="T">The type of the object which should be serialized.</typeparam>
    public class SimpleGenericXMLSerializer<T> where T : new()
    {
        public virtual IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer().UseAutoFormatting()
                                                                .UseOptimizedNamespaces()
                                                                .EnableImplicitTyping(typeof(T))
                                                                .EnableReferences()
                                                                .WithEnumerableSupport()
                                                                .Emit(EmitBehaviors.Classic)
                                                                .Create();
        }

        public string Serialize(T @object)
        {
            string document = this.GetSerializer().Serialize(new XmlWriterSettings { Indent = true }, @object);
            return document;

        }

        public T Deserialize(string xml)
        {
            T document = this.GetSerializer().Deserialize<T>(xml);
            return document;
        }
    }
}