using GRYLibrary.Core.AOA.EqualsHelper;
using GRYLibrary.Core.AOA.EqualsHelper.CustomComparer;
using GRYLibrary.Core.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace GRYLibrary.Core.AOA/*AOA=AdvancedObjectAnalysis*/
{
    /// <remarks>
    /// This type is not fully implemented yet. Some things (like serializing arrays for example) are not working yet.
    /// </remarks>
    public class Generic
    {
        private static readonly IdGenerator<int> _IdGenerator = IdGenerator.GetDefaultIntIdGenerator();
        private static readonly Dictionary<object, int> _ObjectReferenceHashCodeCache = new(new ReferenceEqualsComparer());

        /// <summary>
        /// Returns a hash-code of the given object which is calculated from its own values.
        /// </summary>
        /// <remarks>
        /// Two objects which <see cref="GenericEquals(object, object)"/> considers equal get the same hash-code, which
        /// is what a hash-code has to promise. Everything this looks at is therefore also part of that comparison: the
        /// kind of the object, and the values of its members which are treated as primitive.
        ///
        /// The members which are not primitive are deliberately not followed. A hash-code which walks the whole object
        /// would have to handle the cycles of that graph and would cost as much as the comparison it is supposed to
        /// avoid - while the values of one level are enough to tell two objects apart in practice.
        ///
        /// The values matter: a hash-code which only states the type puts every object of that type into the same
        /// bucket of a hash-set, which turns every insert and every lookup into a comparison against everything which
        /// is already in it. A set of n such objects then costs n² deep comparisons instead of n.
        /// </remarks>
        public static int GenericGetHashCode(object @object)
        {
            if (@object == null)
            {
                return 684835431;
            }
            Type type = @object.GetType();
            int result = GetHashCodeOfTheKindOf(type);
            if (PrimitiveComparer.TypeIsTreatedAsPrimitive(type))
            {
                return CombineHashCodes(result, @object.GetHashCode());
            }
            if (@object is ICollection collection)
            {
                // The amount of elements is a value which two equal collections always share. The elements themselves
                // are not asked: that would cost as much as comparing them.
                return CombineHashCodes(result, collection.Count);
            }
            if (EnumerableTools.TypeIsEnumerable(type))
            {
                // An enumerable which is no collection is not enumerated - doing so can have an effect on it and it can
                // be endless.
                return result;
            }
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (0 < property.GetIndexParameters().Length || !_PropertySelector(property))
                {
                    continue;
                }
                object value = property.GetValue(@object);
                if (value != null && PrimitiveComparer.TypeIsTreatedAsPrimitive(value.GetType()))
                {
                    result = CombineHashCodes(result, value.GetHashCode());
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the value which states the kind of the given type. Every set has the same one, every list has
        /// another one, and so on, so a hash-set and another implementation of the same kind hash equally.
        /// </summary>
        private static int GetHashCodeOfTheKindOf(Type type)
        {
            Type kind = type;
            if (PrimitiveComparer.TypeIsTreatedAsPrimitive(type))
            {
                Utilities.NoOperation();
            }
            else if (EnumerableTools.TypeIsSet(type))
            {
                kind = typeof(ISet<>);
            }
            else if (EnumerableTools.TypeIsList(type))
            {
                kind = typeof(IList<>);
            }
            else if (EnumerableTools.TypeIsDictionary(type))
            {
                kind = typeof(IDictionary<,>);
            }
            else if (EnumerableTools.TypeIsEnumerable(type))
            {
                kind = typeof(IEnumerable);
            }
            else
            {
                Utilities.NoOperation();
            }
            lock (_ObjectReferenceHashCodeCache)
            {
                if (!_ObjectReferenceHashCodeCache.ContainsKey(kind))
                {
                    _ObjectReferenceHashCodeCache.Add(kind, _IdGenerator.GenerateNewId());
                }
                return _ObjectReferenceHashCodeCache[kind];
            }
        }

        private static int CombineHashCodes(int hashCodeSoFar, int hashCodeOfTheValue)
        {
            unchecked
            {
                return (hashCodeSoFar * 397) ^ hashCodeOfTheValue;
            }
        }

        /// <summary>
        /// The members which are looked at. It is the same rule which the comparison uses (see
        /// <see cref="EqualsHelper.PropertyEqualsCalculatorConfiguration.PropertySelector"/>), because a hash-code
        /// which looks at something the comparison ignores could differ for two equal objects.
        /// </summary>
        private static readonly Func<PropertyInfo, bool> _PropertySelector = new EqualsHelper.PropertyEqualsCalculatorConfiguration().PropertySelector;

        public static bool GenericEquals(object object1, object object2)
        {
            return new PropertyEqualsCalculator().DefaultEquals(object1, object2);
        }

        public static string GenericToString(object @object, int maxOutputLength = int.MaxValue)
        {
            return AOA.GenericToString.Instance.ToString(@object, maxOutputLength);
        }

#pragma warning disable IDE0060 // Suppress "Remove unused parameter 'object'"
        public static XmlSchema GenericGetSchema(object @object)
        {
            return null;
        }

        public static void GenericWriteXml(object @object, XmlWriter writer)
        {
            GenericXMLSerializer.CreateForObject(@object).Serialize(@object, writer);
        }

        public static void GenericReadXml(object @object, XmlReader reader)
        {
            GenericXMLSerializer genericXMLSerializer = GenericXMLSerializer.CreateForObject(@object);
            genericXMLSerializer.CopyContentOfObject(@object, genericXMLSerializer.Deserialize(reader));
        }
        public static IEnumerable<(object, Type)> IterateOverObjectTransitively(object @object)
        {
            return new PropertyIterator().IterateOverObjectTransitively(@object);
        }

        public static string GenericSerialize(object @object)
        {
            using StringWriter stringWriter = new();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
            {
                GenericWriteXml(@object, xmlWriter);
            }
            return stringWriter.ToString();
        }

        internal static void GenericSerializeToFile(object @object, string file)
        {
            File.WriteAllBytes(file, new UTF8Encoding(false).GetBytes(GenericSerialize(@object)));
        }

        public static T GenericDeserialize<T>(string serializedObject)
        {
            using XmlReader xmlReader = XmlReader.Create(new StringReader(serializedObject));
            T result = Activator.CreateInstance<T>();
            GenericReadXml(result, xmlReader);
            return result;
        }
        public static T GenericDeserializeFromFile<T>(string file)
        {
            return GenericDeserialize<T>(new UTF8Encoding(false).GetString(File.ReadAllBytes(file)));
        }
    }
}