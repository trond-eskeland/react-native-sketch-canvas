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
using ReactNative.Views.Scroll;
using Windows.UI.Xaml.Input;
using Windows.UI.Input;

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
        public uint color { get; set; }
        public Model.Point lastPoint { get; set; }
        public ThemedReactContext Context { get; set; }

        private ScrollViewer scrollView = new ScrollViewer();

        public bool didDrawBitmap { get; set; }
        public bool viewPortLocked { get; set; } = true;
        public SketchCanvas(ThemedReactContext Context)
        {
            this.Context = Context;
            this.Margin = new Windows.UI.Xaml.Thickness(10, 10, 10, 10);
            this.image.Stretch = Stretch.Uniform;

            scrollView.ZoomMode = ZoomMode.Enabled;
            scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollView.Content = image;


            viewPortLocked = true;




            scrollView.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, (s, e) =>
            {
                // Debug.WriteLine($"zoom: {scrollView.ZoomFactor}");
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

            scrollView.DirectManipulationStarted += ScrollView_DirectManipulationStarted;
            scrollView.DirectManipulationCompleted += ScrollView_DirectManipulationCompleted;


            var _pointerHandlers = new Dictionary<RoutedEvent, PointerEventHandler>()
            {
                { UIElement.PointerPressedEvent, new PointerEventHandler(OnPointerPressed) },
                { UIElement.PointerMovedEvent, new PointerEventHandler(OnPointerMoved) },
                { UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased) },
                { UIElement.PointerCanceledEvent, new PointerEventHandler(OnPointerCanceled) },
                { UIElement.PointerCaptureLostEvent, new PointerEventHandler(OnPointerCaptureLost) },
                { UIElement.PointerExitedEvent, new PointerEventHandler(OnPointerExited) },
            };

            var _pointersInViews = new Dictionary<uint, HashSet<DependencyObject>>();

            foreach (KeyValuePair<RoutedEvent, PointerEventHandler> handler in _pointerHandlers)
            {
                scrollView.AddHandler(handler.Key, handler.Value, true);


            }

            this.Children.Add(scrollView);

        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //this.OnPointerPressed(this, e);


            //var viewPoint = e.GetCurrentPoint(scrollView);
            //Debug.WriteLine(viewPoint.Position.ToString());
        }



        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var viewPoint = e.GetCurrentPoint(scrollView);

            if (mCurrentPath != null
                && !viewPortLocked
                && viewPoint.PointerId != 2
                && (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch
                     || e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen))
            {

                var point = new Model.Point((int)viewPoint.Position.X, (int)viewPoint.Position.Y);




                if ((lastPoint == null || !lastPoint.Equals(point)) && viewPoint.Properties.IsPrimary)
                {
                    // Debug.WriteLine($"add point: x:{viewPoint.Position.X}, y:{viewPoint.Position.Y}, poinderId: {viewPoint.PointerId}, raw-x:{viewPoint.RawPosition.X}, raw-y:{viewPoint.RawPosition.Y} ");
                    addPoint(point);
                    lastPoint = point;

                }


            }

        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerReleased");
            end();
        }

        private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerCanceled");
            end();
        }

        private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerCaptureLost");
            end();
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerExited");
            end();

        }





        private void ScrollView_DirectManipulationStarted(object sender, object e)
        {
            if (!viewPortLocked)
            {
                if (mCurrentPath != null)
                    mCurrentPath.points.Clear();
                scrollView.CancelDirectManipulations();
                Debug.WriteLine("ScrollView_DirectManipulationStarted");
            }

        }

        private void ScrollView_DirectManipulationCompleted(object sender, object e)
        {
            Debug.WriteLine("ScrollView_DirectManipulationCompleted");
            // this.end();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("img loaded");
        }

        private void dispatchZoomEvent()
        {




            var screen = DisplayInformation.GetForCurrentView();

            var scale = (float)screen.ResolutionScale / 100;

            var screenImageRatioWidth = (this.bitmap.PixelWidth / this.image.ActualWidth);
            var screenImageRatioHeight = (this.bitmap.PixelHeight / this.image.ActualHeight);

            //var w = this.Width;
            //var h = this.Height;

            //var t = this.Width / this.image.ActualWidth;

            //var t1 = (scrollView.ExtentWidth - this.Width);
            //var t2 = t1 * screenImageRatioWidth;
            //var t3 = t1 - (t1 * 2);

            //var before = (scrollView.HorizontalOffset * screenImageRatioWidth);



            var horizontalScrollOffset = (scrollView.HorizontalOffset * screenImageRatioWidth);
            var horizontalCenterOffset = ((scrollView.ExtentWidth - this.Width) * screenImageRatioWidth) / 2;

            var verticalScrollOffset = (scrollView.VerticalOffset * screenImageRatioHeight);
            var verticalCenterOffset = ((scrollView.ExtentHeight - this.Height) * screenImageRatioHeight) / 2;

            var horizontalOffset = horizontalCenterOffset < 0 ? horizontalCenterOffset : horizontalScrollOffset; // t2 / 2; //t3 * screenImageRatioWidth; // 100; //((scrollView.ExtentWidth - this.Width)) - (((scrollView.ExtentWidth - this.Width)) * 2); //(
            var verticalOffset = verticalCenterOffset < 0 ? verticalCenterOffset : verticalScrollOffset; // (scrollView.VerticalOffset * screenImageRatioHeight);

            var zoom = scrollView.ZoomFactor;
            // Debug.WriteLine($"scale: {scale}, widthRatio: {screenImageRatioWidth}, heightRatio: {screenImageRatioHeight}, actualWidth: {image.Width}, actualHeight: {image.Height} ");

            if (this.bitmap.PixelWidth == 1)
                return;


            this.GetReactContext()
              .GetNativeModule<UIManagerModule>()
              .EventDispatcher
              .DispatchEvent(
                  new SketchCanvasZoomChangedEvent(
                      this.GetTag(),
                        zoom,
                        screenImageRatioWidth,
                        screenImageRatioHeight,
                        horizontalOffset,
                        verticalOffset));
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
                mPaths.Clear();
                imageText.Clear();
                didDrawBitmap = false;

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

            var HorizontalOffset = (scrollView.HorizontalOffset * screenImageRatioWidth);
            var VerticalOffset = (scrollView.VerticalOffset * screenImageRatioHeight);



            //var aspectOffsetHorizontal = this.Width > this.bitmap.PixelWidth && scrollView.HorizontalOffset == 0 ?
            //    (int)(((this.Width - this.bitmap.PixelWidth) / 2) - (scrollView.VerticalOffset * ratio) - scrollView.VerticalOffset) : 0;

            //var aspectOffsetVertical = this.Height > this.bitmap.PixelHeight && scrollView.VerticalOffset == 0 ?
            //    (int)((this.Height - this.bitmap.PixelHeight) / 2) : 0;

            var zoom = scrollView.ZoomFactor;

            var zoomPoint = new Model.Point(

              (int)(((canvasPoint.x * screenImageRatioWidth) + HorizontalOffset) / zoom),
              (int)(((canvasPoint.y * screenImageRatioHeight) + VerticalOffset) / zoom)

            );

            return zoomPoint;
        }

        public void addPoint(Model.Point point)
        {
            if (mCurrentPath != null)
            {
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

        }

        public void addPath(int id, int strokeColor, float strokeWidth, List<Model.Point> points)
        {
            //FIXME
        }

        public void deletePath(int id)
        {
            var count = mPaths.Count;
            var item = mPaths.Where(x => x.id == id).FirstOrDefault();
            if (item != null)
            {
                mPaths.Remove(item);
                reDraw();
            }
            else if (count > 0)
            {
                mPaths.RemoveAt(count - 1);
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
                return mBackgroundImage != null ? mBackgroundImage.Clone() : BitmapFactory.New((int)1, (int)1);
            }

            return null;
            //return BitmapFactory.New((int)this.Width, (int)this.Height);
        }

        public void lockViewPort(bool enabled)
        {
            this.viewPortLocked = enabled;
            scrollView.VerticalScrollMode = enabled ? ScrollMode.Enabled : ScrollMode.Disabled;
            scrollView.HorizontalScrollMode = enabled ? ScrollMode.Enabled : ScrollMode.Disabled;

            scrollView.ManipulationMode = enabled ? ManipulationModes.TranslateY : ManipulationModes.TranslateY;
            end();
        }

        private async void reDraw()
        {

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
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


                if (bitmap != null && bitmap.PixelWidth != 1)
                {

                    if (!didDrawBitmap)
                    {
                        Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {



                                scrollView.MinZoomFactor = 2.0f; //zoomFactor; // 1.61051f

                                var Succes = scrollView.ChangeView(null, null, 0.2f, false);
                            });
                        }, TimeSpan.FromMilliseconds(10));
                    }

                    didDrawBitmap = true;
                }



            });

        }

    }


}

