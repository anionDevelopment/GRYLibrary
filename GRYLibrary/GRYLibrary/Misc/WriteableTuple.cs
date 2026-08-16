using GRYLibrary.Core.AOA;
using System;

namespace GRYLibrary.Core.Misc
{
    public class WriteableTuple<T1, T2> : Tuple<T1, T2>
    {
        public WriteableTuple() : base(default, default)
        {
        }
        public WriteableTuple(T1 item1, T2 item2) : base(item1, item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
        public new T1 Item1 { get; set; }
        public new T2 Item2 { get; set; }
        public override int GetHashCode()
        {
            return Generic.GenericGetHashCode(this.Item1);
        }

        public override bool Equals(object obj)
        {
            if (obj is not WriteableTuple<T1, T2> typedObject)
            {
                return false;
            }
            if (!this.Item1.NullSafeEquals(typedObject.Item1))
            {
                return false;
            }
            if (!this.Item2.NullSafeEquals(typedObject.Item2))
            {
                return false;
            }
            return true;
        }
    }
}