using GRYLibrary.Core.XMLSerializer;
using System.Text;
using System.Xml.Serialization;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represents a simple Manager for persisting on the file-system and reloading an object.
    /// </summary>
    /// <typeparam name="T">The type of the object which should be persisted.</typeparam>
    /// <remarks>The types of the objects which should be serialized must be stated as class-types (not interface-types for example).
    /// If an object is not serializeable then the type can be "wrapped" in another class which correctly implements <see cref="IXmlSerializable"/>.</remarks>
    public sealed class SimpleObjectPersistence<T> where T : new()
    {
        public T Object { get; set; }
        public string File { get; set; }
        public Encoding FileEncoding { get; set; } = new UTF8Encoding(false);
        public SimpleGenericXMLSerializer<T> Serializer { get; private set; } = new SimpleGenericXMLSerializer<T>();
        public static SimpleObjectPersistence<T> CreateByFile(string file)
        {
            file = Utilities.ResolveToFullPath(file);
            SimpleObjectPersistence<T> result = new()
            {
                File = file
            };
            return result;
        }
        public static SimpleObjectPersistence<T> CreateByObjectAndFile(T @object, string file)
        {
            file = Utilities.ResolveToFullPath(file);
            SimpleObjectPersistence<T> result = new()
            {
                File = file,
                Object = @object
            };
            return result;
        }

        public void LoadObjectFromFile()
        {
            if (!System.IO.File.Exists(this.File) || Utilities.FileIsEmpty(this.File))
            {
                this.ResetObject();
            }
            this.Object = this.Serializer.Deserialize(System.IO.File.ReadAllText(this.File, this.FileEncoding));
        }

        public void ResetObject()
        {
            this.Object = new T();
            this.SaveObjectToFile();
        }

        public void SaveObjectToFile()
        {
            Utilities.EnsureFileExists(this.File, true);
            string value = this.Serializer.Serialize(this.Object);
            System.IO.File.WriteAllText(this.File, value, this.FileEncoding);
        }
    }
}