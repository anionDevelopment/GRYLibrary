using System;
using System.Numerics;

namespace GRYLibrary.Core.Crypto
{
    public readonly struct Point : IEquatable<Point>
    {
        public static Point Infinity { get; } = new Point(default, default, default);
        public Curve UsedCurve { get; }
        public BigInteger X { get; }
        public BigInteger Y { get; }
        public Point(BigInteger x, BigInteger y, Curve usedCurve)
        {
            this.X = x;
            this.Y = y;
            this.UsedCurve = usedCurve;
        }

        public override bool Equals(object obj)
        {
            return obj is Point point && this.Equals(point);
        }

        public bool Equals(Point other)
        {
            return this.X.Equals(other.X) &&
                   this.Y.Equals(other.Y);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.X, this.Y);
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }
        public static Point operator +(Point left, Point right)
        {
            throw new NotImplementedException();//see https://en.wikipedia.org/wiki/Elliptic_curve_point_multiplication#Point_addition
        }
        public Point Double()
        {
            return this * 2;
        }

        public static Point operator *(Point left, long right)
        {
            return left * new BigInteger(right);
        }
        public static Point operator *(Point left, BigInteger right)
        {
            throw new NotImplementedException();//see https://en.wikipedia.org/wiki/Elliptic_curve_point_multiplication#Point_multiplication
        }
        public static Point operator !(Point value)
        {
            throw new NotImplementedException();//see https://en.wikipedia.org/wiki/Elliptic_curve_point_multiplication#Point_negation
        }
        private static BigInteger InverseMod(BigInteger left, BigInteger right)
        {
            throw new NotImplementedException();
        }

        private static BigInteger Mod(BigInteger left, BigInteger right)
        {
            throw new NotImplementedException();
        }
    }
}