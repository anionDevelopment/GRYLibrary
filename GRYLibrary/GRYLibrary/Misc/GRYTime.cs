using System;
using System.Globalization;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represents a time of a day (without any date-information) without milliseconds.
    /// </summary>
    public struct GRYTime : IEquatable<GRYTime>, IComparable<GRYTime>, IComparable
    {
        public static readonly string TimeFormat = "HH:mm:ss";
        public uint Hour { get; set; }
        public uint Minute { get; set; }
        public uint Second { get; set; }
        public static GRYTime GetCurrentTime()
        {
            return GRYDateTime.GetCurrentDateTime().ToGRYTime();
        }

        public static GRYTime GetCurrentTimeInUTC()
        {
            return GRYDateTime.GetCurrentDateTimeInUTC().ToGRYTime();
        }

        public GRYTime(uint hour, uint minute, uint second)
        {
            this.Hour = hour;
            this.Minute = minute;
            this.Second = second;
            this.ToTime();//check if time is valid
        }
        public readonly TimeOnly ToTime()
        {
            return new TimeOnly((int)this.Hour, (int)this.Minute, (int)this.Second);
        }

        public static GRYTime FromTime(TimeOnly value)
        {
            return new GRYTime((uint)value.Hour, (uint)value.Minute, (uint)value.Second);
        }

        public static GRYTime FromString(string @string)
        {
            return FromTime(TimeOnly.ParseExact(@string, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None));
        }

        public override readonly int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override readonly string ToString()
        {
            return $"{this.Hour.ToString().PadLeft(2, '0')}:{this.Minute.ToString().PadLeft(2, '0')}:{this.Second.ToString().PadLeft(2, '0')}";
        }

        public override readonly bool Equals(object obj)
        {
            return obj is GRYTime time && this.Equals(time);
        }

        public readonly bool Equals(GRYTime other)
        {
            return this.Hour == other.Hour &&
                   this.Minute == other.Minute &&
                   this.Second == other.Second;
        }

        public readonly int CompareTo(GRYTime other)
        {
            return this.ToTime().CompareTo(other.ToTime());
        }

        public readonly int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is GRYTime other)
            {
                return this.CompareTo(other);
            }
            throw new ArgumentException($"Object must be of type {nameof(GRYTime)}.", nameof(obj));
        }

        public static bool operator ==(GRYTime left, GRYTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GRYTime left, GRYTime right)
        {
            return !(left == right);
        }

        public static bool operator <(GRYTime left, GRYTime right)
        {
            return left.ToTime() < right.ToTime();
        }
        public static bool operator >(GRYTime left, GRYTime right)
        {
            return left.ToTime() > right.ToTime();
        }
        public static bool operator <=(GRYTime left, GRYTime right)
        {
            return left.ToTime() <= right.ToTime();
        }
        public static bool operator >=(GRYTime left, GRYTime right)
        {
            return left.ToTime() >= right.ToTime();
        }
        public readonly GRYTime AddHours(int amountOfHours)
        {
            GRYDate date = new GRYDate(2000, 1, 1);
            GRYDateTime dateTime = GRYDateTime.FromGRYDateAndTime(date, new GRYTime(this.Hour, this.Minute, this.Second));
            GRYTime result = dateTime.AddHours(amountOfHours).ToGRYTime();
            return result;
        }
        public readonly GRYTime AddMinutes(int amountOfMinuts)
        {
            GRYDate date = new GRYDate(2000, 1, 1);
            GRYDateTime dateTime = GRYDateTime.FromGRYDateAndTime(date, new GRYTime(this.Hour, this.Minute, this.Second));
            GRYTime result = dateTime.AddMinutes(amountOfMinuts).ToGRYTime();
            return result;
        }
        public readonly GRYTime AddSeconds(int amountOfSeconds)
        {
            GRYDate date = new GRYDate(2000, 1, 1);
            GRYDateTime dateTime = GRYDateTime.FromGRYDateAndTime(date, new GRYTime(this.Hour, this.Minute, this.Second));
            GRYTime result = dateTime.AddSeconds(amountOfSeconds).ToGRYTime();
            return result;
        }
    }
}
