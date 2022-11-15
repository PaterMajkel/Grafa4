using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Grafa4;

public enum ProcessType
{
    Adding,
    Substracting,
    Multiplying,
    Dividing
}
public static class Algorithm
{

    public static Bitmap RgbOperation(Bitmap bitmap, double[] rgb, ProcessType type = ProcessType.Adding)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        byte[] vs = new byte[data.Height * data.Stride];
        Marshal.Copy(data.Scan0, vs, 0, vs.Length);

        byte MakeOperation(byte originalValue, double value, ProcessType type)
        {
            return (byte)(type switch
            {
                ProcessType.Substracting => (originalValue - value) < 0 ? 0 : Math.Floor(originalValue - value),
                ProcessType.Multiplying => (originalValue * value) / 255 > 255 ? 255 : Math.Floor((originalValue * value) / 255),
                ProcessType.Dividing => (originalValue / value) > 255 ? 255 : Math.Floor(originalValue / value),
                _ => (originalValue + value) > 255 ? 255 : Math.Floor(originalValue + value),
            });
        }

        for (int i = 0; i < bitmap.Height; i++)
        {
            for (int j = 0; j < bitmap.Width * 3; j += 3)
            {
                vs[i * bitmap.Width * 3 + j] = MakeOperation(vs[i * bitmap.Width * 3 + j], rgb[2], type);
                vs[i * bitmap.Width * 3 + j + 1] = MakeOperation(vs[i * bitmap.Width * 3 + j + 1], rgb[1], type);
                vs[i * bitmap.Width * 3 + j + 2] = MakeOperation(vs[i * bitmap.Width * 3 + j + 2], rgb[0], type);

            }
        }
        Marshal.Copy(vs, 0, data.Scan0, vs.Length);
        bitmap.UnlockBits(data);

        return bitmap;
    }

    public static Bitmap AdjustBrightness(Bitmap Image, int Value)
    {
        System.Drawing.Bitmap TempBitmap = Image;
        float FinalValue = (float)Value / 255.0f;
        System.Drawing.Bitmap NewBitmap = new System.Drawing.Bitmap(TempBitmap.Width, TempBitmap.Height);
        System.Drawing.Graphics NewGraphics = System.Drawing.Graphics.FromImage(NewBitmap);
        float[][] FloatColorMatrix ={
                     new float[] {1, 0, 0, 0, 0},
                     new float[] {0, 1, 0, 0, 0},
                     new float[] {0, 0, 1, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
                 };
        System.Drawing.Imaging.ColorMatrix NewColorMatrix = new ColorMatrix(FloatColorMatrix);
        System.Drawing.Imaging.ImageAttributes Attributes = new ImageAttributes();
        Attributes.SetColorMatrix(NewColorMatrix);
        NewGraphics.DrawImage(TempBitmap, new System.Drawing.Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), 0, 0, TempBitmap.Width, TempBitmap.Height, System.Drawing.GraphicsUnit.Pixel, Attributes);
        Attributes.Dispose();
        NewGraphics.Dispose();
        return NewBitmap;
    }

    private static int Compare(byte[] a, byte[] b)
    {
        if (a[0] + a[1] + a[2] > b[0] + b[1] + b[2])
            return 1;
        return 0;
    }

    public static Bitmap LocalMask(Bitmap bitmap, int pixelSize, bool Mean = true)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        byte[] vs = new byte[data.Height * data.Stride];
        Marshal.Copy(data.Scan0, vs, 0, vs.Length);



        for (int i = 0; i < bitmap.Height; i += pixelSize)
        {
            for (int j = 0; j < bitmap.Width * 3; j += pixelSize * 3)
            {
                if ((i + pixelSize / 2) * bitmap.Width * 3 + j + pixelSize * 3 / 2 + 2 >= vs.Length)
                    break;
                List<byte[]> list = new();
                for (int k = i; k <= i + pixelSize; k++)
                {
                    for (int l = j; l <= j + pixelSize * 3; l += 3)
                    {
                        if (k * bitmap.Width * 3 + l + 2 >= vs.Length)
                            break;
                        list.Add(new byte[] { vs[k * bitmap.Width * 3 + l], vs[k * bitmap.Width * 3 + l + 1], vs[k * bitmap.Width * 3 + l + 2] });
                    }
                }
                byte[] value = { 0, 0, 0 };

                if (Mean)
                {
                    value = new byte[] { (byte)(list.Sum(p => p[0]) / list.Count), (byte)(list.Sum(p => p[1]) / list.Count), (byte)(list.Sum(p => p[2]) / list.Count) };
                }
                else
                {
                    list.Sort(Compare);
                    value = list[list.Count / 2];
                }
                for (int k = i; k <= i + pixelSize; k++)
                {
                    for (int l = j; l <= j + pixelSize * 3; l += 3)
                    {
                        if (k * bitmap.Width * 3 + l + 2 >= vs.Length)
                            break;
                        vs[k * bitmap.Width * 3 + l] = value[0];
                        vs[k * bitmap.Width * 3 + l + 1] = value[1];
                        vs[k * bitmap.Width * 3 + l + 2] = value[2];
                    }
                }
            }
        }
        Marshal.Copy(vs, 0, data.Scan0, vs.Length);
        bitmap.UnlockBits(data);

        return bitmap;
    }

    public static Bitmap LinearFilter(Bitmap bitmap, double[,] xFilterMatrix,
                                    double[,]? yFilterMatrix, double bias = 1, bool grayscale = false)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
        byte[] pixelBuffer = new byte[data.Stride *
                                  bitmap.Height];

        byte[] resultBuffer = new byte[data.Stride *
                                       data.Height];

        Marshal.Copy(data.Scan0, pixelBuffer, 0,
                                   pixelBuffer.Length);
        bitmap.UnlockBits(data);

        if (grayscale == true)
        {
            float rgb = 0;


            for (int k = 0; k < pixelBuffer.Length; k += 4)
            {
                rgb = pixelBuffer[k] * 0.11f;
                rgb += pixelBuffer[k + 1] * 0.59f;
                rgb += pixelBuffer[k + 2] * 0.3f;


                pixelBuffer[k] = (byte)rgb;
                pixelBuffer[k + 1] = pixelBuffer[k];
                pixelBuffer[k + 2] = pixelBuffer[k];
                pixelBuffer[k + 3] = 255;
            }
        }
        double blueX = 0.0;
        double greenX = 0.0;
        double redX = 0.0;

        double blueY = 0.0;
        double greenY = 0.0;
        double redY = 0.0;

        double blueTotal = 0.0;
        double greenTotal = 0.0;
        double redTotal = 0.0;

        int filterOffset = xFilterMatrix.GetUpperBound(0)/2;
        int calcOffset = 0;

        int byteOffset = 0;

        for (int offsetY = filterOffset; offsetY < bitmap.Height - filterOffset - 1; offsetY++)
        {
            for (int offsetX = filterOffset; offsetX < bitmap.Width - filterOffset - 1; offsetX++)
            {
                blueX = greenX = redX = 0;
                blueY = greenY = redY = 0;
                blueTotal = greenTotal = redTotal = 0.0;
                byteOffset = offsetY *
                             data.Stride +
                             offsetX * 4;

                for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                {
                    for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                    {
                        calcOffset = byteOffset + filterX * 4 + filterY * data.Stride;

                        blueX += (double)pixelBuffer[calcOffset] * xFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                        greenX += (double)pixelBuffer[calcOffset + 1] * xFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                        redX += (double)pixelBuffer[calcOffset + 2] * xFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                        if (yFilterMatrix != null)
                        {
                            blueY += (double)pixelBuffer[calcOffset] * yFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                            greenY += (double)pixelBuffer[calcOffset + 1] * yFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                            redY += (double)pixelBuffer[calcOffset + 2] * yFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }
                }
                if (yFilterMatrix != null)
                {
                    blueTotal = Math.Sqrt(blueX * blueX + blueY * blueY);
                    greenTotal = Math.Sqrt(greenX * greenX + greenY * greenY);
                    redTotal = Math.Sqrt(redX * redX + redY * redY);
                }
                else
                {
                    blueTotal = bias * blueX;
                    greenTotal = bias * greenX;
                    redTotal = bias * redX;
                }


                if (blueTotal > 255)
                    blueTotal = 255;
                else if (blueTotal < 0)
                    blueTotal = 0;

                if (greenTotal > 255)
                    greenTotal = 255;
                else if (greenTotal < 0)
                    greenTotal = 0;

                if (redTotal > 255)
                    redTotal = 255;
                else if (redTotal < 0)
                    redTotal = 0;

                resultBuffer[byteOffset] = (byte)(blueTotal);
                resultBuffer[byteOffset + 1] = (byte)(greenTotal);
                resultBuffer[byteOffset + 2] = (byte)(redTotal);
                resultBuffer[byteOffset + 3] = 255;
            }
        }

        Bitmap resultBitmap = new Bitmap(bitmap.Width,
                                         bitmap.Height);

        BitmapData data2 = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

        Marshal.Copy(resultBuffer, 0, data2.Scan0,
                                   resultBuffer.Length);
        resultBitmap.UnlockBits(data2);

        return resultBitmap;
    }

    public static Bitmap GrayScale1(Bitmap bitmap)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
        byte[] pixelBuffer = new byte[data.Stride *
                                  bitmap.Height];

        Marshal.Copy(data.Scan0, pixelBuffer, 0,
                                   pixelBuffer.Length);
        bitmap.UnlockBits(data);

            float rgb = 0;


            for (int k = 0; k < pixelBuffer.Length; k += 4)
            {
                rgb = pixelBuffer[k] * 0.11f;
                rgb += pixelBuffer[k + 1] * 0.59f;
                rgb += pixelBuffer[k + 2] * 0.3f;


                pixelBuffer[k] = (byte)rgb;
                pixelBuffer[k + 1] = pixelBuffer[k];
                pixelBuffer[k + 2] = pixelBuffer[k];
                pixelBuffer[k + 3] = 255;
            }

        Bitmap resultBitmap = new Bitmap(bitmap.Width,
                                     bitmap.Height);

        BitmapData data2 = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

        Marshal.Copy(pixelBuffer, 0, data2.Scan0,
                                   pixelBuffer.Length);
        resultBitmap.UnlockBits(data2);

        return resultBitmap;
    }

    public static Bitmap GrayScale2(Bitmap bitmap)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        byte[] pixelBuffer = new byte[data.Stride *
                                  bitmap.Height];

        Marshal.Copy(data.Scan0, pixelBuffer, 0,
                                   pixelBuffer.Length);
        bitmap.UnlockBits(data);

        float rgb = 0;


        for (int k = 0; k < pixelBuffer.Length; k += 3)
        {
            rgb = (pixelBuffer[k] + pixelBuffer[k + 1] + pixelBuffer[k + 2])/3;

            pixelBuffer[k] = 
            pixelBuffer[k + 1] =
            pixelBuffer[k + 2] = (byte)rgb;
        }

        Bitmap resultBitmap = new Bitmap(bitmap.Width,
                                     bitmap.Height);

        BitmapData data2 = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        Marshal.Copy(pixelBuffer, 0, data2.Scan0,
                                   pixelBuffer.Length);
        resultBitmap.UnlockBits(data2);

        return resultBitmap;
    }

}
