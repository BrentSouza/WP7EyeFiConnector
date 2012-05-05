using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Globalization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows;

namespace EyeFiConnector
{
    public static class Utilities
    {
        public static AutoResetEvent WaitForLog = new AutoResetEvent(true);
        public static byte[] HexToBytes(string hex)
        {
            List<byte> data = new List<byte>();
            string byteSet = string.Empty;
            int stringLen = hex.Length;
            int length = 0;
            for (int i = 0; i < stringLen; i += 2)
            {
                length = (stringLen - i) > 1 ? 2 : 1;
                byteSet = hex.Substring(i, length);
                data.Add(Convert.ToByte(byteSet, 16));
            }

            return data.ToArray();
        }

        public static string HexToByteString(string hex)
        {
            byte[] byteArray = HexToBytes(hex);
            return UTF8Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
        }

        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }

        public static string EncodeXml(XDocument doc, Encoding encoding)
        {
            string s;

            using (MemoryStream sw = new MemoryStream())
            {
                using (StreamWriter strw = new StreamWriter(sw, encoding))
                {
                    doc.Save(strw);
                    s = encoding.GetString(sw.ToArray(), 0, (int)sw.Length);
                }
            }

            return s;
        }

        /// <summary>
        /// The interpolation method.
        /// </summary>
        public enum Interpolation
        {
            /// <summary>
            /// The nearest neighbor algorithm simply selects the color of the nearest pixel.
            /// </summary>
            NearestNeighbor = 0,

            /// <summary>
            /// Linear interpolation in 2D using the average of 3 neighboring pixels.
            /// </summary>
            Bilinear,
        }

        #region Resize

        /// <summary>
        /// Creates a new resized WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="width">The new desired width.</param>
        /// <param name="height">The new desired height.</param>
        /// <param name="interpolation">The interpolation method that should be used.</param>
        /// <returns>A new WriteableBitmap that is a resized version of the input.</returns>
        public static WriteableBitmap Resize(this WriteableBitmap bmp, int width, int height, Interpolation interpolation)
        {
            var pd = Resize(bmp.Pixels, bmp.PixelWidth, bmp.PixelHeight, width, height, interpolation);

            var result = new WriteableBitmap(width, height);
            Buffer.BlockCopy(pd, 0, result.Pixels, 0, 4 * pd.Length);
            return result;
        }

        /// <summary>
        /// Creates a new resized bitmap.
        /// </summary>
        /// <param name="pixels">The source pixels.</param>
        /// <param name="widthSource">The width of the source pixels.</param>
        /// <param name="heightSource">The height of the source pixels.</param>
        /// <param name="width">The new desired width.</param>
        /// <param name="height">The new desired height.</param>
        /// <param name="interpolation">The interpolation method that should be used.</param>
        /// <returns>A new bitmap that is a resized version of the input.</returns>
        public static int[] Resize(int[] pixels, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
        {
            var pd = new int[width * height];
            var xs = (float)widthSource / width;
            var ys = (float)heightSource / height;

            float fracx, fracy, ifracx, ifracy, sx, sy, l0, l1, rf, gf, bf;
            int c, x0, x1, y0, y1;
            byte c1a, c1r, c1g, c1b, c2a, c2r, c2g, c2b, c3a, c3r, c3g, c3b, c4a, c4r, c4g, c4b;
            byte a, r, g, b;

            // Nearest Neighbor
            if (interpolation == Interpolation.NearestNeighbor)
            {
                var srcIdx = 0;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        sx = x * xs;
                        sy = y * ys;
                        x0 = (int)sx;
                        y0 = (int)sy;

                        pd[srcIdx++] = pixels[y0 * widthSource + x0];
                    }
                }
            }

               // Bilinear
            else if (interpolation == Interpolation.Bilinear)
            {
                var srcIdx = 0;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        sx = x * xs;
                        sy = y * ys;
                        x0 = (int)sx;
                        y0 = (int)sy;

                        // Calculate coordinates of the 4 interpolation points
                        fracx = sx - x0;
                        fracy = sy - y0;
                        ifracx = 1f - fracx;
                        ifracy = 1f - fracy;
                        x1 = x0 + 1;
                        if (x1 >= widthSource)
                        {
                            x1 = x0;
                        }
                        y1 = y0 + 1;
                        if (y1 >= heightSource)
                        {
                            y1 = y0;
                        }


                        // Read source color
                        c = pixels[y0 * widthSource + x0];
                        c1a = (byte)(c >> 24);
                        c1r = (byte)(c >> 16);
                        c1g = (byte)(c >> 8);
                        c1b = (byte)(c);

                        c = pixels[y0 * widthSource + x1];
                        c2a = (byte)(c >> 24);
                        c2r = (byte)(c >> 16);
                        c2g = (byte)(c >> 8);
                        c2b = (byte)(c);

                        c = pixels[y1 * widthSource + x0];
                        c3a = (byte)(c >> 24);
                        c3r = (byte)(c >> 16);
                        c3g = (byte)(c >> 8);
                        c3b = (byte)(c);

                        c = pixels[y1 * widthSource + x1];
                        c4a = (byte)(c >> 24);
                        c4r = (byte)(c >> 16);
                        c4g = (byte)(c >> 8);
                        c4b = (byte)(c);


                        // Calculate colors
                        // Alpha
                        l0 = ifracx * c1a + fracx * c2a;
                        l1 = ifracx * c3a + fracx * c4a;
                        a = (byte)(ifracy * l0 + fracy * l1);

                        // Red
                        l0 = ifracx * c1r * c1a + fracx * c2r * c2a;
                        l1 = ifracx * c3r * c3a + fracx * c4r * c4a;
                        rf = ifracy * l0 + fracy * l1;

                        // Green
                        l0 = ifracx * c1g * c1a + fracx * c2g * c2a;
                        l1 = ifracx * c3g * c3a + fracx * c4g * c4a;
                        gf = ifracy * l0 + fracy * l1;

                        // Blue
                        l0 = ifracx * c1b * c1a + fracx * c2b * c2a;
                        l1 = ifracx * c3b * c3a + fracx * c4b * c4a;
                        bf = ifracy * l0 + fracy * l1;

                        // Divide by alpha
                        if (a > 0)
                        {
                            rf = rf / a;
                            gf = gf / a;
                            bf = bf / a;
                        }

                        // Cast to byte
                        r = (byte)rf;
                        g = (byte)gf;
                        b = (byte)bf;

                        // Write destination
                        pd[srcIdx++] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
            return pd;
        }

        #endregion


        public static void Log(string entry,Boolean isOverwrite)
        {
            WaitForLog.WaitOne();

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                FileMode mode = isOverwrite ? FileMode.Create : FileMode.OpenOrCreate;

                using (IsolatedStorageFileStream fstream = new IsolatedStorageFileStream("log.txt", mode, store))
                {
                    fstream.Seek(0, SeekOrigin.End);
                    using (var sw = new StreamWriter(fstream))
                    {
                        sw.WriteLine(entry);
                    }
                }   
            }

            WaitForLog.Set();
        }

    }
}
