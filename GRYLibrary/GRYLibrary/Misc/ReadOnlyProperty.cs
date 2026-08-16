using System;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represents a property whose value can not be set directly but updated by an update-function. You can always get the value from the last run of the update-function.
    /// </summary>
    public abstract class ReadOnlyProperty
    {
        public abstract void UpdateValue();
        public abstract object GetValue();
    }
    public class ReadOnlyProperty<T> : ReadOnlyProperty
    {
        private readonly Property<T> _Property;
        private readonly Func<Tuple<bool/*calculateValueWasSuccessful*/, T/*value*/>> _SetValueFunction;
        public DateTimeOffset LastUpdate()
        {
            return this._Property.LastWriteTime;
        }

        /// <param name="setValueFunction">
        /// Represents the function which can update the <see cref="Value"/>.
        /// This function must return a tuple which contains
        /// a value which indicates whether the operation was successful or not and
        /// the updated value if the  operation was successful.
        /// </param>
        public ReadOnlyProperty(Func<Tuple<bool, T>> setValueFunction)
        {
            this._Property = new Property<T>(default, string.Empty, false);
            this._SetValueFunction = setValueFunction;
        }
        public override void UpdateValue()
        {
            try
            {
                Tuple<bool, T> result = this._SetValueFunction();
                if (result.Item1)
                {
                    this._Property.Value = result.Item2;
                }
                else
                {
                    this._Property.UnsetValue();
                }
            }
            catch
            {
                this._Property.UnsetValue();
            }
        }

        public override object GetValue()
        {
            return this.Value;
        }

        public T Value => this._Property.Value;
    }
}