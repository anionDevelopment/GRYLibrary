using GRYLibrary.Core.AOA.EqualsHelper.CustomComparer;
using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GRYLibrary.Core.AOA.EqualsHelper
{
    public class PropertyEqualsCalculatorConfiguration
    {
        private static readonly IdGenerator<int> _IdGenerator = IdGenerator.GetDefaultIntIdGenerator();
        internal ISet<EquivalenceClass> EquivalenceClasses { get; } = new HashSet<EquivalenceClass>();

        /// <summary>
        /// States for an object which equivalence-class it belongs to.
        /// </summary>
        /// <remarks>
        /// The classes themselves are not searched for it. A comparison asks this question for every pair of objects it
        /// looks at, and searching through every class would make the comparison of two objects depend on how many
        /// objects were compared before - which is what turns the comparison of two large object-graphs into a
        /// comparison of every object with every other one.
        /// </remarks>
        private Dictionary<object, EquivalenceClass> EquivalenceClassOfAnObject { get; } = new Dictionary<object, EquivalenceClass>(new ReferenceEqualsComparer());
        private ISet<ReferenceTuple> NotEqualPairs { get; } = new HashSet<ReferenceTuple>();
        private ISet<ReferenceTuple> PendingComparisons { get; } = new HashSet<ReferenceTuple>();
        public Func<PropertyInfo, bool> PropertySelector { get; set; } = (PropertyInfo propertyInfo) =>
        {
            try
            {
                if (propertyInfo.GetMethod != null)
                {
                    if (!propertyInfo.GetMethod.IsPublic)
                    {
                        return false;
                    }
                    if (propertyInfo.GetMethod.IsStatic)
                    {
                        return false;
                    }
                }
                if (propertyInfo.SetMethod != null)
                {
                    if (!propertyInfo.SetMethod.IsPublic)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        };
        public Func<FieldInfo, bool> FieldSelector { get; set; } = (FieldInfo fieldInfo) => false;
        public List<AbstractCustomComparer> CustomComparer { get; set; }
        public PropertyEqualsCalculatorConfiguration()
        {
            this.CustomComparer = [
              new PrimitiveComparer(this),
              new TypeComparer(this),
              new KeyValuePairComparer(this),
              new TupleComparer(this),
              new ListComparer(this),
              new SetComparer(this),
              new DictionaryComparer(this),
              new EnumerableComparer(this),
              new AttributeValueComparer(this),
            ];
        }
        internal void AddPending(object object1, object object2)
        {
            this.PendingComparisons.Add(new ReferenceTuple(object1, object2));
        }

        internal bool ArePending(object object1, object object2)
        {
            return this.PendingComparisons.Contains(new ReferenceTuple(object1, object2));
        }

        internal void RemovePending(object object1, object object2)
        {
            this.PendingComparisons.Remove(new ReferenceTuple(object1, object2));
        }

        public int GetHashCode(object @object)
        {
            return Generic.GenericGetHashCode(@object);
        }

        /// <remarks>This function requires that <paramref name="object"/> was already assigned to an <see cref="EquivalenceClass"/>.</remarks>
        private EquivalenceClass GetEquivalenceClassOfObject(object @object)
        {
            if (@object != null && this.EquivalenceClassOfAnObject.TryGetValue(@object, out EquivalenceClass equivalenceClass))
            {
                return equivalenceClass;
            }
            throw new KeyNotFoundException($"Object '{@object}' was not assigned to an {nameof(EquivalenceClass)} yet.");
        }

        internal void MarkedAsNotEqual(object object1, object object2)
        {
            this.RemovePending(object1, object2);
            this.NotEqualPairs.Add(new ReferenceTuple(object1, object2));
        }
        internal bool WereMarkedAsNotEqual(object object1, object object2)
        {
            return this.NotEqualPairs.Contains(new ReferenceTuple(object1, object2));
        }

        private bool BelongsToEquivalenceClass(EquivalenceClass equivalenceClass, object @object)
        {
            return equivalenceClass.Contains(@object);
        }

        public bool AreInSameEquivalenceClass(object object1, object object2)
        {
            if (!this.HasEquivalenceClass(object1))
            {
                return false;
            }
            if (!this.HasEquivalenceClass(object2))
            {
                return false;
            }
            return this.GetEquivalenceClassOfObject(object1).Equals(this.GetEquivalenceClassOfObject(object2));
        }

        private bool HasEquivalenceClass(object @object)
        {
            return @object != null && this.EquivalenceClassOfAnObject.ContainsKey(@object);
        }

        internal void AddEqualObjectsToEquivalenceClasses(object object1, object object2)
        {
            this.RemovePending(object1, object2);
            if (this.HasEquivalenceClass(object1))
            {
                this.AddToEquivalenceClass(this.GetEquivalenceClassOfObject(object1), object2);
                return;
            }
            if (this.HasEquivalenceClass(object2))
            {
                this.AddToEquivalenceClass(this.GetEquivalenceClassOfObject(object2), object1);
                return;
            }
            EquivalenceClass equivalenceClass = new(object1, _IdGenerator.GenerateNewId());
            this.EquivalenceClasses.Add(equivalenceClass);
            this.AddToEquivalenceClass(equivalenceClass, object1);
            this.AddToEquivalenceClass(equivalenceClass, object2);
        }

        private void AddToEquivalenceClass(EquivalenceClass equivalenceClass, object @object)
        {
            equivalenceClass.Add(@object);
            if (@object != null)
            {
                this.EquivalenceClassOfAnObject[@object] = equivalenceClass;
            }
        }
    }
}