using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using Windows.UI.Core;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Graphics.Display;
using Windows.Foundation;
using ReactNative.Bridge;
using ReactNative.UIManager;
using ReactNative.Views.Image;
using ReactNative.UIManager.Events;
using Newtonsoft.Json.Linq;

namespace RNSketchCanvas
{
    public class SketchCanvas : Panel
    {
        public WriteableBitmap bitmap { get; set; }
        private WriteableBitmap mBackgroundImage { get; set; }

        private List<SketchData> mPaths = new List<SketchData>();
        private SketchData mCurrentPath = null;
        private Image image = new Image();
        public ThemedReactContext Context { get; set; }

        public SketchCanvas(ThemedReactContext Context)
        {
            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            this.Context = Context;
            this.Children.Add(image);
        }
        public async Task<bool> openImageFile(string filename, string directory, string mode)
        {
            var restult = false;
            try
            {
                //var installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;

                //FIXME - only open files from TemporaryFolder is for now supported
                var tempState = Windows.Storage.ApplicationData.Current.TemporaryFolder;
                var filenameOnly = System.IO.Path.GetFileName(filename);

                Windows.Storage.StorageFile imageFile = await tempState.GetFileAsync(filenameOnly);

                using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);

                    mBackgroundImage =
                        new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                    fileStream.Seek(0);
                    await mBackgroundImage.SetSourceAsync(fileStream);

                    mBackgroundImage = this.ResizedImage(mBackgroundImage, (int)this.Width, (int)this.Height, mode, WriteableBitmapExtensions.Interpolation.Bilinear);
                    restult = true;
                }

            }
            catch (Exception)
            {

            }

            reDraw();

            return restult;
        }



        public WriteableBitmap ResizedImage(WriteableBitmap sourceImage, int maxWidth, int maxHeight, string mode, WriteableBitmapExtensions.Interpolation interpolation)
        {

            //FIXME - Add support for string mode: 'AspectFill' | 'AspectFit' | 'ScaleToFill'


            var origHeight = sourceImage.PixelHeight;
            var origWidth = sourceImage.PixelWidth;



            var ratioX = maxWidth / (float)origWidth;
            var ratioY = maxHeight / (float)origHeight;
            var ratio = Math.Min(ratioX, ratioY);

            var newHeight = (int)(origHeight * ratio);
            var newWidth = (int)(origWidth * ratio);



            return sourceImage.Resize(newWidth, newHeight, interpolation);

        }


        public async void setCanvasText(string aText)
        {




            //CanvasDevice device = CanvasDevice.GetSharedDevice();
            //CanvasBitmap bitmap = CanvasBitmap.CreateFromBytes(device, new byte[0], 100, 100, Windows.Graphics.DirectX.DirectXPixelFormat.Unknown);

            //CanvasRenderTarget offScreen = null;

            //if (bitmap != null)
            //{
            //    offScreen = new CanvasRenderTarget(device,
            //        bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height, 96);
            //    using (var ds = offScreen.CreateDrawingSession())
            //    {
            //        // do not forget clear buffer
            //        ds.Clear(Colors.Transparent);

            //        // draw original bitmap
            //        ds.DrawImage(bitmap);

            //        // draw something on it
            //        ds.FillCircle(bitmap.SizeInPixels.Width / 2,
            //            bitmap.SizeInPixels.Height / 2, 50, Colors.GreenYellow);

            //    }

            //    IRandomAccessStream s = null;


            //    await offScreen.SaveAsync(s, CanvasBitmapFileFormat.Auto);

            //    this.bitmap = await BitmapFactory.FromStream(s);



            //}

            //TextBlock tb = new TextBlock();
            //tb.Foreground = new SolidColorBrush(Colors.White);
            //tb.Text = "some text";
            //tb.FontSize = 20;
            //TranslateTransform tf = new TranslateTransform() { X = 100, Y = 100 };





            //var clr = new Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl();

            //this.Children.Add(clr);
            //var cl = new CanvasCommandList(clr);


            //using (var clds = cl.CreateDrawingSession())
            //{
            //    clds.DrawText(aText, new System.Numerics.Vector2(10f), Color.FromArgb(255, 100, 100, 100));
            //}



            //var displayInformation = DisplayInformation.GetForCurrentView();


            //clr.Arrange(new Rect(0, 0, 100, 100));

            //var renderTargetBitmap = new RenderTargetBitmap();
            //await renderTargetBitmap.RenderAsync(clr, 100, 100);

            //var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();


            //var bb = await BitmapFactory.FromPixelBuffer(pixelBuffer, 100, 100);

            //bitmap = bb;

            //this.image.Source = bitmap;

            //var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("Screen.jpg", CreationCollisionOption.ReplaceExisting);
            //using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);

            //    encoder.SetPixelData(
            //            BitmapPixelFormat.Bgra8,
            //            BitmapAlphaMode.Ignore,
            //            (uint)renderTargetBitmap.PixelWidth,
            //            (uint)renderTargetBitmap.PixelHeight,
            //            displayInformation.LogicalDpi,
            //            displayInformation.LogicalDpi,
            //            pixelBuffer.ToArray());

            //    await encoder.FlushAsync();
            //}


            //mArrCanvasText.clear();
            //mArrSketchOnText.clear();
            //mArrTextOnSketch.clear();

            //if (aText != null)
            //{
            //    for (int i = 0; i < aText.size(); i++)
            //    {
            //        ReadableMap property = aText.getMap(i);
            //        if (property.hasKey("text"))
            //        {
            //            String alignment = property.hasKey("alignment") ? property.getString("alignment") : "Left";
            //            int lineOffset = 0, maxTextWidth = 0;
            //            String[] lines = property.getString("text").split("\n");
            //            ArrayList<CanvasText> textSet = new ArrayList<CanvasText>(lines.length);
            //            for (String line: lines)
            //            {
            //                ArrayList<CanvasText> arr = property.hasKey("overlay") && "TextOnSketch".equals(property.getString("overlay")) ? mArrTextOnSketch : mArrSketchOnText;
            //                CanvasText text = new CanvasText();
            //                Paint p = new Paint(Paint.ANTI_ALIAS_FLAG);
            //                p.setTextAlign(Paint.Align.LEFT);
            //                text.text = line;
            //                if (property.hasKey("font"))
            //                {
            //                    Typeface font;
            //                    try
            //                    {
            //                        font = Typeface.createFromAsset(mContext.getAssets(), property.getString("font"));
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        font = Typeface.create(property.getString("font"), Typeface.NORMAL);
            //                    }
            //                    p.setTypeface(font);
            //                }
            //                p.setTextSize(property.hasKey("fontSize") ? (float)property.getDouble("fontSize") : 12);
            //                p.setColor(property.hasKey("fontColor") ? property.getInt("fontColor") : 0xFF000000);
            //                text.anchor = property.hasKey("anchor") ? new PointF((float)property.getMap("anchor").getDouble("x"), (float)property.getMap("anchor").getDouble("y")) : new PointF(0, 0);
            //                text.position = property.hasKey("position") ? new PointF((float)property.getMap("position").getDouble("x"), (float)property.getMap("position").getDouble("y")) : new PointF(0, 0);
            //                text.paint = p;
            //                text.isAbsoluteCoordinate = !(property.hasKey("coordinate") && "Ratio".equals(property.getString("coordinate")));
            //                text.textBounds = new Rect();
            //                p.getTextBounds(text.text, 0, text.text.length(), text.textBounds);

            //                text.lineOffset = new PointF(0, lineOffset);
            //                lineOffset += text.textBounds.height() * 1.5 * (property.hasKey("lineHeightMultiple") ? property.getDouble("lineHeightMultiple") : 1);
            //                maxTextWidth = Math.max(maxTextWidth, text.textBounds.width());

            //                arr.add(text);
            //                mArrCanvasText.add(text);
            //                textSet.add(text);
            //            }
            //            for (CanvasText text: textSet)
            //            {
            //                text.height = lineOffset;
            //                if (text.textBounds.width() < maxTextWidth)
            //                {
            //                    float diff = maxTextWidth - text.textBounds.width();
            //                    text.textBounds.left += diff * text.anchor.x;
            //                    text.textBounds.right += diff * text.anchor.x;
            //                }
            //            }
            //            if (getWidth() > 0 && getHeight() > 0)
            //            {
            //                for (CanvasText text: textSet)
            //                {
            //                    text.height = lineOffset;
            //                    PointF position = new PointF(text.position.x, text.position.y);
            //                    if (!text.isAbsoluteCoordinate)
            //                    {
            //                        position.x *= getWidth();
            //                        position.y *= getHeight();
            //                    }
            //                    position.x -= text.textBounds.left;
            //                    position.y -= text.textBounds.top;
            //                    position.x -= (text.textBounds.width() * text.anchor.x);
            //                    position.y -= (text.height * text.anchor.y);
            //                    text.drawPosition = position;
            //                }
            //            }
            //            if (lines.length > 1)
            //            {
            //                for (CanvasText text: textSet)
            //                {
            //                    switch (alignment)
            //                    {
            //                        case "Left":
            //                        default:
            //                            break;
            //                        case "Right":
            //                            text.lineOffset.x = (maxTextWidth - text.textBounds.width());
            //                            break;
            //                        case "Center":
            //                            text.lineOffset.x = (maxTextWidth - text.textBounds.width()) / 2;
            //                            break;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //invalidateCanvas(false);
        }

        public void clear()
        {
            mPaths = new List<SketchData>();
            reDraw();
        }

        public void newPath(int id, UInt32 strokeColor, int strokeWidth)
        {
            mCurrentPath = new SketchData(id, strokeColor, strokeWidth);
            mPaths.Add(mCurrentPath);
        }

        public void addPoint(Point point)
        {

            //point.ScaleImage(this.Width, this.Height, mBackgroundImage.PixelHeight, mBackgroundImage.PixelWidth);
            mCurrentPath.addPoint(point);

            if (mCurrentPath.isTranslucent)
            {
                //mTranslucentDrawingCanvas.drawColor(Color.TRANSPARENT, PorterDuff.Mode.MULTIPLY);
                //mCurrentPath.draw(mTranslucentDrawingCanvas);
            }
            else
            {
                mCurrentPath.drawLastPoint(bitmap);
            }
        }

        public void addPath(int id, int strokeColor, float strokeWidth, List<Point> points)
        {
           //FIXME - for now not supported
        }

        public void deletePath(int id)
        {
            var item = mPaths.Where(x => x.id == id).FirstOrDefault();
            if (item != null)
            {
                mPaths.Remove(item);
                reDraw();
            }
        }

        public void end()
        {
            if (mCurrentPath != null)
            {
                if (mCurrentPath.isTranslucent)
                {
                    //FIXME - draw translucent not supported for now.
                    //mCurrentPath.draw(mDrawingCanvas);
                    //mTranslucentDrawingCanvas.drawColor(Color.TRANSPARENT, PorterDuff.Mode.MULTIPLY);
                }
                mCurrentPath = null;
            }


        }

        public void onSaved(bool success, string path)
        {
            var eventData = new JObject
            {
                { "success", success },
                { "path", path },
            };

            Context.GetJavaScriptModule<RCTEventEmitter>()
                .receiveEvent(this.GetTag(), "topChange", eventData);
        }

        public async void save(string format, string folder, string filename, bool transparent, bool includeImage, bool includeText, bool cropToImageSize)
        {
            //FIXME - Implement missing features
            if (includeImage == false || includeText == false || cropToImageSize == true)
                throw new NotImplementedException();

            var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);

            try
            {
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var fileFormat = format.Contains("png") ? "png" : "jpeg";

                    BitmapEncoder encoder = await BitmapEncoder
                        .CreateAsync(format.Contains("png") ? BitmapEncoder.PngEncoderId : BitmapEncoder.JpegEncoderId, stream);
                    Stream pixelStream = bitmap.PixelBuffer.AsStream();
                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, transparent ? BitmapAlphaMode.Premultiplied : BitmapAlphaMode.Ignore,
                        (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();
                    this.onSaved(true, file.Path);
                }
            }
            catch (Exception ex)
            {

                this.onSaved(false, null);
            }

        }

        public void onSizeChanged(int Width, int Height, int oldWidth, int oldHeight)
        {
            reDraw();
        }

        public async Task<string> getBase64(string format, bool transparent, bool includeImage, bool includeText, bool cropToImageSize)
        {
            //FIXME - Implement missing features
            if (includeImage == false || includeText == false || cropToImageSize == false)
                throw new NotImplementedException();

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    IRandomAccessStream stream = new InMemoryRandomAccessStream();

                    var fileFormat = format.Contains("png") ? "png" : "jpeg";

                    BitmapEncoder encoder = await BitmapEncoder
                        .CreateAsync(format.Contains("png") ? BitmapEncoder.PngEncoderId : BitmapEncoder.JpegEncoderId, stream);
                    Stream pixelStream = bitmap.PixelBuffer.AsStream();
                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, transparent ? BitmapAlphaMode.Premultiplied : BitmapAlphaMode.Ignore,
                        (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();

                    stream.AsStreamForRead().CopyTo(memoryStream);

                    return $"data:image/{fileFormat};base64," + Convert.ToBase64String(memoryStream.ToArray());
                }

            }
            catch (Exception)
            {
            }

            return null;
        }

        private WriteableBitmap createImage(bool transparent, bool includeImage, bool includeText, bool cropToImageSize)
        {
            //FIXME - Not in use for now, but should be implemented to support all features
            return BitmapFactory.New((int)this.Width, (int)this.Height);
        }

        private async void reDraw()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                bitmap = mBackgroundImage != null ? mBackgroundImage.Clone() : BitmapFactory.New((int)this.Width, (int)this.Height);

                this.image.Source = bitmap;
                this.image.Width = bitmap.PixelWidth;
                this.image.Height = bitmap.PixelHeight;

                try
                {
                    foreach (var item in mPaths)
                    {
                        item.draw(bitmap);
                    }
                }
                catch (Exception)
                {
                }
            });
        }

    }
}

