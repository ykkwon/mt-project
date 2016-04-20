using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using AcousticFingerprintingLibrary_0._4._5;


namespace DatabasePopulationApplication_0._4._5
{
    /// <summary>
    ///   Class for creating images from audio file
    /// </summary>
    /// <remarks>
    ///   Allows creation of the signal, spectrogram and wavelets images
    /// </remarks>
    public static class Imaging
    {
        /// <summary>
        ///   Creates an image from top wavelet fingerprint data
        /// </summary>
        /// <param name = "data">The concatenated fingerprint containing top wavelets</param>
        /// <param name = "width">Width of the image</param>
        /// <param name = "height">Height of the image</param>
        public static Image GetFingerprintImage(bool[] data, int width, int height)
        {
            #if SAFE
            // create new image
            if (data == null)
                throw new ArgumentException("Bitmap data was not supplied");
            #endif
            Bitmap image = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            //Scale the data and write to image
            for (int i = 0; i < width /*128*/; i++)
            {
                for (int j = 0; j < height - 1 /*32*/; j++)
                {
                    int color = data[height * i + 2 * j] || data[height * i + 2 * j + 1] ? 255 : 0;
                    image.SetPixel(i, j, Color.FromArgb(color, color, color));
                }
            }
            //return the image
            return image;
        }

        /// <summary>
        ///   Gets the full media representation of the fingerprints
        /// </summary>
        /// <param name = "data">Fingerprints</param>
        /// <param name = "width">Width of the image</param>
        /// <param name = "height">Height of the image</param>
        /// <returns>Bitmap representation of the fingerprints. All fingerprints in one file</returns>
        public static Bitmap GetFingerprintsImage(List<Fingerprint> data, int width, int height)
        //public static Bitmap GetFingerprintsImage(List<bool[]> data, int width, int height)
        {
            const int imagesPerRow = 5; /*5 bitmap images per line*/
            const int spaceBetweenImages = 10; /*10 pixel space between images*/
            int fingersCount = data.Count;
            int rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);

            int imageWidth = imagesPerRow * (width + spaceBetweenImages) + spaceBetweenImages;
            int imageHeight = rowCount * (height + spaceBetweenImages) + spaceBetweenImages;

            Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);

            /*Change the background of the bitmap*/
            for (int i = 0; i < imageWidth; i++)
                for (int j = 0; j < imageHeight; j++)
                    image.SetPixel(i, j, Color.White);

            int verticalOffset = spaceBetweenImages;
            int horizontalOffset = spaceBetweenImages;
            int count = 0;
            for (int z = 0; z < data.Count; z++)
            {
                bool[] finger = data[z].Signature;
                for (int i = 0; i < width /*128*/; i++)
                {
                    for (int j = 0; j < height /*32*/; j++)
                    {
                        int color = finger[2 * height * i + 2 * j] || finger[2 * height * i + 2 * j + 1] ? 255 : 0;
                        image.SetPixel(i + horizontalOffset, j + verticalOffset, Color.FromArgb(color, color, color));
                    }
                }
                count++;
                if (count % imagesPerRow == 0)
                {
                    verticalOffset += height + spaceBetweenImages;
                    horizontalOffset = spaceBetweenImages;
                }
                else
                    horizontalOffset += width + spaceBetweenImages;
            }
            return image;
        }

        /// <summary>
        ///   Get a spectrogram of the signal specified at the input
        /// </summary>
        /// <param name = "data">Signal</param>
        /// <param name = "width">Width of the image</param>
        /// <param name = "height">Height of the image</param>
        /// <returns>Spectral image of the signal</returns>
        /// <remarks>
        ///   X axis - time
        ///   Y axis - frequency
        ///   Color - magnitude level of corresponding band value of the signal
        /// </remarks>
        public static Bitmap GetSpectrogramImage(float[][] spectrum, int width, int height)
        {
            #if SAFE
            if (width < 0)
                throw new ArgumentException("width should be bigger than 0");
            if (height < 0)
                throw new ArgumentException("height should be bigger than 0");
            #endif
            Bitmap image = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image);
            /*Fill Back color*/
            using (Brush brush = new SolidBrush(Color.Black))
            {
                graphics.FillRectangle(brush, new Rectangle(0, 0, width, height));
            }
            int bands = spectrum[0].Length;
            double max = spectrum.Max((b) => b.Max((v) => Math.Abs(v)));
            double deltaX = (double)(width - 1) / spectrum.Length; /*By how much the image will move to the left*/
            double deltaY = (double)(height - 1) / (bands + 1); /*By how much the image will move upward*/
            int prevX = 0;
            for (int i = 0, n = spectrum.Length; i < n; i++)
            {
                double x = i * deltaX;
                if ((int)x == prevX) continue;
                for (int j = 0, m = spectrum[0].Length; j < m; j++)
                {
                    Color color = ValueToBlackWhiteColor(spectrum[i][j], max / 10);
                    image.SetPixel((int)x, height - (int)(deltaY * j) - 1, color);
                }
                prevX = (int)x;
            }

            return image;
        }

        /// <summary>
        ///   Get corresponding grey pallet color of the spectrogram
        /// </summary>
        /// <param name = "value">Value</param>
        /// <param name = "maxValue">Max range of the values</param>
        /// <returns>Grey color corresponding to the value</returns>
        public static Color ValueToBlackWhiteColor(double value, double maxValue)
        {
            int color = (int)(Math.Abs(value) * 255 / Math.Abs(maxValue));
            if (color > 255)
                color = 255;
            return Color.FromArgb(color, color, color);
        }

        /// <summary>
        ///   Gets the spectrum of the wavelet decomposition before extracting top wavelets and binary transformation
        /// </summary>
        /// <param name = "pathToFile">Path to file to be drawn</param>
        /// <param name = "proxy">Proxy manager</param>
        /// <param name = "manager">Fingerprint manager</param>
        /// <returns>Image to be saved</returns>
        public static Image GetWaveletSpectralImage(string pathToFile, BassProxy proxy, FingerprintManager manager)
        {
            List<float[][]> wavelets = new List<float[][]>();
            int sampleRate = manager.SampleRate;
            float[] samples = BassProxy.GetSamplesMono(pathToFile, sampleRate);
            float[][] spectrum = manager.CreateLogSpectrogram(samples);
            int specLen = spectrum.GetLength(0);
            int start = 0 / manager.Overlap;
            int logbins = manager.LogBins;
            int fingerprintLength = manager.FingerprintWidth;
            int overlap = manager.Overlap;
            while (start + fingerprintLength < specLen)
            {
                float[][] frames = new float[fingerprintLength][];
                for (int i = 0; i < fingerprintLength; i++)
                {
                    frames[i] = new float[logbins];
                    Array.Copy(spectrum[start + i], frames[i], logbins);
                }
                start += fingerprintLength + manager.Stride / overlap;
                wavelets.Add(frames);
            }

            const int imagesPerRow = 5; /*5 bitmap images per line*/
            const int spaceBetweenImages = 10; /*10 pixel space between images*/
            int width = wavelets[0].GetLength(0);
            int height = wavelets[0][0].Length;
            int fingersCount = wavelets.Count;
            int rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);

            int imageWidth = imagesPerRow * (width + spaceBetweenImages) + spaceBetweenImages;
            int imageHeight = rowCount * (height + spaceBetweenImages) + spaceBetweenImages;

            Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);
            /*Change the background of the bitmap*/
            for (int i = 0; i < imageWidth; i++)
                for (int j = 0; j < imageHeight; j++)
                    image.SetPixel(i, j, Color.White);

            double maxValue = wavelets.Max((wavelet) => (wavelet.Max((column) => column.Max())));
            int verticalOffset = spaceBetweenImages;
            int horizontalOffset = spaceBetweenImages;
            int count = 0;
            double max = wavelets.Max(wav => wav.Max(w => w.Max(v => Math.Abs(v))));
            foreach (float[][] wavelet in wavelets)
            {
                for (int i = 0; i < width /*128*/; i++)
                {
                    for (int j = 0; j < height /*32*/; j++)
                    {
                        Color color = ValueToBlackWhiteColor(wavelet[i][j], max / 4);
                        image.SetPixel(i + horizontalOffset, j + verticalOffset, color);
                    }
                }
                count++;
                if (count % imagesPerRow == 0)
                {
                    verticalOffset += height + spaceBetweenImages;
                    horizontalOffset = spaceBetweenImages;
                }
                else
                    horizontalOffset += width + spaceBetweenImages;
            }
            return image;
        }
    }
}