using System.Collections.Generic;

namespace GRYLibrary.Core.Misc
{
    public class FixedSizeQueue<T>
    {
        private readonly Queue<T> _Queue = new();
        private readonly int _MaxSize;

        public int Count => this._Queue.Count;

        public FixedSizeQueue() : this(int.MaxValue)
        {
        }
        public FixedSizeQueue(int maxSize)
        {
            GRYLibrary.Core.Misc.Utilities.AssertCondition(0 < maxSize, $"{maxSize} must be grater than 0.");
            this._MaxSize = maxSize;
        }
        public T[] GetEntries() => [.. this._Queue];

        public void Enqueue(T item)
        {
            if (this._MaxSize < this._Queue.Count + 1)
            {
                this._Queue.Dequeue();
            }
            this._Queue.Enqueue(item);
        }
        public T Dequeue()
        {
            return this._Queue.Dequeue();
        }

        public void Clear()
        {
            this._Queue.Clear();
        }
    }
}
