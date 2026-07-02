using System;

namespace GRYLibrary.Core.Misc
{
    public class ColorGradient
    {
        public ExtendedColor StartColor { get; private set; }
        public ExtendedColor DestinationColor { get; private set; }

        public ColorGradient(ExtendedColor startColor, ExtendedColor destinationColor)
        {
            this.DestinationColor = destinationColor;
            this.StartColor = startColor;
        }
        public ExtendedColor GetColorGradientValue(double gradient)
        {
            return this.GetColorGradientValue(new PercentValue((decimal)gradient));
        }

        public ExtendedColor GetColorGradientValue(PercentValue percentValue)
        {
            byte a = this.GetGradient(this.StartColor.A, this.DestinationColor.A, percentValue);
            byte r = this.GetGradient(this.StartColor.R, this.DestinationColor.R, percentValue);
            byte g = this.GetGradient(this.StartColor.G, this.DestinationColor.G, percentValue);
            byte b = this.GetGradient(this.StartColor.B, this.DestinationColor.B, percentValue);
            return new ExtendedColor(a, r, g, b);
        }

        private byte GetGradient(byte startValue, byte destinationValue, PercentValue gradient)
        {
            return (byte)Math.Round(startValue + ((destinationValue - startValue) * gradient.Value), 0);
        }
    }
}