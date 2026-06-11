using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorSchemeInverter
{
    public static class InverterEngine
    {
        private const int Threshold = 10;

        private static bool IsGrayscalePixel(int r, int g, int b)
        {
            return Math.Abs(r - g) <= Threshold &&
                   Math.Abs(r - b) <= Threshold &&
                   Math.Abs(g - b) <= Threshold;
        }

        public static byte[] TransformDibBytes(byte[] dib)
        {
            if (dib.Length < 40)
            {
                return dib;
            }

            var headerSize = BitConverter.ToInt32(dib, 0);
            if (headerSize < 40)
            {
                Logger.Info($"Unsupported DIB header size: {headerSize}. Skipping translation.");
                return dib;
            }

            var width = BitConverter.ToInt32(dib, 4);
            var storedHeight = BitConverter.ToInt32(dib, 8);
            var bitCount = BitConverter.ToUInt16(dib, 14);

            // Real image height is half the stored height because of stacking masks
            var height = Math.Abs(storedHeight) / 2;

            if (bitCount != 32)
            {
                Logger.Info($"Skipping non-32bpp format context ({width}x{height}, {bitCount}bpp)");
                return dib;
            }

            var pixelOffset = headerSize;
            var pixelBytes = width * height * 4;

            if (pixelOffset + pixelBytes > dib.Length)
            {
                Logger.Warning("Pixel tracking out of bounds context. Array skipped.");
                return dib;
            }

            var end = pixelOffset + pixelBytes;
            for (var p = pixelOffset; p < end; p += 4)
            {
                var b = dib[p];
                var g = dib[p + 1];
                var r = dib[p + 2];

                if (IsGrayscalePixel(r, g, b))
                {
                    dib[p] = (byte)(255 - b);
                    dib[p + 1] = (byte)(255 - g);
                    dib[p + 2] = (byte)(255 - r);
                }
            }

            return dib;
        }

        public static byte[] ConvertCurBytes(byte[] bytes)
        {
            if (bytes.Length < 6) return bytes;

            var count = BitConverter.ToUInt16(bytes, 4);

            // Loop through directory frames tracking target references
            for (var i = 0; i < count; i++)
            {
                var offset = 6 + (i * 16);
                if (offset + 16 > bytes.Length) break;

                var size = BitConverter.ToUInt32(bytes, offset + 8);
                var imageOffset = BitConverter.ToUInt32(bytes, offset + 12);

                if (imageOffset + size > bytes.Length) continue;

                // Extract frame payload primitive
                var img = new byte[size];
                Buffer.BlockCopy(bytes, (int)imageOffset, img, 0, (int)size);

                // Run structural mutation block directly
                TransformDibBytes(img);

                // Re-write transformed data context securely back into reference array
                Buffer.BlockCopy(img, 0, bytes, (int)imageOffset, (int)size);
            }

            return bytes;
        }

        public static byte[] ConvertAniBytes(byte[] bytes)
        {
            if (bytes.Length < 12 || BitConverter.ToUInt32(bytes, 0) != 0x46464952) // 'RIFF'
            {
                return bytes;
            }

            var offset = 12;
            var fileLength = bytes.Length;

            while (offset + 8 <= fileLength)
            {
                var chunkId = BitConverter.ToUInt32(bytes, offset);
                var chunkSize = BitConverter.ToUInt32(bytes, offset + 4);
                var payloadStart = offset + 8;
                var paddedChunkSize = chunkSize + (chunkSize % 2);

                if (chunkId == 0x6E6F6369) // 'icon'
                {
                    if (payloadStart + chunkSize <= fileLength)
                    {
                        var curBytes = new byte[chunkSize];
                        Buffer.BlockCopy(bytes, payloadStart, curBytes, 0, (int)chunkSize);

                        ConvertCurBytes(curBytes);

                        Buffer.BlockCopy(curBytes, 0, bytes, payloadStart, (int)chunkSize);
                    }
                }
                else if (chunkId == 0x5453494C) // 'LIST'
                {
                    offset = payloadStart + 4;
                    continue;
                }

                offset = payloadStart + (int)paddedChunkSize;
            }

            return bytes;
        }
    }
}