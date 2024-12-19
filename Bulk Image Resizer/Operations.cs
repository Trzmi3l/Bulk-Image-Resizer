using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulk_Image_Resizer
{
    internal class Operations
    {
        public static void renameToNumbers(string[] files, int min)
        {
            try
            {
                int length = files.Length;
                string[] newPaths = new string[length];

                for (int i = 0; i < length; i++)
                {
                    string directory = Path.GetDirectoryName(files[i]);
                    string extension = Path.GetExtension(files[i]);
                    newPaths[i] = Path.Combine(directory, $"{i+min}{extension}");
                }


                for (int i = length - 1; i >= 0; i--)
                {
                    if (!File.Exists(files[i])) continue; 
                    File.Move(files[i], newPaths[i]);
                }

                Console.WriteLine("Files renamed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"${ex.Message}");
            }
        }
        public static void resizeImages(string[] files, int width, int height, SmoothingMode smoothingQuality, InterpolationMode interpolationQuality, CompositingQuality compositingQuality)
        {
            foreach (string inputPath in files)
            {
                try
                {

                    string directory = Path.GetDirectoryName(inputPath);
                    string filenameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
                    string extension = Path.GetExtension(inputPath);
                    string outputPath = Path.Combine(directory, $"{filenameWithoutExt}_resized{extension}");
                    using (Image image = Image.FromFile(inputPath))
                    using (Bitmap resizedImage = new Bitmap(width, height))
                    using (Graphics graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.CompositingQuality = compositingQuality;
                        graphics.SmoothingMode = smoothingQuality;
                        graphics.InterpolationMode = interpolationQuality;
                        graphics.DrawImage(image, 0, 0, width, height);
                        resizedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    Interlocked.Increment(ref MultiThreading._iters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"${ex.Message}");
                }
            }
        }

        static void SplitImageIntoTiles(string inputPath, string outputDirectory, int tileWidth, int tileHeight)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(inputPath))
                {
                    int numTilesX = bitmap.Width / tileWidth;
                    int numTilesY = bitmap.Height / tileHeight;

                    for (int y = 0; y < numTilesY; y++)
                    {
                        for (int x = 0; x < numTilesX; x++)
                        {
                            Rectangle srcRect = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                            using (Bitmap tileBitmap = bitmap.Clone(srcRect, bitmap.PixelFormat))
                            {
                                string outputFilePath = Path.Combine(outputDirectory, $"tile_{Path.GetFileNameWithoutExtension(inputPath)}_{y}_{x}.png");
                                tileBitmap.Save(outputFilePath, System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error splitting image {inputPath}: {ex.Message}");
            }
        }

        static void SplitImageWithOffsets(string inputPath, string outputDirectory, int imageWidth, int imageHeight, int tileWidth, int tileHeight)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(inputPath))
                {
                    int offsetX = tileWidth / 3;
                    int offsetY = tileHeight / 3;

                    for (int y = 0; y < imageHeight; y += tileHeight - offsetY)
                    {
                        for (int x = 0; x < imageWidth; x += tileWidth - offsetX)
                        {
                            Rectangle srcRect = new Rectangle(x, y, tileWidth, tileHeight);
                            if (srcRect.Right > bitmap.Width) srcRect.Width = bitmap.Width - srcRect.X;
                            if (srcRect.Bottom > bitmap.Height) srcRect.Height = bitmap.Height - srcRect.Y;

                            using (Bitmap tileBitmap = bitmap.Clone(srcRect, bitmap.PixelFormat))
                            {
                                string outputFilePath = Path.Combine(outputDirectory, $"offset_tile_{Path.GetFileNameWithoutExtension(inputPath)}_{y}_{x}.png");
                                tileBitmap.Save(outputFilePath, System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating offset tiles for image {inputPath}: {ex.Message}");
            }
        }

        static void GenerateRotatedImages(string inputPath, string outputDirectory)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(inputPath))
                {
                    string baseFileName = Path.GetFileNameWithoutExtension(inputPath);

                    for (int angle = 0; angle < 360; angle += 90)
                    {
                        using (Bitmap rotatedBitmap = RotateImage(bitmap, angle))
                        {
                            string outputFilePath = Path.Combine(outputDirectory, $"{baseFileName}_rotated_{angle}.png");
                            rotatedBitmap.Save(outputFilePath, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating rotated images for {inputPath}: {ex.Message}");
            }
        }

        static Bitmap RotateImage(Bitmap bitmap, float angle)
        {
            Bitmap rotatedBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            using (Graphics graphics = Graphics.FromImage(rotatedBitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.TranslateTransform((float)bitmap.Width / 2, (float)bitmap.Height / 2);
                graphics.RotateTransform(angle);
                graphics.TranslateTransform(-(float)bitmap.Width / 2, -(float)bitmap.Height / 2);
                graphics.DrawImage(bitmap, new Point(0, 0));
            }
            return rotatedBitmap;
        }
    }
}
