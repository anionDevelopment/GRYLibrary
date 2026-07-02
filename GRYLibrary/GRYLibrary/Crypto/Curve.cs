using System;
using System.Globalization;
using System.Numerics;

namespace GRYLibrary.Core.Crypto
{
    public readonly struct Curve
    {
        public static Curve Curve25519 { get; } = new Curve(
            (x) => BigInteger.Pow(x, 3) + (486662 * x * x) + x,
            BigInteger.Pow(2, 255) - 19,
            BigInteger.Parse("15112221349535400772501151409588531511454012693041857206046113283949847762202")
        );
        public static Curve Secp256k1 { get; } = new Curve(
            (x) => BigInteger.Pow(x, 3) + 7,
            BigInteger.Pow(2, 256) - BigInteger.Pow(2, 32) - BigInteger.Pow(2, 9) - BigInteger.Pow(2, 8) - BigInteger.Pow(2, 7) - BigInteger.Pow(2, 6) - BigInteger.Pow(2, 4) - 1,
            BigInteger.Parse("0279BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798", NumberStyles.AllowHexSpecifier)
        );

        public Func<BigInteger, BigInteger> CurveFunction { get; }
        public BigInteger Field { get; }
        public BigInteger BasePointX { get; }
        public Curve(Func<BigInteger, BigInteger> curveFunction, BigInteger field, BigInteger basePointX)
        {
            this.CurveFunction = curveFunction;
            this.Field = field;
            this.BasePointX = basePointX;
        }
    }
}