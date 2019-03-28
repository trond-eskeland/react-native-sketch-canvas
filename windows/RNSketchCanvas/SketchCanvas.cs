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
using System.Diagnostics;
using Newtonsoft.Json;

namespace RNSketchCanvas
{
    public class SketchCanvas : Panel
    {
        public WriteableBitmap bitmap { get; set; }
        private WriteableBitmap mBackgroundImage { get; set; }

        private List<SketchData> mPaths = new List<SketchData>();
        private List<Model.ImageText> imageText = new List<Model.ImageText>();
        private SketchData mCurrentPath = null;
        private StackPanel stack = new StackPanel() { HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center, VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center};
        private Image image = new Image() { Name = "img" };
        public ThemedReactContext Context { get; set; }

        public SketchCanvas(ThemedReactContext Context)
        {
            this.Context = Context;

            this.Margin = new Windows.UI.Xaml.Thickness(10, 10, 10, 10);

            stack.Children.Add(image);
            this.image.Stretch = Stretch.None;

            this.Children.Add(stack);
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


        public void setCanvasText(JArray text)
        {
            this.imageText = new List<Model.ImageText>();

            foreach (var item in text)
            {
                try
                {
                    imageText.Add(item.ToObject<Model.ImageText>());
                }
                catch (Exception)
                {
                }
            }

            reDraw();
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

        public  void addPoint(Model.Point point)
        {
            mCurrentPath.addPoint(point);

            if (mCurrentPath.isTranslucent)
            {
                //FIXME
                //mTranslucentDrawingCanvas.drawColor(Color.TRANSPARENT, PorterDuff.Mode.MULTIPLY);
                //mCurrentPath.draw(mTranslucentDrawingCanvas);
            }
            else
            {
                mCurrentPath.drawLastPoint(bitmap);
            }
        }

        public void addPath(int id, int strokeColor, float strokeWidth, List<Model.Point> points)
        {
            //FIXME
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
           //  reDraw();
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

                try
                {
                    foreach (var item in imageText)
                    {
                        //FIXME, only arial size 15 is supported.
                        bitmap.DrawString(item.text, item.position.x, item.position.y, "arial", 15, item.fontColor);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }


                this.image.Source = bitmap;
                //this.image.Width = bitmap.PixelWidth;
                //this.image.Height = bitmap.PixelHeight;
            });
        }

    }
}

