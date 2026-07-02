using GRYLibrary.Core.Misc.Event;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc
{
    public interface IProperty
    {
        string PropertyName { get; set; }
        Type PropertyValueType { get; }
    }
    [DataContract]
    public class Property<T> : EventSender<Property<T>, PropertyChangedEvengArgument<T>>, IProperty
    {
        [DataMember]
        private string _PropertyName;
        [DataMember]
        public bool AllowNullAsValue = true;
        [DataMember]
        public bool AddValuesToHistory = true;
        [DataMember]
        public bool NotifyAboutChanges = true;
        [DataMember]
        private readonly T _InitialValue;
        [DataMember]
        private T _Value;
        [DataMember]
        private readonly Stack<KeyValuePair<DateTimeOffset, T>> _History;
        [DataMember]
        public bool LockEnabled = false;
        [DataMember]
        public bool Unset = true;

        public object LockObject = new();
        public bool HasValue => !this.Unset;
        public T InitialValue => this._InitialValue;
        /// <summary>
        /// The history contains all T-objects which where set as value for <see cref="Property{T}.Value"/> with the <see cref="DateTime"/> when they were set.
        /// </summary>
        public Stack<KeyValuePair<DateTimeOffset, T>> History => new Stack<KeyValuePair<DateTimeOffset, T>>(new Stack<KeyValuePair<DateTimeOffset, T>>(this._History));
        public void UnsetValue()
        {
            this.Unset = true;
        }

        public string PropertyName { get => this._PropertyName; set => this._PropertyName = value; }

        public Type PropertyValueType => typeof(T);
        public virtual T Value
        {
            get
            {
                if (this.LockEnabled)
                {
                    lock (this.LockObject)
                    {
                        return this.GetValue();
                    }
                }
                else
                {
                    return this.GetValue();
                }
            }
            set
            {
                if (this.LockEnabled)
                {
                    lock (this.LockObject)
                    {
                        this.SetValue(value);
                    }
                }
                else
                {
                    this.SetValue(value);
                }
            }
        }

        public DateTimeOffset LastWriteTime { get; private set; }

        private T GetValue()
        {
            if (this.HasValue)
            {
                return this._Value;
            }
            else
            {
                throw new Exception("No value set");
            }
        }

        private void SetValue(T value)
        {
            if ((value == null) && !this.AllowNullAsValue)
            {
                throw new ArgumentException("null is not allowed as value");
            }
            T oldValue = this._Value;
            T newValue = value;
            this.Unset = false;
            this._Value = newValue;
            DateTimeOffset changeDate = GUtilities.GetNow();
            this.LastWriteTime = changeDate;
            if (this.AddValuesToHistory)
            {
                this._History.Push(new KeyValuePair<DateTimeOffset, T>(changeDate, this._Value));
            }
            Argument<Property<T>, PropertyChangedEvengArgument<T>> argument = new(this, new PropertyChangedEvengArgument<T>(oldValue, newValue, changeDate));
            try
            {
                if (this.NotifyAboutChanges)
                {
                    this.Notify(argument);
                }
            }
            catch
            {
            }
        }

        public Property(T initialValue, string propertyName, bool addValuesToHistory = false)
        {
            this._History = new Stack<KeyValuePair<DateTimeOffset, T>>();
            this._InitialValue = initialValue;
            this.PropertyName = propertyName;
            this.ResetToInitialValue();
            this.AddValuesToHistory = addValuesToHistory;
            if (!this.AddValuesToHistory)
            {
                this.ResetHistory();
            }
        }

        public void ResetToInitialValue()
        {
            this.SetValue(this._InitialValue);
        }

        public void ResetHistory()
        {
            this._History.Clear();
        }

        public bool EqualsValue(Property<T> @object)
        {
            return this.EqualsValue(@object.Value);
        }

        public bool EqualsValue(T @object)
        {
            return this.Value.Equals(@object);
        }

        public T GetValueByTimestamp(DateTimeOffset dateTime)
        {
            Stack<KeyValuePair<DateTimeOffset, T>> history = this.History;
            while (history.Count > 0)
            {
                KeyValuePair<DateTimeOffset, T> current = history.Pop();
                if (current.Key < dateTime)
                {
                    return current.Value;
                }
            }
            throw new Exception("Value was not set at moment " + dateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        }
    }
    public class PropertyChangedEvengArgument<T>
    {
        internal PropertyChangedEvengArgument(T oldValue, T newValue, DateTimeOffset changeMoment)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.ChangeMoment = changeMoment;
        }
        public T OldValue { get; }
        public T NewValue { get; }
        public DateTimeOffset ChangeMoment { get; }
    }
}