using GRYLibrary.Core.Logging.GRYLogger;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GRYLibrary.Core.Misc.Event
{
    [DataContract]
    public abstract class EventSender<SenderType, EventArgumentType>
    {
        public EventSender() : this(null)
        {
        }
        public EventSender(GRYLog logObject)
        {
            this.LogObject = logObject;
            this.Initialize(default);
        }
        private ISet<IObserver<SenderType, EventArgumentType>> _Observer = null;
        public void Register(IObserver<SenderType, EventArgumentType> observer)
        {
            this._Observer.Add(observer);
        }

        public void Deregister(IObserver<SenderType, EventArgumentType> observer)
        {
            this._Observer.Remove(observer);
        }

        [OnDeserializing()]
        public void Initialize(StreamingContext context)
        {
            if (this._Observer == null)
            {
                this._Observer = new HashSet<IObserver<SenderType, EventArgumentType>>();
            }
        }

        public GRYLog LogObject { get; set; }
        protected void Notify(Argument<SenderType, EventArgumentType> argument)
        {
            foreach (IObserver<SenderType, EventArgumentType> observer in this._Observer)
            {
                try
                {
                    observer.Update(this, argument);
                }
                catch (Exception exception)
                {
                    if (this.LogObject != null)
                    {
                        this.LogObject.Log("Error occurred in observer.Update", exception);
                    }
                }
            }
        }
    }
}