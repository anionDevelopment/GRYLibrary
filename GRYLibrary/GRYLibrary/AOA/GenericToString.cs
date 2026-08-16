using GRYLibrary.Core.AOA.EqualsHelper;
using GRYLibrary.Core.AOA.EqualsHelper.CustomComparer;
using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GRYLibrary.Core.AOA
{
    public class GenericToString
    {
        public Func<PropertyInfo, bool> PropertySelector { get; set; } = (PropertyInfo propertyInfo) => propertyInfo.CanWrite && propertyInfo.GetMethod.IsPublic;
        public Func<FieldInfo, bool> FieldSelector { get; set; } = (FieldInfo propertyInfo) => false;
        public static GenericToString Instance { get; } = new GenericToString();
        private GenericToString() { }
        /// <summary>
        /// Represents a generic ToString-function which can handle cyclic references.
        /// </summary>
        /// <param name="object">The object which should be converted to a string</param>
        /// <param name="maxOutputLength">Maximal length of the output</param>
        /// <returns></returns>
        public string ToString(object @object, int maxOutputLength = int.MaxValue)
        {
            int minimalOutputLength = 4;
            if (maxOutputLength < minimalOutputLength)
            {
                throw new Exception($"The value of '{nameof(maxOutputLength)}' is {maxOutputLength} but must be {minimalOutputLength} or greater.");
            }
            string result = this.ToString(@object, new Dictionary<object, int>(new ReferenceEqualsComparer()), 0);
            if (result.Length > maxOutputLength)
            {
                result = result[..(maxOutputLength - 3)] + "...";
            }
            return result;
        }
        private string ToString(object @object, IDictionary<object, int> visitedObjects, int currentIndentationLevel)
        {
            if (@object == null)
            {
                return this.GetIndentation(currentIndentationLevel) + "null";
            }
            Type type = @object.GetType();
            if (PrimitiveComparer.TypeIsTreatedAsPrimitive(type))
            {
                return this.GetIndentation(currentIndentationLevel) + $"(Type: {@object.GetType().Name}, Value: \"{@object.ToString().Replace("\"", "\\\"")}\")";
            }
            if (visitedObjects.ContainsKey(@object))
            {
                return this.GetIndentation(currentIndentationLevel) + $"[Object {visitedObjects[@object]}]";
            }
            try
            {
                // Assign a per-invocation, per-object id so that the output is deterministic and
                // independent of any process-wide state. Using the object's hash-code here would make
                // the id depend on how many other types were processed earlier in the process.
                int id = visitedObjects.Count + 1;
                visitedObjects.Add(@object, id);

                if (EnumerableTools.ObjectIsEnumerable(@object))
                {
                    IList<object> objectAsEnumerable = [.. EnumerableTools.ObjectToEnumerable<object>(@object)];
                    string result = this.GetIndentation(currentIndentationLevel) + "[" + Environment.NewLine;
                    int count = objectAsEnumerable.Count;
                    for (int i = 0; i < count; i++)
                    {
                        object current = objectAsEnumerable[i];
                        result += this.ToString(current, visitedObjects, currentIndentationLevel + 1);
                        if (i < count - 1)
                        {
                            result = result + "," + Environment.NewLine;
                        }
                    }
                    return result + Environment.NewLine + this.GetIndentation(currentIndentationLevel) + "]";
                }
                else
                {
                    List<(string/*Propertyname*/, object)> propertyValues = [];
                    foreach (FieldInfo field in type.GetFields())
                    {
                        if (this.FieldSelector(field))
                        {
                            propertyValues.Add((field.Name, field.GetValue(@object)));
                        }
                    }
                    foreach (PropertyInfo property in type.GetProperties())
                    {
                        if (this.PropertySelector(property))
                        {
                            propertyValues.Add((property.Name, property.GetValue(@object)));
                        }
                    }
                    string result = this.GetIndentation(currentIndentationLevel) + $"{{ (ObjectId: {id}, Type: {type.FullName}) ";
                    foreach ((string, object) entry in propertyValues)
                    {
                        result = result + Environment.NewLine + this.GetIndentation(currentIndentationLevel + 1) + entry.Item1 + ": " + Environment.NewLine + this.ToString(entry.Item2, visitedObjects, currentIndentationLevel + 1);
                    }
                    return result + Environment.NewLine + this.GetIndentation(currentIndentationLevel) + "}";
                }
            }
            catch
            {
                return $"[Error while executing {nameof(ToString)} for object of type {type.FullName}]";
            }
        }

        private string GetIndentation(int currentIndentationLevel)
        {
            return string.Empty.PadRight(currentIndentationLevel * 2);
        }
    }
}