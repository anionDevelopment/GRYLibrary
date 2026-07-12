using System;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represents a <see cref="decimal"/>-number between 0 and 1.
    /// </summary>
    public readonly struct PercentValue
    {
        public static PercentValue ZeroPercent { get; } = new PercentValue((decimal)0);
        public static PercentValue HundredPercent { get; } = new PercentValue((decimal)1);
        private static readonly Random _Random = new Random();
        public int ValueInPercent => (int)Math.Round(this.Value * 100);
        public decimal Value { get; }

        public PercentValue(double value) : this((decimal)value)
        {
        }
        public PercentValue(decimal value)
        {
            if (value < 0)
            {
                this.Value = 0;
            }
            else if (value > 1)
            {
                this.Value = 1;
            }
            else
            {
                this.Value = value;
            }
        }
        public static PercentValue CreateByPercentValue(int percentValue)
        {
            if (percentValue < 0)
            {
                return ZeroPercent;
            }
            else if (percentValue > 100)
            {
                return HundredPercent;
            }
            else
            {
                return new PercentValue((decimal)percentValue / 100);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is PercentValue value && this.Value == value.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Value);
        }

        public override string ToString()
        {
            return GUtilities.DecimalToString(this.Value);
        }

        public static PercentValue Random()
        {
            return new PercentValue(_Random.NextDouble());
        }

        #region Operators
        public static bool operator ==(PercentValue left, PercentValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PercentValue left, PercentValue right)
        {
            return !(left == right);
        }

        public static bool operator ==(PercentValue left, int right)
        {
            return left.Value == right;
        }
        public static bool operator !=(PercentValue left, int right)
        {
            return left.Value != right;
        }
        public static bool operator ==(int left, PercentValue right)
        {
            return left == right.Value;
        }

        public static bool operator !=(int left, PercentValue right)
        {
            return left != right.Value;
        }
        public static bool operator ==(PercentValue left, double right)
        {
            return left.Value == (decimal)right;
        }

        public static bool operator !=(PercentValue left, double right)
        {
            return left.Value != (decimal)right;
        }
        public static bool operator ==(double left, PercentValue right)
        {
            return (decimal)left == right.Value;
        }
        public static bool operator !=(double left, PercentValue right)
        {
            return (decimal)left != right.Value;
        }
        public static bool operator ==(PercentValue left, decimal right)
        {
            return left.Value == right;
        }
        public static bool operator !=(PercentValue left, decimal right)
        {
            return left.Value != right;
        }
        public static bool operator ==(decimal left, PercentValue right)
        {
            return left == right.Value;
        }

        public static bool operator !=(decimal left, PercentValue right)
        {
            return left != right.Value;
        }
        public static bool operator <(PercentValue left, PercentValue right)
        {
            return left.Value < right.Value;
        }
        public static bool operator >(PercentValue left, PercentValue right)
        {
            return left.Value > right.Value;
        }
        public static bool operator <=(PercentValue left, PercentValue right)
        {
            return left.Value <= right.Value;
        }
        public static bool operator >=(PercentValue left, PercentValue right)
        {
            return left.Value >= right.Value;
        }
        public static bool operator <(PercentValue left, double right)
        {
            return left.Value < (decimal)right;
        }
        public static bool operator >(PercentValue left, double right)
        {
            return left.Value > (decimal)right;
        }
        public static bool operator <=(PercentValue left, double right)
        {
            return left.Value <= (decimal)right;
        }
        public static bool operator >=(PercentValue left, double right)
        {
            return left.Value >= (decimal)right;
        }
        public static bool operator <(double left, PercentValue right)
        {
            return (decimal)left < right.Value;
        }
        public static bool operator >(double left, PercentValue right)
        {
            return (decimal)left > right.Value;
        }
        public static bool operator <=(double left, PercentValue right)
        {
            return (decimal)left <= right.Value;
        }
        public static bool operator >=(double left, PercentValue right)
        {
            return (decimal)left >= right.Value;
        }
        public static bool operator <(PercentValue left, decimal right)
        {
            return left.Value < right;
        }
        public static bool operator >(PercentValue left, decimal right)
        {
            return left.Value > right;
        }
        public static bool operator <=(PercentValue left, decimal right)
        {
            return left.Value <= right;
        }
        public static bool operator >=(PercentValue left, decimal right)
        {
            return left.Value >= right;
        }
        public static bool operator <(decimal left, PercentValue right)
        {
            return left < right.Value;
        }
        public static bool operator >(decimal left, PercentValue right)
        {
            return left > right.Value;
        }
        public static bool operator <=(decimal left, PercentValue right)
        {
            return left <= right.Value;
        }
        public static bool operator >=(decimal left, PercentValue right)
        {
            return left >= right.Value;
        }
        public static PercentValue operator +(PercentValue left, PercentValue right)
        {
            return new PercentValue(left.Value + right.Value);
        }
        public static PercentValue operator -(PercentValue left, PercentValue right)
        {
            return new PercentValue(left.Value - right.Value);
        }
        public static PercentValue operator *(PercentValue left, PercentValue right)
        {
            return new PercentValue(left.Value * right.Value);
        }
        public static PercentValue operator /(PercentValue left, PercentValue right)
        {
            return new PercentValue(left.Value / right.Value);
        }
        public static PercentValue operator +(PercentValue left, double right)
        {
            return new PercentValue(left.Value + (decimal)right);
        }
        public static PercentValue operator -(PercentValue left, double right)
        {
            return new PercentValue(left.Value - (decimal)right);
        }
        public static PercentValue operator *(PercentValue left, double right)
        {
            return new PercentValue(left.Value * (decimal)right);
        }
        public static PercentValue operator /(PercentValue left, double right)
        {
            return new PercentValue(left.Value * (decimal)right);
        }
        public static PercentValue operator +(double left, PercentValue right)
        {
            return new PercentValue((decimal)left + right.Value);
        }
        public static PercentValue operator -(double left, PercentValue right)
        {
            return new PercentValue((decimal)left - right.Value);
        }
        public static PercentValue operator *(double left, PercentValue right)
        {
            return new PercentValue((decimal)left * right.Value);
        }
        public static PercentValue operator /(double left, PercentValue right)
        {
            return new PercentValue((decimal)left * right.Value);
        }
        public static PercentValue operator +(PercentValue left, decimal right)
        {
            return new PercentValue(left.Value + right);
        }
        public static PercentValue operator -(PercentValue left, decimal right)
        {
            return new PercentValue(left.Value - right);
        }
        public static PercentValue operator *(PercentValue left, decimal right)
        {
            return new PercentValue(left.Value * right);
        }
        public static PercentValue operator /(PercentValue left, decimal right)
        {
            return new PercentValue(left.Value / right);
        }
        public static PercentValue operator +(decimal left, PercentValue right)
        {
            return new PercentValue(left + right.Value);
        }
        public static PercentValue operator -(decimal left, PercentValue right)
        {
            return new PercentValue(left - right.Value);
        }
        public static PercentValue operator *(decimal left, PercentValue right)
        {
            return new PercentValue(left * right.Value);
        }
        public static PercentValue operator /(decimal left, PercentValue right)
        {
            return new PercentValue(left / right.Value);
        }
        #endregion
    }
}