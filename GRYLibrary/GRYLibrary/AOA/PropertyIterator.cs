using GRYLibrary.Core.AOA.EqualsHelper.CustomComparer;
using GRYLibrary.Core.AOA.PropertyIteratorHelper;
using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GRYLibrary.Core.AOA
{
    public class PropertyIterator
    {
        public PropertyIteratorConfiguration Configuration { get; set; }
        public PropertyIterator() : this(new PropertyIteratorConfiguration())
        {
        }
        public PropertyIterator(PropertyIteratorConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        public IEnumerable<(object, Type)> IterateOverObjectTransitively(object @object)
        {
            List<(object, Type)> result = [];
            this.IterateOverObjectTransitively(@object, result);
            return result;
        }
        private void IterateOverObjectTransitively(object @object, IList<(object, Type)> visitedObjects)
        {
            if (this.Contains(visitedObjects, @object))
            {
                return;
            }
            bool objectIsNull = @object == null;
            if (objectIsNull)
            {
                visitedObjects.Add((@object, typeof(object)));
                return;
            }
            Type type = @object.GetType();
            visitedObjects.Add((@object, type));
            if (EnumerableTools.TypeIsEnumerable(type))
            {
                foreach (object item in EnumerableTools.ObjectToEnumerable(@object))
                {
                    this.IterateOverObjectTransitively(item, visitedObjects);
                }
            }
            else if (PrimitiveComparer.TypeIsTreatedAsPrimitive(type))
            {
                // TODO
            }
            else
            {

                foreach (FieldInfo field in type.GetFields().Where((field) => this.Configuration.FieldSelector(field)))
                {
                    this.IterateOverObjectTransitively(field.GetValue(@object), visitedObjects);
                }
                foreach (PropertyInfo property in type.GetProperties().Where((property) => this.Configuration.PropertySelector(property)))
                {
                    this.IterateOverObjectTransitively(property.GetValue(@object), visitedObjects);
                }
            }
        }

        private bool Contains(IList<(object, Type)> visitedObjects, object @object)
        {
            foreach ((object, Type) currentItem in visitedObjects)
            {
                if (Utilities.ImprovedReferenceEquals(currentItem.Item1, @object))
                {
                    return true;
                }
            }
            return false;
        }
    }
}