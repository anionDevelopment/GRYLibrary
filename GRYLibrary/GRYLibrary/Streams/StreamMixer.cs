using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace GRYLibrary.Core.Streams
{
    internal class StreamMixer
    {
        private readonly ISet<StreamPipe> _Connections = new HashSet<StreamPipe>();
        public void AddPipe(StreamPipe connection)
        {
            if (this._Connections.Add(connection))
            {
                connection.Start();
            }
            ;
        }
        public void RemovePipe(StreamPipe connection)
        {
            if (this._Connections.Remove(connection))
            {
                connection.Stop();
            }
            ;
        }
        public IImmutableSet<StreamPipe> GetConnections()
        {
            return this._Connections.ToImmutableHashSet();
        }

        public class StreamPipe
        {
            public Stream Source { get; }
            public Stream Target { get; }
            public StreamPipe(Stream source, Stream target)
            {
                this.Source = source;
                this.Target = target;
            }
            public override bool Equals(object obj)
            {
                if ((StreamPipe)obj is StreamPipe typedObject)
                {
                    return typedObject.Source.Equals(this.Source) && typedObject.Target.Equals(this.Target);
                }
                else
                {
                    return false;
                }
            }
            private bool _Started = false;
            internal void Start()
            {
                if (!this._Started)
                {
                    this._Started = true;
                    throw new System.NotImplementedException();
                }
            }
            internal void Stop()
            {
                if (this._Started)
                {
                    this._Started = false;
                    throw new System.NotImplementedException();
                }
            }
            public override int GetHashCode()
            {
                return this.Source.GetHashCode() ^ this.Target.GetHashCode();
            }
        }
    }
}