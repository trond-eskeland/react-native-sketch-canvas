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
using Windows.UI.Xaml;

namespace RNSketchCanvas
{
    public class SketchCanvas : Panel
    {
        public WriteableBitmap bitmap { get; set; }
        private WriteableBitmap mBackgroundImage { get; set; }

        private List<SketchData> mPaths = new List<SketchData>();
        private List<Model.ImageText> imageText = new List<Model.ImageText>();
        private SketchData mCurrentPath = null;
        private StackPanel stack = new StackPanel() { HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center, VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center };
        private Image image = new Image() { Name = "img" };
        public ThemedReactContext Context { get; set; }

        private ScrollViewer scrollView = new ScrollViewer();
        public SketchCanvas(ThemedReactContext Context)
        {
            this.Context = Context;
            this.Margin = new Windows.UI.Xaml.Thickness(10, 10, 10, 10);
            this.image.Stretch = Stretch.Uniform;

            scrollView.ZoomMode = ZoomMode.Enabled;
            scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollView.Content = image;



            scrollView.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, (s, e) =>
            {
                Debug.WriteLine($"zoom: {scrollView.ZoomFactor}");
                dispatchZoomEvent();
            });
            scrollView.RegisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, (s, e) =>
            {
                dispatchZoomEvent();
            });
            scrollView.RegisterPropertyChangedCallback(ScrollViewer.HorizontalOffsetProperty, (s, e) =>
            {
                dispatchZoomEvent();
            });


            scrollView.RegisterPropertyChangedCallback(ScrollViewer.MinZoomFactorProperty, (s, e) =>
            {
                scrollView.ChangeView(0, 0, null);
            });


            this.Children.Add(scrollView);


        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("img loaded");
        }

        private void dispatchZoomEvent()
        {

            var aspectOffsetHorizontal = this.Width > this.bitmap.PixelWidth ?
            (int)((this.Width - this.bitmap.PixelWidth) / 2) : 0;

            var aspectOffsetVertical = this.Height > this.bitmap.PixelHeight ?
            (int)((this.Height - this.bitmap.PixelHeight) / 2) : 0;

            this.GetReactContext()
              .GetNativeModule<UIManagerModule>()
              .EventDispatcher
              .DispatchEvent(
                  new SketchCanvasZoomChangedEvent(
                      this.GetTag(),
                      scrollView.ZoomFactor,
                       scrollView.HorizontalOffset + aspectOffsetHorizontal,
                       scrollView.VerticalOffset + aspectOffsetVertical));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = new Size(0, 0);

            //foreach (UIElement child in Children)
            //{
            //    child.Measure(availableSize);
            //    resultSize.Width = Math.Max(size.Width, child.DesiredSize.Width);
            //    resultSize.Height = Math.Max(size.Height, child.DesiredSize.Height);
            //}

            size.Width = double.IsPositiveInfinity(availableSize.Width) ?
               size.Width : availableSize.Width;

            size.Height = double.IsPositiveInfinity(availableSize.Height) ?
               size.Height : availableSize.Height;

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {

            if (finalSize.Width > 0)
            {
                foreach (UIElement child in Children)
                {
                    child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
                    this.image.MaxHeight = finalSize.Height;
                    this.image.MaxWidth = finalSize.Width;
                }
            }

            return finalSize;
        }


        public async Task<bool> openImageFile(string filename, string directory, string mode)
        {
            var restult = false;
            try
            {
                Windows.Storage.StorageFile imageFile = await StorageFile.GetFileFromPathAsync(filename);

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

            //var origHeight = sourceImage.PixelHeight;
            //var origWidth = sourceImage.PixelWidth;

            //var ratioX = maxWidth / (float)origWidth;
            //var ratioY = maxHeight / (float)origHeight;
            //var ratio = Math.Min(ratioX, ratioY);

            //var newHeight = (int)(origHeight * ratio);
            //var newWidth = (int)(origWidth * ratio);

            //return sourceImage.Resize(newWidth, newHeight, interpolation);

            return sourceImage;

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

        public Model.Point getAbsolutePoint(Model.Point canvasPoint)
        {
            var screenImageRatioWidth = this.bitmap.PixelWidth / this.image.ActualWidth;

            var screenImageRatioHeight = this.bitmap.PixelHeight / this.image.ActualHeight;

            var ratio = this.bitmap.PixelWidth / this.bitmap.PixelWidth;

            var aspectOffsetHorizontal = this.Width > this.bitmap.PixelWidth && scrollView.HorizontalOffset == 0 ?
                (int)(((this.Width - this.bitmap.PixelWidth) / 2) - (scrollView.VerticalOffset * ratio) - scrollView.VerticalOffset) : 0;

            //var aspectOffsetVertical = this.Height > this.bitmap.PixelHeight && scrollView.VerticalOffset == 0 ?
            //    (int)((this.Height - this.bitmap.PixelHeight) / 2) : 0;

            var zoom = scrollView.ZoomFactor;

            //var xFactor = (int)((screenImageRatioWidth + (scrollView.HorizontalOffset * screenImageRatioWidth)) / zoom);
            //var yFactor = (int)((screenImageRatioHeight + (scrollView.VerticalOffset * screenImageRatioHeight)) / zoom);

            //var zoomPoint = new Model.Point((int)(canvasPoint.x * screenImageRatioWidth), (int)(canvasPoint.y * screenImageRatioHeight));

            var zoomPoint = new Model.Point(

              (int)(((canvasPoint.x * screenImageRatioWidth) + (scrollView.HorizontalOffset * screenImageRatioWidth) - aspectOffsetHorizontal) / zoom),
              (int)(((canvasPoint.y * screenImageRatioHeight) + (scrollView.VerticalOffset * screenImageRatioHeight)) / zoom)

            );

            return zoomPoint;
        }

        public void addPoint(Model.Point point)
        {

            /*
             *  working with full zoom out and zoom in (not when centered in middle)
             
             var aspectOffsetHorizontal = this.Width > this.bitmap.PixelWidth && scrollView.HorizontalOffset == 0 ?
                (int)(((this.Width - this.bitmap.PixelWidth) / 2) - (scrollView.VerticalOffset * ratio) - scrollView.VerticalOffset) : 0;


            var zoom = scrollView.ZoomFactor;
            var zoomPoint = new Model.Point
              ((int)((point.x + scrollView.HorizontalOffset - aspectOffsetHorizontal) / zoom),
              (int)((point.y + scrollView.VerticalOffset / screenImageRatioH) / zoom)

              );
             * 
             * 
             */

            //var screenImageRatioWidth = this.bitmap.PixelWidth / this.image.ActualWidth;

            //var screenImageRatioHeight = this.bitmap.PixelHeight / this.image.ActualHeight;

            //var ratio = this.bitmap.PixelWidth / this.bitmap.PixelWidth;

            //var aspectOffsetHorizontal = this.Width > this.bitmap.PixelWidth && scrollView.HorizontalOffset == 0 ?
            //    (int)(((this.Width - this.bitmap.PixelWidth) / 2) - (scrollView.VerticalOffset * ratio) - scrollView.VerticalOffset) : 0;

            ////var aspectOffsetVertical = this.Height > this.bitmap.PixelHeight && scrollView.VerticalOffset == 0 ?
            ////    (int)((this.Height - this.bitmap.PixelHeight) / 2) : 0;

            //var zoom = scrollView.ZoomFactor;
            //var zoomPoint = new Model.Point(

            //  (int)(((point.x * screenImageRatioWidth) + (scrollView.HorizontalOffset * screenImageRatioWidth) - aspectOffsetHorizontal) / zoom),
            //  (int)(((point.y * screenImageRatioHeight) + (scrollView.VerticalOffset * screenImageRatioHeight)) / zoom)

            //);


            //Debug.WriteLine($"ratio: {ratio}, aspectOffsetHorizontal: {aspectOffsetHorizontal}, scrollView.HorizontalOffset: {scrollView.HorizontalOffset}, scrollView.VerticalOffset: {scrollView.VerticalOffset}");
            //Debug.WriteLine($"zoomFactor: {zoom}, point x: {point.x}, point y: {point.y}, calc x: {zoomPoint.x}, calc y: {zoomPoint.y}");

            var zoomPoint = getAbsolutePoint(point);



            mCurrentPath.addPoint(zoomPoint);

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

            if (this.Width > 0 && this.Height > 0)
            {
                return mBackgroundImage != null ? mBackgroundImage.Clone() : BitmapFactory.New((int)this.Width * 5, (int)this.Height * 5);
            }

            return null;
            //return BitmapFactory.New((int)this.Width, (int)this.Height);
        }

        private async void reDraw()
        {

            //this.UpdateLayout();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                var bitmapWasNull = bitmap == null;
                bitmap = createImage(false, false, false, false);

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


                if (bitmapWasNull && bitmap != null)
                {
                    //  var ratioWidth = scrollView.ViewportWidth / image.ActualWidth;
                    //  var ratioHeight = scrollView.ViewportHeight / image.ActualHeight;
                    //  var zoomFactor = (ratioWidth >= 1 && ratioHeight >= 1)
                    //? 1F
                    //: (float)(Math.Min(ratioWidth, ratioHeight));
                    //  scrollView.MinZoomFactor = zoomFactor;
                    //  scrollView.ChangeView(null, null, zoomFactor);


                    //this.image.MaxWidth = bitmap.PixelWidth;
                    //this.image.MaxHeight = bitmap.PixelHeight;

                    //test 2
                    //  var ratioWidth = scrollView.ViewportWidth / bitmap.PixelWidth;
                    //  var ratioHeight = scrollView.ViewportHeight / bitmap.PixelHeight;
                    //  var zoomFactor = (ratioWidth >= 1 && ratioHeight >= 1)
                    //? 1F
                    //: (float)(Math.Min(ratioWidth, ratioHeight));
                    //  scrollView.MinZoomFactor = 1.0f;
                    //  scrollView.ChangeView(null, null, zoomFactor);

                    

                    var period = TimeSpan.FromMilliseconds(10);
                    Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {

                            //var offset = (this.Width - image.ActualWidth) / 2;

                          //  var _ratioWidth = scrollView.ViewportWidth / image.ActualWidth;
                          //  var _ratioHeight = scrollView.ViewportHeight / image.ActualHeight;
                          //  var _zoomFactor = (_ratioWidth >= 1 && _ratioHeight >= 1)
                          //? 1F
                          //: (float)(Math.Min(_ratioWidth, _ratioHeight));
       


                            scrollView.MinZoomFactor = 2.0f; //zoomFactor; // 1.61051f


                            var Succes = scrollView.ChangeView(null, null, 0.2f, false);
                        });
                    }, period);

                }

                

            });

        }

    }
}

