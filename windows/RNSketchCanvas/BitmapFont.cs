using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace RNSketchCanvas
{
    public static class BitmapFont
    {

        private static Dictionary<string, List<FontInfo>> fonts = new Dictionary<string, List<FontInfo>>();

        private class FontInfo
        {
            public FontInfo(WriteableBitmap image, Dictionary<char, Rect> metrics, int size)
            {
                this.Image = image;
                this.Metrics = metrics;
                this.Size = size;
            }
            public WriteableBitmap Image { get; private set; }
            public Dictionary<char, Rect> Metrics { get; private set; }
            public int Size { get; private set; }
        }

        public async static void RegisterFont(string name, params int[] sizes)
        {
            foreach (var size in sizes)
            {
                string fontFile = name + "_" + size + ".png";
                string fontMetricsFile = name + "_" + size + ".xml";

                var installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var storageFontFile = await installedLocation.GetFileAsync("RNSketchCanvas\\Assets\\" + fontFile);
                var storageMetricsFile = await installedLocation.GetFileAsync("RNSketchCanvas\\Assets\\" + fontMetricsFile);

                using (IRandomAccessStream fileStream = await storageFontFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage image = new BitmapImage();
                    await image.SetSourceAsync(fileStream);

                    var wbImage = new WriteableBitmap(image.PixelWidth, image.PixelHeight);
                    fileStream.Seek(0);
                    await wbImage.SetSourceAsync(fileStream);


                    var metrics = XDocument.Load("RNSketchCanvas\\Assets\\" + fontMetricsFile);
                    var dict = (from c in metrics.Root.Elements()
                                let key = (char)((int)c.Attribute("key"))
                                let rect = new Rect((int)c.Element("x"), (int)c.Element("y"), (int)c.Element("width"), (int)c.Element("height"))
                                select new { Char = key, Metrics = rect }).ToDictionary(x => x.Char, x => x.Metrics);

                    var fontInfo = new FontInfo(wbImage, dict, size);

                    if (fonts.ContainsKey(name))
                    {
                        fonts[name].Add(fontInfo);
                    }
                    else
                    {
                        fonts.Add(name, new List<FontInfo> { fontInfo });
                    }


                }


  
                   
            }
        }

        private static FontInfo GetNearestFont(string fontName, int size)
        {
            if (fonts.ContainsKey(fontName))
            {
                return fonts[fontName].OrderBy(x => Math.Abs(x.Size - size)).First();
            }

            return null;
            
        }

        public static Size MeasureString(string text, string fontName, int size)
        {
            var font = GetNearestFont(fontName, size);

            double scale = (double)size / font.Size;

            var letters = text.Select(x => font.Metrics[x]).ToArray();

            return new Size(letters.Sum(x => x.Width * scale), letters.Max(x => x.Height * scale));
        }


        public static Color ToColor(this uint value)
        {
            var colorByte = BitConverter.GetBytes(value);

            if (colorByte.Length == 4)
            {
                return Color.FromArgb(colorByte[3], colorByte[2], colorByte[1], colorByte[0]);
            }

            return Color.FromArgb(0,0,0,0);
        }
        public static void DrawString(this WriteableBitmap bmp, string text, int x, int y, string fontName, int size, uint color)
        {
            var font = GetNearestFont(fontName, size);

            if (font == null)
                return;


            var textHeight = 0.0;
            var lines = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);


            for (int i = 0; i < lines.Length; i++)
            {
                var letters = lines[i].Select(f => font.Metrics[f]).ToArray();
                if (letters.Length == 0)
                    continue;
                textHeight = (letters.FirstOrDefault().Height + 2) * i;

                double scale = (double)size / font.Size;

                double destX = x;
                var textColor = color.ToColor();
                foreach (var letter in letters)
                {
                    var destRect = new Rect(destX, y, letter.Width * scale, letter.Height * scale);
                   
                    //bmp.Blit(destRect, font.Image, letter, color, WriteableBitmapExtensions.BlendMode.Alpha);
                    //bmp.Blit(destRect, font.Image, letter, WriteableBitmapExtensions.BlendMode.Alpha);

                    bmp.Blit(new Point(destRect.X, destRect.Y + textHeight), font.Image, letter, textColor, WriteableBitmapExtensions.BlendMode.Alpha);
                    destX += destRect.Width;
                }
            }

        }
    }
}
