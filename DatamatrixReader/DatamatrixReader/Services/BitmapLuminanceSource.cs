using System.Drawing.Imaging;
using System.Drawing;
using ZXing;

namespace DatamatrixReader.Services
{
    // Bitmap'i LuminanceSource'a dönüştürme işlemini yapan sınıf
    public class BitmapLuminanceSource: BaseLuminanceSource
    {
        public BitmapLuminanceSource(Bitmap bitmap) : base(bitmap.Width, bitmap.Height)
        {
            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

            int stride = bitmapData.Stride;
            int width = bitmap.Width;
            int height = bitmap.Height;
            int strideDifference = stride - (width * 4);

            byte[] pixelData = new byte[stride * height];
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixelData, 0, pixelData.Length);
            bitmap.UnlockBits(bitmapData);

            var luminanceData = new byte[width * height];

            for (int y = 0; y < height; y++)
            {
                int offset = y * width;
                int strideOffset = y * stride;

                for (int x = 0; x < width; x++)
                {
                    int pixelOffset = strideOffset + (x * 4);
                    luminanceData[offset + x] = (byte)((pixelData[pixelOffset + 2] + pixelData[pixelOffset + 1] + pixelData[pixelOffset]) / 3);
                }
            }

            this.luminances = luminanceData;
        }

        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            return new BitmapLuminanceSource(newLuminances, width, height);
        }

        private BitmapLuminanceSource(byte[] luminances, int width, int height) : base(width, height)
        {
            this.luminances = luminances;
        }
    }
}
