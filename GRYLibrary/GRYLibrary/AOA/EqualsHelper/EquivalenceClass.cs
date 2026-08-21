using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.AOA.EqualsHelper
{
    internal class EquivalenceClass
    {
        public int HashChode { get; }
        public Guid Id { get; }
        public object ReferenceItem { get; }
        public ISet<object> ContainedObjects => new HashSet<object>(this._ContainedObjects, new ReferenceEqualsComparer());
        private readonly ISet<object> _ContainedObjects;
        public EquivalenceClass(object @object, int hashCode)
        {
            this.Id = Guid.NewGuid();
            this.HashChode = hashCode;
            this.ReferenceItem = @object;
            this._ContainedObjects = new HashSet<object>(new ReferenceEqualsComparer())
            {
                this.ReferenceItem
            };
        }
        public override bool Equals(object @object)
        {
            return @object is EquivalenceClass @class && this.Id.Equals(@class.Id);
        }

        public override int GetHashCode()
        {
            return this.HashChode;
        }

        internal void Add(object @object)
        {
            this._ContainedObjects.Add(@object);
        }

        /// <summary>
        /// States whether the given object belongs to this equivalence-class.
        /// </summary>
        /// <remarks>
        /// This asks the contained objects directly. <see cref="ContainedObjects"/> is not used for it: that property
        /// creates a copy of the whole set on every access, which is the wrong price for a question about one object.
        /// </remarks>
        internal bool Contains(object @object)
        {
            return this._ContainedObjects.Contains(@object);
        }
    }
}