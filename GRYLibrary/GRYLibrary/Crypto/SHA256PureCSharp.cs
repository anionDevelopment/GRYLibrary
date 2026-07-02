using GRYLibrary.Core.Misc;
using System;
using System.Linq;
using System.Text;

namespace GRYLibrary.Core.Crypto
{
    public class SHA256PureCSharp : HashAlgorithm
    {
        public override byte[] GetIdentifier()
        {
            return Utilities.PadLeft(Encoding.ASCII.GetBytes("SHA256PC#"), 10);
        }

        public uint[] H_InitialValue = [
            0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19,
        ];
        public uint[] K_InitialValue = [
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2,
        ];

        public override byte[] Hash(byte[] data)
        {
            uint[] H = [.. this.H_InitialValue];
            uint[] K = [.. this.K_InitialValue];

            byte[] message = data;
            int L_messageLengthInBits = data.Length * 8;

            message = [.. message, .. new byte[] { 128 }];

            int K_amountOfBitsToAppend = 512 - ((L_messageLengthInBits + 8 + 64) % 512);
            Utilities.AssertCondition(K_amountOfBitsToAppend % 8 == 0);
            int K_amountOfBytesToAppend = K_amountOfBitsToAppend / 8;
            message = [.. message, .. new byte[K_amountOfBytesToAppend]];
            Utilities.AssertCondition((L_messageLengthInBits + 8 + K_amountOfBitsToAppend + 64) % 512 == 0);

            message = [.. message, .. Utilities.UnsignedInteger64BitToByteArray((ulong)L_messageLengthInBits)];
            Utilities.AssertCondition(message.Length % 64 == 0);

            int chunkSizeInBits = 512;
            int chunkSizeInBytes = chunkSizeInBits / 8;
            int amountOfChunks = message.Length / chunkSizeInBytes;
            for (int chunkIndex = 0; chunkIndex < amountOfChunks; chunkIndex++)
            {
                byte[] currentChunk = [.. message.Skip(chunkIndex * chunkSizeInBytes).Take(chunkSizeInBytes)];
                Utilities.AssertCondition(currentChunk.Length == chunkSizeInBytes);
                uint[] W = new uint[64];
                uint[] currentChunkAsUnsignedIntegerArray = Utilities.ByteArrayToUnsignedInteger32BitArray(currentChunk);
                Utilities.AssertCondition(currentChunkAsUnsignedIntegerArray.Length == chunkSizeInBytes / 4);
                Array.Copy(currentChunkAsUnsignedIntegerArray, W, currentChunkAsUnsignedIntegerArray.Length);
                for (int i = 16; i < 64; i++)
                {
                    uint s0 = XOr(XOr(RightRotate(W[i - 15], 7), RightRotate(W[i - 15], 18)), RightShift(W[i - 15], 3));
                    uint s1 = XOr(XOr(RightRotate(W[i - 2], 17), RightRotate(W[i - 2], 19)), RightShift(W[i - 2], 10));
                    W[i] = Add(W[i - 16], s0, W[i - 7], s1);
                }

                uint a = H[0];
                uint b = H[1];
                uint c = H[2];
                uint d = H[3];
                uint e = H[4];
                uint f = H[5];
                uint g = H[6];
                uint h = H[7];
                for (int i = 0; i < 64; i++)
                {
                    uint S1 = CalculateS1(e);
                    uint ch = CalculateCh(e, f, g);//choose
                    uint temp1 = CalculateTemp1(h, S1, ch, K[i], W[i]);
                    uint S0 = CalculateS0(a);
                    uint maj = CalculateMaj(a, b, c);//majority
                    uint temp2 = CalculateTemp2(S0, maj);
                    h = g;
                    g = f;
                    f = e;
                    e = Add(d, temp1);
                    d = c;
                    c = b;
                    b = a;
                    a = Add(temp1, temp2);
                }
                H[0] = Add(a, H[0]);
                H[1] = Add(b, H[1]);
                H[2] = Add(c, H[2]);
                H[3] = Add(d, H[3]);
                H[4] = Add(e, H[4]);
                H[5] = Add(f, H[5]);
                H[6] = Add(g, H[6]);
                H[7] = Add(h, H[7]);

            }
            return
            [
                .. Utilities.UnsignedInteger32BitToByteArray(H[0])
,
                .. Utilities.UnsignedInteger32BitToByteArray(H[1]),
                .. Utilities.UnsignedInteger32BitToByteArray(H[2]),
                .. Utilities.UnsignedInteger32BitToByteArray(H[3]),
                .. Utilities.UnsignedInteger32BitToByteArray(H[4]),
                .. Utilities.UnsignedInteger32BitToByteArray(H[5]),
                .. Utilities.UnsignedInteger32BitToByteArray(H[6]),
                .. Utilities.UnsignedInteger32BitToByteArray(H[7]),
            ];
        }

        public static uint CalculateS1(uint e)
        {
            return XOr(XOr(RightRotate(e, 6), RightRotate(e, 11)), RightRotate(e, 25));
        }

        public static uint CalculateCh(uint e, uint f, uint g)
        {
            return XOr(And(e, f), And(Not(e), g));
        }

        public static uint CalculateTemp1(uint h, uint s1, uint ch, uint ki, uint wi)
        {
            return Add(h, s1, ch, ki, wi);
        }

        public static uint CalculateS0(uint a)
        {
            return XOr(XOr(RightRotate(a, 2), RightRotate(a, 13)), RightRotate(a, 22));
        }

        public static uint CalculateMaj(uint a, uint b, uint c)
        {
            return XOr(XOr(And(a, b), And(a, c)), And(b, c));
        }

        public static uint CalculateTemp2(uint s0, uint maj)
        {
            return Add(s0, maj);
        }

        public static uint Add(params uint[] summands)
        {
            uint result = 0;
            for (int i = 0; i < summands.Length; i++)
            {
                result += summands[i];
            }
            return result;
        }
        public static uint XOr(uint left, uint right)
        {
            return left ^ right;
        }

        public static uint RightShift(uint value, byte amountOfDigits)
        {
            return value >> amountOfDigits;
        }

        public static uint RightRotate(uint value, byte amountOfDigits)
        {
            Utilities.AssertCondition(amountOfDigits < 32);
            return (value >> amountOfDigits) | (value << (32 - amountOfDigits));
        }
        public static uint And(uint left, uint right)
        {
            return left & right;
        }

        public static uint Not(uint value)
        {
            return ~value;
        }
    }
}