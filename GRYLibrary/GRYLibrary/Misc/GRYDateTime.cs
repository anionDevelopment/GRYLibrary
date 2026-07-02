using System;
using System.Globalization;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represents a datetime without milliseconds in the gregorian calendar in UTC.
    /// </summary>
    public struct GRYDateTime : IEquatable<GRYDateTime>, IComparable<GRYDateTime>, IComparable
    {
        public static readonly string DateFormat = "yyyy-MM-dd HH:mm:ss";
        public uint Year { get; set; }
        public uint Month { get; set; }
        public uint Day { get; set; }
        public uint Hour { get; set; }
        public uint Minute { get; set; }
        public uint Second { get; set; }
        public static GRYDateTime GetCurrentDateTime()
        {
            return FromDateTime(DateTime.Now);
        }

        public static GRYDateTime GetCurrentDateTimeInUTC()
        {
            return FromDateTime(DateTime.UtcNow);
        }

        public GRYDateTime(uint year, uint month, uint day, uint hour, uint minute, uint second)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
            this.Hour = hour;
            this.Minute = minute;
            this.Second = second;
            this.ToDateTime();//check if datetime is valid
        }
        public readonly DateTime ToDateTime()
        {
            return new DateTime((int)this.Year, (int)this.Month, (int)this.Day, (int)this.Hour, (int)this.Minute, (int)this.Second);
        }

        public readonly GRYTime ToGRYTime()
        {
            return new GRYTime(this.Hour, this.Minute, this.Second);
        }

        public readonly GRYDate ToGRYDate()
        {
            return new GRYDate(this.Year, this.Month, this.Day);
        }

        public static GRYDateTime FromGRYDateAndTime(GRYDate date, GRYTime time)
        {
            return new GRYDateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }

        public static GRYDateTime FromDateTime(DateTime value)
        {
            return new GRYDateTime((uint)value.Year, (uint)value.Month, (uint)value.Day, (uint)value.Hour, (uint)value.Minute, (uint)value.Second);
        }

        public static GRYDateTime FromDateTime(DateTimeOffset value)
        {
            DateTimeOffset utcValue = value.ToUniversalTime();
            return new GRYDateTime((uint)utcValue.Year, (uint)utcValue.Month, (uint)utcValue.Day, (uint)utcValue.Hour, (uint)utcValue.Minute, (uint)utcValue.Second);
        }

        public static GRYDateTime FromString(string @string)
        {
            return FromDateTime(DateTime.ParseExact(@string, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None));
        }

        public override readonly int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override readonly string ToString()
        {
            return $"{this.Year.ToString().PadLeft(4, '0')}-{this.Month.ToString().PadLeft(2, '0')}-{this.Day.ToString().PadLeft(2, '0')} {this.Hour.ToString().PadLeft(2, '0')}:{this.Minute.ToString().PadLeft(2, '0')}:{this.Second.ToString().PadLeft(2, '0')}";
        }

        public readonly string ToURLEncodedString()
        {
            return this.ToString().Replace(" ", "%20").Replace(":", "%3A");
        }

        public override readonly bool Equals(object obj)
        {
            return obj is GRYDateTime time && this.Equals(time);
        }

        public readonly bool Equals(GRYDateTime other)
        {
            return this.Year == other.Year &&
                   this.Month == other.Month &&
                   this.Day == other.Day &&
                   this.Hour == other.Hour &&
                   this.Minute == other.Minute &&
                   this.Second == other.Second;
        }

        public readonly int CompareTo(GRYDateTime other)
        {
            return this.ToDateTime().CompareTo(other.ToDateTime());
        }

        public readonly int CompareTo(object obj)
        {
            return this.ToDateTime().CompareTo(obj);
        }

        public readonly GRYDateTime ToFullHour()
        {
            return new GRYDateTime(this.Year, this.Month, this.Day, this.Hour, 0, 0);
        }

        public readonly GRYDateTime AddDays(int amountOfDays)
        {
            return FromDateTime(this.ToDateTime().AddDays(amountOfDays));
        }

        public readonly GRYDateTime AddHours(int amountOfHours)
        {
            return FromDateTime(this.ToDateTime().AddHours(amountOfHours));
        }

        public readonly GRYDateTime AddMinutes(int amountOfMinuts)
        {
            return FromDateTime(this.ToDateTime().AddMinutes(amountOfMinuts));
        }

        public readonly GRYDateTime AddSeconds(int amountOfSeconds)
        {
            return FromDateTime(this.ToDateTime().AddSeconds(amountOfSeconds));
        }


        public static bool operator ==(GRYDateTime left, GRYDateTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GRYDateTime left, GRYDateTime right)
        {
            return !(left == right);
        }

        public static GRYDateTime operator +(GRYDateTime left, TimeSpan right)
        {
            return FromDateTime(left.ToDateTime() + right);
        }

        public static GRYDateTime operator -(GRYDateTime left, TimeSpan right)
        {
            return FromDateTime(left.ToDateTime() - right);
        }

        public static bool operator <(GRYDateTime left, GRYDateTime right)
        {
            return left.ToDateTime() < right.ToDateTime();
        }

        public static bool operator >(GRYDateTime left, GRYDateTime right)
        {
            return left.ToDateTime() > right.ToDateTime();
        }

        public static bool operator <=(GRYDateTime left, GRYDateTime right)
        {
            return left.ToDateTime() <= right.ToDateTime();
        }

        public static bool operator >=(GRYDateTime left, GRYDateTime right)
        {
            return left.ToDateTime() >= right.ToDateTime();
        }
    }
}
