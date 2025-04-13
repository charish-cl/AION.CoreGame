using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameDevKitEditor
{
    public class ImageComparer
    {
        public static Dictionary<string, List<Texture2D>> imageCache = new Dictionary<string,  List<Texture2D>>();

        public static void AddImage(Texture2D image)
        {
            string key1 = GetImageKey(image);
            
            if (string.IsNullOrEmpty(key1)) return;
            if (!imageCache.ContainsKey(key1))
            {
                imageCache[key1] = new List<Texture2D>();
                imageCache[key1].Add(image);
            }
            else
            {
                imageCache[key1].Add(image);
            }

        }
        public static bool CompareImagesWithCache(Texture2D image1, Texture2D image2)
        {
            string key1 = GetImageKey(image1);
            string key2 = GetImageKey(image2);
            
            return key1 == key2;
        }

        private static string GetImageKey(Texture2D image)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                Color32[] pixels = image.GetPixels32();
                // StringBuilder sb = new StringBuilder();
                // foreach (Color32 pixel in pixels)
                // {
                //     sb.Append(pixel.r);
                //     sb.Append(pixel.g);
                //     sb.Append(pixel.b);
                //     sb.Append(pixel.a);
                // }
                //几百毫秒
                //
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < pixels.Length; i += 100) // Save every 100th pixel 平均10ms
                {
                    Color32 pixel = pixels[i];
                    sb.Append(pixel.r);
                    sb.Append(pixel.g);
                    sb.Append(pixel.b);
                    sb.Append(pixel.a);
                }
                stopwatch.Stop();
                UnityEngine.Debug.Log("Execution time: " + stopwatch.ElapsedMilliseconds + "ms");
                return sb.ToString();
            }
            catch (Exception e)
            {
                Debug.Log($"image {image.name}");
                Console.WriteLine(e);
            }

            return default;
        }

        public static bool CompareImages(Texture2D image1, Texture2D image2)
        {
            if (image1.width != image2.width || image1.height != image2.height)
            {
                return false;
            }
        
            // for (int x = 0; x < image1.width; x++)
            // {
            //     for (int y = 0; y < image1.height; y++)
            //     {
            //         if (image1.GetPixel(x, y) != image2.GetPixel(x, y))
            //         {
            //             return false;
            //         }
            //     }
            // }
            //Execution time: 7411ms
            Color32[] pixels1 = image1.GetPixels32();
            Color32[] pixels2 = image2.GetPixels32();

            for (int i = 0; i < pixels1.Length; i++)
            {
                if (pixels1[i].r != pixels2[i].r || pixels1[i].g != pixels2[i].g || pixels1[i].b != pixels2[i].b || pixels1[i].a != pixels2[i].a)
                {
                    return false;
                }
            }
            //Execution time: 3ms
          
            return true;
        }
    }
}