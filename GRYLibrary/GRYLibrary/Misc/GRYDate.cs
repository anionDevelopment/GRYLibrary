using System;
using System.Globalization;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represents a date (without any time-information) in the gregorian calendar.
    /// </summary>
    public struct GRYDate : IEquatable<GRYDate>, IComparable<GRYDate>, IComparable
    {
        public static readonly string DateFormat = "yyyy-MM-dd";
        public uint Year { get; set; }
        public uint Month { get; set; }
        public uint Day { get; set; }
        public static GRYDate GetCurrentDate()
        {
            return GRYDateTime.GetCurrentDateTime().ToGRYDate();
        }

        public static GRYDate GetCurrentDateInUTC()
        {
            return GRYDateTime.GetCurrentDateTimeInUTC().ToGRYDate();
        }

        public GRYDate(uint year, uint month, uint day)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
            this.ToDate();//check if date is valid
        }
        public readonly DateOnly ToDate()
        {
            return new DateOnly((int)this.Year, (int)this.Month, (int)this.Day);
        }

        public static GRYDate FromDate(DateOnly value)
        {
            return new GRYDate((uint)value.Year, (uint)value.Month, (uint)value.Day);
        }

        public static GRYDate FromString(string @string)
        {
            return FromDate(DateOnly.ParseExact(@string, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None));
        }

        public override readonly int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override readonly string ToString()
        {
            return $"{this.Year.ToString().PadLeft(4, '0')}-{this.Month.ToString().PadLeft(2, '0')}-{this.Day.ToString().PadLeft(2, '0')}";
        }

        public override readonly bool Equals(object obj)
        {
            return obj is GRYDate time && this.Equals(time);
        }

        public readonly bool Equals(GRYDate other)
        {
            return this.Year == other.Year &&
                   this.Month == other.Month &&
                   this.Day == other.Day;
        }

        public readonly int CompareTo(GRYDate other)
        {
            return this.ToDate().CompareTo(other.ToDate());
        }

        public readonly int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is GRYDate other)
            {
                return this.CompareTo(other);
            }
            throw new ArgumentException($"Object must be of type {nameof(GRYDate)}.", nameof(obj));
        }

        public readonly GRYDate AddDays(int amountOfDays)
        {
            return FromDate(this.ToDate().AddDays(amountOfDays));
        }

        public static bool operator ==(GRYDate left, GRYDate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GRYDate left, GRYDate right)
        {
            return !(left == right);
        }

        public static bool operator <(GRYDate left, GRYDate right)
        {
            return left.ToDate() < right.ToDate();
        }
        public static bool operator >(GRYDate left, GRYDate right)
        {
            return left.ToDate() > right.ToDate();
        }
        public static bool operator <=(GRYDate left, GRYDate right)
        {
            return left.ToDate() <= right.ToDate();
        }
        public static bool operator >=(GRYDate left, GRYDate right)
        {
            return left.ToDate() >= right.ToDate();
        }
    }
}
