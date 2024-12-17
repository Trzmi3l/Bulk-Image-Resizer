using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bulk_Image_Resizer
{
    internal class MultiThreading
    {

        static List<List<string>> chunks = new List<List<string>>();
        static int filecount;

        public static int _iters = 0;
        public static async Task PerformTaskEverySecondAsync(int filecount = 0)
        {
            while (true)
            {
                // Wykonaj jakąś operację (np. wyświetlenie komunikatu)
                Console.Write("\r" + _iters + "/" + filecount);

                // Opóźnienie 1 sekundy przed kolejnym wykonaniem
                await Task.Delay(100); // 1000 ms = 1 sekunda
            }
        }

        public static List<List<string>> splitToChunks(List<string> source, int numberOfChunks)
        {
            var result = new List<List<string>>();

            int chunkSize = (int)Math.Ceiling((double)source.Count / numberOfChunks);

            for (int i = 0; i < source.Count; i += chunkSize)
            {
                int currentChunkSize = Math.Min(chunkSize, source.Count - i);
                result.Add(source.GetRange(i, currentChunkSize));
            }
            return result;
        }

        public static void init(List<string> files, int numberOfChunks)
        {
            chunks = splitToChunks(files, numberOfChunks);
            filecount = files.Count;
        }

        public static void resizeImagesMulti(int width, int height, int quality)
        {
            SmoothingMode sq;
            InterpolationMode iq;
            CompositingQuality cq;
            switch (quality)
            {
                case 1:
                    sq = SmoothingMode.None;
                    iq = InterpolationMode.Low;
                    cq = CompositingQuality.HighSpeed;
                    break;
                case 2:
                    sq = SmoothingMode.Default;
                    iq = InterpolationMode.Bicubic;
                    cq = CompositingQuality.AssumeLinear;
                    break;
                case 3:
                    sq = SmoothingMode.HighQuality;
                    iq = InterpolationMode.HighQualityBicubic;
                    cq = CompositingQuality.HighQuality;
                    break;
                default:
                    sq = SmoothingMode.Default;
                    iq = InterpolationMode.Bicubic;
                    cq = CompositingQuality.AssumeLinear;
                    break;
            }
            PerformTaskEverySecondAsync(filecount);
            Parallel.ForEach(chunks, (chunk, state, index) =>
            {
                
                Operations.resizeImages(chunk.ToArray(), width, height, sq, iq, cq);
                
            });
        }
    }
}
