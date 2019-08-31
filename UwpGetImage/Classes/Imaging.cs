using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;

namespace UwpGetImage.Classes
{
    public static class Imaging
    {

        public static async Task<StorageFile> WriteableBitmapToStorageFile(ushort[,] image, bool isScaleValues, List<KeyValuePair<string, string>> metadata)
        {
            //Setup image maxVal.
            var imgHeight = image.GetLength(0);
            var imgWidth = image.GetLength(1);
            float maxVal = 1;
            if (isScaleValues)
            {
                for (int i = 0; i < imgHeight; i++)
                {
                    for (int j = 0; j < imgWidth; j++)
                    {
                        if (maxVal < image[i, j])
                        {
                            maxVal = image[i, j];
                        }
                    }
                }
            }

            string FileName = "MyFile.png";
            var file =
                await
                    Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(FileName,
                        CreationCollisionOption.GenerateUniqueName);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                ImageInfo imgInfo = new ImageInfo(imgWidth, imgHeight, 16, false, true, false);
                PngWriter wrt = new PngWriter(stream.AsStreamForWrite(), imgInfo);
                PngMetadata da = wrt.GetMetadata();
                foreach (var item in metadata)
                    if (item.Value != null)
                        da.SetText(item.Key, item.Value);
                ImageLines imLines = new ImageLines(imgInfo, ImageLine.ESampleType.INT, false, 0, imgHeight, 1);
                for (int i = 0; i < imLines.ImgInfo.Rows; i++)
                    for (int j = 0; j < imLines.ImgInfo.Cols; j++)
                    {
                        if (isScaleValues)
                            imLines.Scanlines[i][j] = (ushort)(ushort.MaxValue * (double)image[i, j] / maxVal);
                        else
                            imLines.Scanlines[i][j] = image[i, j];
                    }
                wrt.WriteRowsInt(imLines.Scanlines);
                wrt.End();
            }
            return file;
        }

        private static Color FalseColorSpectrum(ushort val)
        {
            float x = 0.72f*(0.925f - ((float) val/255.0f));

            byte R =
                (byte)
                    System.Math.Max(0,
                        System.Math.Min((255*(System.Math.Cos(2.0*System.Math.PI*(x - 0.0000)) + 0.5)), 255));
            byte G =
                (byte)
                    System.Math.Max(0,
                        System.Math.Min((255*(System.Math.Cos(2.0*System.Math.PI*(x - 0.3333)) + 0.5)), 255));
            byte B =
                (byte)
                    System.Math.Max(0,
                        System.Math.Min((255*(System.Math.Cos(2.0*System.Math.PI*(x - 0.6666)) + 0.5)), 255));

            return Color.FromArgb(255, R, G, B);
        }

        public static WriteableBitmap GetImageFromUShort(ushort[,] img, bool scale, bool falseColor)
        {
            ushort val;
            int imgHeight = img.GetLength(0);
            int imgWidth = img.GetLength(1);

            float maxVal = 1;
            if (scale)
            {
                for (int i = 0; i < imgHeight; i++)
                {
                    for (int j = 0; j < imgWidth; j++)
                    {
                        if (maxVal < img[i, j])
                        {
                            maxVal = img[i, j];
                        }
                    }
                }
            }

            WriteableBitmap bitmap = BitmapFactory.New(imgWidth, imgHeight);


            using (bitmap.GetBitmapContext())
            {
                for (int i = 0; i < imgHeight; i++)
                {
                    for (int j = 0; j < imgWidth; j++)
                    {
                        if (scale)
                        {
                            val = (ushort) (255.0f*((float) img[i, j]/maxVal));
                        }
                        else
                        {
                            val = img[i, j];
                        }

                        if (falseColor)
                        {
                            bitmap.SetPixel(j, i, FalseColorSpectrum(val));
                        }
                        else
                        {
                            if (scale)
                                bitmap.SetPixel(j, i, (byte)val, (byte)val, (byte)val);
                            else
                            {
                                //val >>= 4;
                                var new_val = byte.MaxValue * (float)val / ushort.MaxValue;
                                byte grey = 0;
                                if (new_val > 255)
                                    grey = 255;
                                else
                                    grey = (byte)new_val;
                                bitmap.SetPixel(j, i, grey, grey, grey);
                            }
                                //bakulev Color.FromArgb(val, val, val));
                        }
                    }
                }
            }

            return bitmap;
        }
    }
}
