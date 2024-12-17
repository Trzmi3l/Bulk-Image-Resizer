using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace Bulk_Image_Resizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string folderPath = "";



            Console.WriteLine("Type folder path: ");
            folderPath = Console.ReadLine();

            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                                      .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".bmp"))
                                      .ToArray();


            Console.WriteLine("Type 1 for renaming, type 2 for resizing images: ... ");
            string input = Console.ReadLine();

            if (input.Contains("1"))
            {
                // Renaming operation
                Operations.renameToNumbers(files, 0);
            }
            else if (input.Contains("2"))
            {
                Console.WriteLine("Type 1 for multithreaded or 2 for single-threaded");

                string resizingChoice = Console.ReadLine();

                if (resizingChoice.Contains("1"))
                {
                    // Multithreaded resizing
                    HandleMultithreadedResizing(files);
                }
                else if (resizingChoice.Contains("2"))
                {
                    // Single-threaded resizing
                    HandleSingleThreadedResizing(files);
                }
                else
                {
                    Console.WriteLine("Invalid input for resizing type.");
                }
            }
            else
            {
                Console.WriteLine("Invalid choice for renaming or resizing.");
            }





        }
        private static void HandleMultithreadedResizing(string[] files)
        {
            Console.WriteLine("Type height, width and quality (1, 2 or 3) separated by comma, e.g., '1000,2000,2': ... ");
            string raw = Console.ReadLine();

            if (TryParseDimensionsAndQuality(raw, out int width, out int height, out int quality))
            {
                MultiThreading.init(files.ToList(), Environment.ProcessorCount);
                MultiThreading.resizeImagesMulti(width, height, quality);
            }
            else
            {
                Console.WriteLine("Invalid input format.");
            }
        }

        private static void HandleSingleThreadedResizing(string[] files)
        {
            Console.WriteLine("Type height, width and quality (1, 2 or 3) separated by comma, e.g., '1000,2000,2': ... ");
            string raw = Console.ReadLine();

            if (TryParseDimensionsAndQuality(raw, out int width, out int height, out int quality))
            {
                SmoothingMode smoothingMode;
                InterpolationMode interpolationMode;
                CompositingQuality compositingQuality;

                switch (quality)
                {
                    case 1:
                        smoothingMode = SmoothingMode.None;
                        interpolationMode = InterpolationMode.Low;
                        compositingQuality = CompositingQuality.HighSpeed;
                        break;
                    case 2:
                        smoothingMode = SmoothingMode.Default;
                        interpolationMode = InterpolationMode.Bicubic;
                        compositingQuality = CompositingQuality.AssumeLinear;
                        break;
                    case 3:
                        smoothingMode = SmoothingMode.HighQuality;
                        interpolationMode = InterpolationMode.HighQualityBicubic;
                        compositingQuality = CompositingQuality.HighQuality;
                        break;
                    default:
                        Console.WriteLine("Invalid quality level.");
                        return;
                }
                MultiThreading.PerformTaskEverySecondAsync(files.Count());
                Operations.resizeImages(files, width, height, smoothingMode, interpolationMode, compositingQuality);
            }
            else
            {
                Console.WriteLine("Invalid input format.");
            }
        }

        private static bool TryParseDimensionsAndQuality(string input, out int width, out int height, out int quality)
        {
            width = 0;
            height = 0;
            quality = 0;

            string[] parts = input.Split(',');

            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out width) || !int.TryParse(parts[1], out height) || !int.TryParse(parts[2], out quality))
                return false;

            return true;
        }


    }





}
