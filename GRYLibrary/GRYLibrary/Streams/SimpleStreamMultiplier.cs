using System.Collections.Generic;
using System.IO;

namespace GRYLibrary.Core.Streams
{
    public class SimpleStreamMultiplier
    {
        public Stream Source { get; set; }
        public IList<Stream> Destinations { get; } = [];
        public int Read(int maximalAmountOfBytesToRead)
        {
            byte[] buffer = new byte[maximalAmountOfBytesToRead];
            int amountOfReadBytes = this.Source.Read(buffer, 0, buffer.Length);
            if (0 < amountOfReadBytes)
            {
                foreach (Stream destination in this.Destinations)
                {
                    destination.Write(buffer, 0, amountOfReadBytes);
                }
            }
            return amountOfReadBytes;
        }
    }
}