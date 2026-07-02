
using System;
using System.Drawing;

namespace GRYLibrary.Core.Misc
{
    public class ExtendedColor
    {
        public static readonly ExtendedColor Black = new(255, 0, 0, 0);
        public static readonly ExtendedColor White = new(255, 255, 255, 255);
        public static readonly ExtendedColor Transparency = new(0, 0, 0, 0);
        private readonly string _ARGBStringValue;
        private readonly string _RGBStringValue;
        public byte A { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public Color DrawingColor { get; }
        public (double hue, PercentValue saturation, PercentValue value) HSV { get; }
        public int ColorCode { get; }

        public ExtendedColor() : this(0)
        {
        }
        public ExtendedColor(double hue, PercentValue saturation, PercentValue value) : this(ColorFromHSV(hue, saturation, value).ToArgb())
        {
        }
        public ExtendedColor(byte a, byte r, byte g, byte b) : this(Color.FromArgb(a, r, g, b).ToArgb())
        {
        }
        public ExtendedColor(byte r, byte g, byte b) : this(255, r, g, b)
        {
        }
        public ExtendedColor(int colorCode)
        {
            this.ColorCode = colorCode;
            this.DrawingColor = Color.FromArgb(this.ColorCode);
            this._ARGBStringValue = this.ColorCode.ToString("X8");
            this._RGBStringValue = this.DrawingColor.R.ToString("X2") + this.DrawingColor.G.ToString("X2") + this.DrawingColor.B.ToString("X2");
            this.A = this.DrawingColor.A;
            this.R = this.DrawingColor.R;
            this.G = this.DrawingColor.G;
            this.B = this.DrawingColor.B;
            this.HSV = ColorToHSV(this.DrawingColor);
        }

        public string GetARGBString(bool withNumberSign = false)
        {
            if (withNumberSign)
            {
                return "#" + this._ARGBStringValue;
            }
            else
            {
                return this._ARGBStringValue;
            }
        }
        public string GetRGBString(bool withNumberSign = false)
        {
            if (withNumberSign)
            {
                return "#" + this._RGBStringValue;
            }
            else
            {
                return this._RGBStringValue;
            }
        }
        public override bool Equals(object @object)
        {
            if (@object is ExtendedColor color)
            {
                return this.ColorCode == color.ColorCode;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return this.ColorCode;
        }

        public override string ToString()
        {
            return $"{nameof(ExtendedColor)}({nameof(this.A)}={this.A},{nameof(this.R)}={this.R},{nameof(this.G)}={this.G},{nameof(this.B)}={this.B})";
        }

        private static (double hue, PercentValue saturation, PercentValue value) ColorToHSV(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            double hue = color.GetHue();
            double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            double value = max / 255d;
            return (hue, new PercentValue(saturation), new PercentValue(value));
        }
        private static Color ColorFromHSV(double hue, PercentValue saturation, PercentValue value)
        {
            double dSaturation = decimal.ToDouble(saturation.Value);
            double dValue = decimal.ToDouble(value.Value);
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = (hue / 60) - Math.Floor(hue / 60);

            dValue = dValue * 255;
            int v = Convert.ToInt32(dValue);
            int p = Convert.ToInt32(dValue * (1 - dSaturation));
            int q = Convert.ToInt32(dValue * (1 - (f * dSaturation)));
            int t = Convert.ToInt32(dValue * (1 - ((1 - f) * dSaturation)));

            if (hi == 0)
            {
                return Color.FromArgb(255, v, t, p);
            }
            else if (hi == 1)
            {
                return Color.FromArgb(255, q, v, p);
            }
            else if (hi == 2)
            {
                return Color.FromArgb(255, p, v, t);
            }
            else if (hi == 3)
            {
                return Color.FromArgb(255, p, q, v);
            }
            else if (hi == 4)
            {
                return Color.FromArgb(255, t, p, v);
            }
            else
            {
                return Color.FromArgb(255, v, p, q);
            }
        }
    }
}