using GRYLibrary.Core.AOA.SerializeHelper.SimplifiedObjects;
using GRYLibrary.Core.AOA.EqualsHelper;
using GRYLibrary.Core.AOA.EqualsHelper.CustomComparer;
using GRYLibrary.Core.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static GRYLibrary.Core.AOA.SerializeHelper.FlatObject;

namespace GRYLibrary.Core.AOA.SerializeHelper
{
    /// <summary>
    /// Represents a GRYSerializedObject.
    /// </summary>
    public sealed class GRYSObject : IDisposable
    {
        public Guid RootObjectId { get; set; }
        public HashSet<FlatObject> Objects { get; set; }
        private readonly IList<object> _DeserializedSets = [];
        public static GRYSObject Create(object @object, SerializationConfiguration serializationConfiguration)
        {
            Dictionary<object, FlatObject> dictionary = new(new ReferenceEqualsComparer());
            FillDictionary(dictionary, @object, serializationConfiguration);
            GRYSObject result = new()
            {
                Objects = new HashSet<FlatObject>(dictionary.Values),
            };
            foreach (object key in dictionary.Keys)
            {
                if (key == @object)
                {
                    result.RootObjectId = dictionary[@object].ObjectId;
                }
            }
            return result;
        }

        private static Guid FillDictionary(Dictionary<object, FlatObject> dictionary, object @object, SerializationConfiguration serializationConfiguration)
        {
            if (@object == null)
            {
                return default;
            }
            Type typeOfObject = @object.GetType();
            if (EnumerableTools.ObjectIsKeyValuePair(@object))
            {
                KeyValuePair<object, object> kvp = EnumerableTools.ObjectToKeyValuePair<object, object>(@object);
                @object = new XMLSerializer.KeyValuePair<dynamic, dynamic>(kvp.Key, kvp.Value);
            }
            if (dictionary.ContainsKey(@object))
            {
                return dictionary[@object].ObjectId;
            }
            else
            {
                FlatObject simplification;
                if (PrimitiveComparer.TypeIsTreatedAsPrimitive(typeOfObject))
                {
                    simplification = new FlatPrimitive();
                }
                else if (EnumerableTools.ObjectIsEnumerable(@object))
                {
                    simplification = new FlatEnumerable();
                }
                else
                {
                    simplification = new FlatComplexObject();
                }
                simplification.ObjectId = Guid.NewGuid();
                simplification.TypeName = @object.GetType().AssemblyQualifiedName;
                dictionary.Add(@object, simplification);
                simplification.Accept(new SerializeVisitor(@object, dictionary, serializationConfiguration));

                return simplification.ObjectId;
            }
        }

        private class DeserializeVisitor : IFlatObjectVisitor
        {
            private readonly IDictionary<Guid, object> _DeserializedObjects;
            public DeserializeVisitor(IDictionary<Guid, object> deserializedObjects)
            {
                this._DeserializedObjects = deserializedObjects;
            }
            public void Handle(FlatComplexObject simplifiedObject)
            {
                object @object = this.GetDeserialisedObjectOrDefault(simplifiedObject.ObjectId);
                Type typeOfObject = @object.GetType();
                foreach (FlatAttribute attribute in simplifiedObject.Attributes)
                {
                    PropertyInfo property = typeOfObject.GetProperty(attribute.Name);
                    if (property != null)
                    {
                        if (property.PropertyType.Equals(typeof(Type)))
                        {
                            property.SetValue(@object, Type.GetType(this._DeserializedObjects[attribute.ObjectId].ToString()));
                        }
                        else
                        {
                            if (attribute.ObjectId.Equals(default))
                            {
                                property.SetValue(@object, Utilities.GetDefault(property.PropertyType));
                            }
                            else
                            {
                                property.SetValue(@object, this._DeserializedObjects[attribute.ObjectId]);
                            }
                        }
                        continue;
                    }

                    FieldInfo field = typeOfObject.GetField(attribute.Name);
                    if (field != null)
                    {
                        if (attribute.ObjectId.Equals(default))
                        {
                            field.SetValue(@object, Utilities.GetDefault(field.FieldType));
                        }
                        else
                        {
                            field.SetValue(@object, this._DeserializedObjects[attribute.ObjectId]);
                        }
                        continue;
                    }

                    throw new KeyNotFoundException($"Can not find attribute '{attribute.Name}' in type '{typeOfObject}'.");
                }
            }

            public void Handle(FlatEnumerable simplifiedEnumerable)
            {
                object enumerable = this.GetDeserialisedObjectOrDefault(simplifiedEnumerable.ObjectId);
                if (Utilities.IsDefault(enumerable))
                {
                    return;
                }
                Type enumerableType = Type.GetType(simplifiedEnumerable.TypeName);
                bool isDictionaryNotGeneric = EnumerableTools.TypeIsDictionaryNotGeneric(enumerableType);
                bool isDictionaryGeneric = EnumerableTools.TypeIsDictionaryGeneric(enumerableType);
                foreach (Guid itemId in simplifiedEnumerable.Items)
                {
                    object itemForEnumerable;
                    itemForEnumerable = this.GetDeserialisedObjectOrDefault(itemId);
                    object[] arguments;
                    if (isDictionaryGeneric)
                    {
                        XMLSerializer.KeyValuePair<object, object> gkvp = (XMLSerializer.KeyValuePair<object, object>)itemForEnumerable;
                        arguments = [gkvp.Key, gkvp.Value];
                    }
                    else if (isDictionaryNotGeneric)
                    {
                        DictionaryEntry keyValuePair = EnumerableTools.ObjectToDictionaryEntry(itemForEnumerable);
                        arguments = [keyValuePair.Key, keyValuePair.Value];
                    }
                    else
                    {
                        arguments = [itemForEnumerable];
                    }
                    EnumerableTools.AddItemToEnumerable(enumerable, arguments);
                }
            }
            public void Handle(FlatPrimitive simplifiedPrimitive)
            {
                Utilities.NoOperation();
            }

            private object GetDeserialisedObjectOrDefault(Guid id)
            {
                if (default(Guid).Equals(id))
                {
                    return default;
                }
                else
                {
                    return this._DeserializedObjects[id];
                }
            }
        }
        private class CreateObjectVisitor : IFlatObjectVisitor<object>
        {
            public bool IsSet { get; private set; } = false;
            public object Handle(FlatComplexObject simplifiedObject)
            {
                Type type = Type.GetType(simplifiedObject.TypeName);
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (MissingMethodException)
                {
                    throw;
                }
            }

            public object Handle(FlatEnumerable simplifiedEnumerable)
            {
                Type typeOfSimplifiedEnumerable = Type.GetType(simplifiedEnumerable.TypeName);
                Type concreteTypeOfEnumerable;
                /*if (EnumerableTools.TypeIsArrayGeneric(typeOfSimplifiedEnumerable))
               {
                   concreteTypeOfEnumerable = typeOfSimplifiedEnumerable;
               }
               else*/
                if (EnumerableTools.TypeIsListGeneric(typeOfSimplifiedEnumerable))
                {
                    concreteTypeOfEnumerable = typeof(List<>);
                }
                else if (EnumerableTools.TypeIsListNotGeneric(typeOfSimplifiedEnumerable))
                {
                    concreteTypeOfEnumerable = typeof(ArrayList);
                }
                else if (EnumerableTools.TypeIsSet(typeOfSimplifiedEnumerable))
                {
                    concreteTypeOfEnumerable = typeof(GRYSet<>);
                    this.IsSet = true;
                }
                else if (EnumerableTools.TypeIsDictionaryGeneric(typeOfSimplifiedEnumerable))
                {
                    concreteTypeOfEnumerable = typeof(Dictionary<,>);
                }
                else if (EnumerableTools.TypeIsDictionaryNotGeneric(typeOfSimplifiedEnumerable))
                {
                    concreteTypeOfEnumerable = typeof(Hashtable);
                }
                else if (EnumerableTools.TypeIsEnumerable(typeOfSimplifiedEnumerable))
                {
                    concreteTypeOfEnumerable = typeof(List<>);
                }
                else if (EnumerableTools.TypeIsKeyValuePair(typeOfSimplifiedEnumerable))
                {
                    concreteTypeOfEnumerable = typeof(List<>);
                }
                else
                {
                    throw new ArgumentException($"Unknown type of enumerable: {typeOfSimplifiedEnumerable}");
                }
                if (typeOfSimplifiedEnumerable.GenericTypeArguments.Length > 0)
                {
                    concreteTypeOfEnumerable = concreteTypeOfEnumerable.MakeGenericType(typeOfSimplifiedEnumerable.GenericTypeArguments);
                }
                return Activator.CreateInstance(concreteTypeOfEnumerable);
            }

            public object Handle(FlatPrimitive simplifiedPrimitive)
            {
                if (simplifiedPrimitive.TypeName == typeof(Type).AssemblyQualifiedName)
                {
                    return Type.GetType((string)simplifiedPrimitive.Value);
                }
                else
                {
                    return simplifiedPrimitive.Value;
                }
            }
        }
        private class SerializeVisitor : IFlatObjectVisitor
        {
            private readonly object _Object;
            private readonly Dictionary<object, FlatObject> _Dictionary;
            private readonly SerializationConfiguration _SerializationConfiguration;

            public SerializeVisitor(object @object, Dictionary<object, FlatObject> dictionary, SerializationConfiguration serializationConfiguration)
            {
                this._Object = @object;
                this._Dictionary = dictionary;
                this._SerializationConfiguration = serializationConfiguration;
            }

            public void Handle(FlatComplexObject simplifiedObject)
            {
                foreach (PropertyInfo property in this._Object.GetType().GetProperties())
                {
                    if (this._SerializationConfiguration.PropertySelector(property))
                    {
                        AddSimplifiedAttribute(simplifiedObject, property.Name, property.PropertyType, property.GetValue(this._Object), this._Dictionary, this._SerializationConfiguration);
                    }
                }
                foreach (FieldInfo field in this._Object.GetType().GetFields())
                {
                    if (this._SerializationConfiguration.FieldSelector(field))
                    {
                        AddSimplifiedAttribute(simplifiedObject, field.Name, field.FieldType, field.GetValue(this._Object), this._Dictionary, this._SerializationConfiguration);
                    }
                }
            }

            public void Handle(FlatEnumerable simplifiedEnumerable)
            {
                simplifiedEnumerable.Items = [];
                foreach (object @object in EnumerableTools.ObjectToEnumerable<object>(this._Object))
                {
                    simplifiedEnumerable.Items.Add(FillDictionary(this._Dictionary, @object, this._SerializationConfiguration));
                }
            }

            public void Handle(FlatPrimitive simplifiedPrimitive)
            {
                if (Utilities.IsAssignableFrom(this._Object, typeof(Type)))
                {
                    simplifiedPrimitive.Value = ((Type)this._Object).AssemblyQualifiedName;
                }
                else if (this._Object.GetType().IsEnum)
                {
                    simplifiedPrimitive.Value = (int)this._Object;
                }
                else
                {
                    simplifiedPrimitive.Value = this._Object;
                }
            }
        }
        private static void AddSimplifiedAttribute(FlatComplexObject container, string attributeName, Type attributeType, object attributeValue, Dictionary<object, FlatObject> dictionary, SerializationConfiguration serializationConfiguration)
        {
            FlatAttribute attribute = new()
            {
                ObjectId = FillDictionary(dictionary, attributeValue, serializationConfiguration),
                Name = attributeName,
                TypeName = attributeType.AssemblyQualifiedName
            };
            container.Attributes.Add(attribute);
        }

        internal object Get()
        {
            Dictionary<Guid, object> deserializedObjects = [];
            IList<FlatObject> sorted = [.. this.Objects];
            sorted = [.. sorted.OrderBy((item) =>
            {
                if (item.TypeName.StartsWith("GRYLibrary.Core.XMLSerializer.KeyValuePair"))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            })];
            foreach (FlatObject simplified in sorted)
            {
                CreateObjectVisitor createObjectVisitor = new();
                object createdObject = simplified.Accept(createObjectVisitor);
                deserializedObjects.Add(simplified.ObjectId, createdObject);
                if (createObjectVisitor.IsSet)
                {
                    this._DeserializedSets.Add(createdObject);
                }
            }
            foreach (FlatObject simplified in sorted)
            {
                simplified.Accept(new DeserializeVisitor(deserializedObjects));
            }
            return deserializedObjects[this.RootObjectId];
        }

        public void Dispose()
        {
            this.EnableSetFunctionality();
        }

        private void EnableSetFunctionality()
        {
            foreach (object grySet in this._DeserializedSets)
            {
                MethodInfo method = grySet.GetType().GetMethod(nameof(GRYSet<object>.DisallowDuplicatedElements), BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(grySet, null);
            }
        }
    }
}