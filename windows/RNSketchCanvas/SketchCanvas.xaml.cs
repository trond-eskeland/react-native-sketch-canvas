using ReactNative.UIManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Graphics.Display;
using ReactNative.UIManager.Events;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Text;

namespace RNSketchCanvas
{
    public sealed partial class SketchCanvas : UserControl
    {
        private List<SketchData> mPaths = new List<SketchData>();
        private List<Model.ImageText> imageText = new List<Model.ImageText>();
        private SketchData mCurrentPath = null;


        public uint color { get; set; }
        public Windows.UI.Color color2 { get; set; }
        public Model.Point lastPoint { get; set; }
        public ThemedReactContext Context { get; set; }



        public bool didDrawBitmap { get; set; }
        public bool viewPortLocked { get; set; } = true;


        IRandomAccessStream imageStream;
        CanvasVirtualBitmap virtualBitmap;
        CanvasVirtualBitmapOptions virtualBitmapOptions;

        public SketchCanvas(ThemedReactContext Context)
        {
            this.InitializeComponent();

            this.Context = Context;


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
                //  scrollView.ChangeView(0, 0, null);
            });


            scrollView.DirectManipulationStarted += ScrollView_DirectManipulationStarted;
            scrollView.DirectManipulationCompleted += ScrollView_DirectManipulationCompleted;


            var _pointerHandlers = new Dictionary<RoutedEvent, PointerEventHandler>()
            {

                { UIElement.PointerMovedEvent, new PointerEventHandler(OnPointerMoved) },
                { UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased) },
                { UIElement.PointerCanceledEvent, new PointerEventHandler(OnPointerReleased) },
                { UIElement.PointerCaptureLostEvent, new PointerEventHandler(OnPointerReleased) },
                { UIElement.PointerExitedEvent, new PointerEventHandler(OnPointerReleased) },
            };

            var _pointersInViews = new Dictionary<uint, HashSet<DependencyObject>>();

            foreach (KeyValuePair<RoutedEvent, PointerEventHandler> handler in _pointerHandlers)
            {
                scrollView.AddHandler(handler.Key, handler.Value, true);


            }

            //this.Children.Add(scrollView);

        }
        private void Control_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollView.MaxWidth = double.MaxValue;
            scrollView.MaxHeight = double.MaxValue;
            scrollView.ZoomMode = ZoomMode.Enabled;

        }
        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            //IOGraph.RemoveFromVisualTree();
            canvas.RemoveFromVisualTree();

            //IOGraph = null;
            canvas = null;
        }

        private void Canvas_CreateResources(CanvasVirtualControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            if (imageStream != null)
            {
                args.TrackAsyncAction(LoadVirtualBitmap().AsAsyncAction());
            }
        }

        private bool didRender;

        private void Canvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            foreach (var region in args.InvalidatedRegions)
            {
                using (var ds = canvas.CreateDrawingSession(region))
                {
                    if (virtualBitmap != null)
                    {
                        ds.DrawImage(virtualBitmap, region, region);
                        try
                        {
                            foreach (var item in mPaths)
                            {
                                item.Draw(ds);
                                //Debug.WriteLine($"actual offset: x: {canvas.ActualOffset.X}, y: {canvas.ActualOffset.Y}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Canvas_RegionsInvalidated, draw line error" + ex);
                        }

                        foreach (var item in imageText)
                        {
                            ds.DrawText(item.text, new System.Numerics.Vector2(item.position.x, item.position.y), Helper.Utils.GetColor(item.fontColor),
                                              new CanvasTextFormat()
                                              {
                                                  //VerticalAlignment = CanvasVerticalAlignment.Center,
                                                  //HorizontalAlignment = CanvasHorizontalAlignment.Center,
                                                  //FontFamily = "Comic Sans MS",
                                                  FontSize = 30
                                              });
                        }
                    }

                }
            }

            if (!didRender)
            {
                didRender = true;
                Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {



                        if (virtualBitmap != null)
                        {
                            if (this.Width > this.virtualBitmap.SizeInPixels.Width || this.Height > this.virtualBitmap.SizeInPixels.Height)
                            {
                                scrollView.MinZoomFactor = 1.0f;
                                scrollView.MaxZoomFactor = 1.0f;
                            }
                            else
                            {
                                scrollView.MinZoomFactor = 0.1f;
                                scrollView.MaxZoomFactor = 10.0f;
                            }
                        }




                        var Succes = scrollView.ChangeView(null, null, 0.5f, false);
                        dispatchZoomEvent();
                    });


                }, TimeSpan.FromMilliseconds(100));


                Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Debug.WriteLine("dispatchZoomEvent - ensure image loaded...");
                        dispatchZoomEvent();
                    });


                }, TimeSpan.FromMilliseconds(1000));

            }
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

        private void dispatchZoomEvent()
        {

            try
            {


                // Debug.WriteLine("dispatchZoomEvent");

                if (virtualBitmap == null)
                    return;

                var zoom = scrollView.ZoomFactor;

                var screenImageRatioWidth = (this.virtualBitmap.SizeInPixels.Width / this.canvas.ActualWidth);
                var screenImageRatioHeight = (this.virtualBitmap.SizeInPixels.Height / this.canvas.ActualHeight);

                var horizontalScrollOffset = (scrollView.HorizontalOffset * screenImageRatioWidth);
                var horizontalCenterOffset = ((scrollView.ExtentWidth - this.Width) * screenImageRatioWidth) / 2;

                var verticalScrollOffset = (scrollView.VerticalOffset * screenImageRatioHeight);
                var verticalCenterOffset = ((scrollView.ExtentHeight - this.Height) * screenImageRatioHeight) / 2;

                var horizontalOffset = horizontalCenterOffset < 0 ? horizontalCenterOffset : horizontalScrollOffset;
                var verticalOffset = verticalCenterOffset < 0 ? verticalCenterOffset : verticalScrollOffset;



                double smallImageHorizontalOffset = 0;
                if (this.Width > this.virtualBitmap.SizeInPixels.Width)
                {
                    smallImageHorizontalOffset = ((this.virtualBitmap.SizeInPixels.Width - this.Width) / 2);
                }


                double smallImageVerticalOffset = 0;
                if (this.Height > this.virtualBitmap.SizeInPixels.Height)
                {
                    smallImageVerticalOffset = ((this.virtualBitmap.SizeInPixels.Height - this.Height) / 2);
                }



                this.GetReactContext()
                  .GetNativeModule<UIManagerModule>()
                  .EventDispatcher
                  .DispatchEvent(
                      new SketchCanvasZoomChangedEvent(
                          this.GetTag(),
                            zoom,
                            screenImageRatioWidth,
                            screenImageRatioHeight,
                            smallImageHorizontalOffset == 0 ? horizontalOffset : smallImageHorizontalOffset,
                            smallImageVerticalOffset == 0 ? verticalOffset : smallImageVerticalOffset));


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            ReDraw();
        }


        public async Task<bool> openImageFile(string filename, string directory, string mode)
        {
            scrollView.MaxWidth = double.MaxValue;
            scrollView.MaxHeight = double.MaxValue;

            var restult = false;
            try
            {
                Windows.Storage.StorageFile imageFile = await StorageFile.GetFileFromPathAsync(filename);

                imageStream = await imageFile.OpenReadAsync();
                await LoadVirtualBitmap();

                mPaths.Clear();
                imageText.Clear();
                didDrawBitmap = false;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("openImageFile, " + ex);
            }

            ReDraw();

            return restult;
        }

        private async Task LoadVirtualBitmap()
        {
            if (virtualBitmap != null)
            {
                virtualBitmap.Dispose();
                virtualBitmap = null;
            }

            didRender = false;

            virtualBitmapOptions = CanvasVirtualBitmapOptions.None;

            virtualBitmap = await CanvasVirtualBitmap.LoadAsync(canvas.Device, imageStream, virtualBitmapOptions);

            if (canvas == null)
            {
                // This can happen if the page is unloaded while LoadAsync is running
                return;
            }

            var size = virtualBitmap.Size;

            canvas.Width = size.Width;
            canvas.Height = size.Height;

            ReDraw();
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

            ReDraw();
        }

        public void clear()
        {
            mPaths = new List<SketchData>();
            ReDraw();
        }

        public void newPath(int id, Windows.UI.Color strokeColor, int strokeWidth)
        {
            mCurrentPath = new SketchData(id, strokeColor, strokeWidth);
            mPaths.Add(mCurrentPath);
        }

        public Model.Point getAbsolutePoint(Model.Point canvasPoint)
        {
            var zoom = scrollView.ZoomFactor;

            var imageBounds = this.virtualBitmap.SizeInPixels;

            var screenImageRatioWidth = imageBounds.Width / this.canvas.ActualWidth;
            var screenImageRatioHeight = imageBounds.Height / this.canvas.ActualHeight;
            //var screenImageRatioWidth = this.virtualBitmap.SizeInPixels.Width / this.Width;
            //var screenImageRatioHeight = this.virtualBitmap.SizeInPixels.Height / this.Height;



            var horizontalScrollOffset = (scrollView.HorizontalOffset * screenImageRatioWidth);
            var horizontalCenterOffset = ((scrollView.ExtentWidth - this.Width) * screenImageRatioWidth) / 2;

            var verticalScrollOffset = (scrollView.VerticalOffset * screenImageRatioHeight);
            var verticalCenterOffset = ((scrollView.ExtentHeight - this.Height) * screenImageRatioHeight) / 2;

            var horizontalOffset = horizontalCenterOffset < 0 ? horizontalCenterOffset : horizontalScrollOffset;
            var verticalOffset = verticalCenterOffset < 0 ? verticalCenterOffset : verticalScrollOffset;


            double smallImageHorizontalOffset = 0;
            if (this.Width > imageBounds.Width)
            {
                smallImageHorizontalOffset = ((imageBounds.Width - this.Width) / 2);
                // smallImagehorizontalOffset = 0;
            }


            double smallImageVerticalOffset = 0;
            if (this.Height > imageBounds.Height)
            {
                smallImageVerticalOffset = ((imageBounds.Height - this.Height) / 2);
            }


            var zoomPoint = new Model.Point(

              smallImageHorizontalOffset == 0 ? (int)(((canvasPoint.x * screenImageRatioWidth) + horizontalOffset) / zoom) : (int)(((canvasPoint.x * screenImageRatioWidth) + smallImageHorizontalOffset) / zoom),
              smallImageVerticalOffset == 0 ? (int)(((canvasPoint.y * screenImageRatioHeight) + verticalOffset) / zoom) : (int)(((canvasPoint.y * screenImageRatioHeight) + smallImageVerticalOffset) / zoom)

            );

            // Debug.WriteLine(" Draw on image: " + zoomPoint.ToString() + ", zoom: " + zoom.ToString() + ", smallImagehorizontalOffset:" + smallImageHorizontalOffset.ToString());
            return zoomPoint;
        }

        public void addPoint(Model.Point point)
        {
            if (mCurrentPath != null)
            {
                try
                {
                    var zoomPoint = getAbsolutePoint(point);
                    mCurrentPath.AddPoint(zoomPoint);
                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex.Message);
                }
        
                ReDraw();
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
                ReDraw();
            }
            else if (count > 0)
            {
                mPaths.RemoveAt(count - 1);
                ReDraw();
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
                    CanvasDevice device = CanvasDevice.GetSharedDevice();
                    CanvasRenderTarget offscreen = new CanvasRenderTarget(canvas, virtualBitmap.SizeInPixels.Width, virtualBitmap.SizeInPixels.Height, 96);
                    using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
                    {
                        ds.Clear(Colors.White);
                        ds.DrawImage(virtualBitmap);
                        try
                        {
                            foreach (var item in mPaths)
                            {
                                item.Draw(ds);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("draw line error: " + ex);
                        }

                        foreach (var item in imageText)
                        {
                            ds.DrawText(item.text, new System.Numerics.Vector2(item.position.x, item.position.y), Helper.Utils.GetColor(item.fontColor));
                        }

                    }
                    await offscreen.SaveAsync(stream, CanvasBitmapFileFormat.Png);

                    //var bounds = virtualBitmap.Bounds;
                    //await CanvasImage.SaveAsync(virtualBitmap, bounds, 96, device, stream, CanvasBitmapFileFormat.Jpeg);
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
            ReDraw();
        }

        public async Task<string> getBase64(string format, bool transparent, bool includeImage, bool includeText, bool cropToImageSize)
        {
            ////FIXME - Implement missing features
            //if (includeImage == false || includeText == false || cropToImageSize == false)
            //    throw new NotImplementedException();

            //try
            //{
            //    using (var memoryStream = new MemoryStream())
            //    {
            //        IRandomAccessStream stream = new InMemoryRandomAccessStream();

            //        var fileFormat = format.Contains("png") ? "png" : "jpeg";

            //        BitmapEncoder encoder = await BitmapEncoder
            //            .CreateAsync(format.Contains("png") ? BitmapEncoder.PngEncoderId : BitmapEncoder.JpegEncoderId, stream);
            //        Stream pixelStream = bitmap.PixelBuffer.AsStream();
            //        byte[] pixels = new byte[pixelStream.Length];
            //        await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            //        encoder.SetPixelData(BitmapPixelFormat.Bgra8, transparent ? BitmapAlphaMode.Premultiplied : BitmapAlphaMode.Ignore,
            //            (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96.0, 96.0, pixels);
            //        await encoder.FlushAsync();

            //        stream.AsStreamForRead().CopyTo(memoryStream);

            //        return $"data:image/{fileFormat};base64," + Convert.ToBase64String(memoryStream.ToArray());
            //    }

            //}
            //catch (Exception)
            //{
            //}

            //return null;
            return "";
        }

        public void lockViewPort(bool enabled)
        {
            this.viewPortLocked = enabled;
            scrollView.VerticalScrollMode = enabled ? ScrollMode.Enabled : ScrollMode.Disabled;
            scrollView.HorizontalScrollMode = enabled ? ScrollMode.Enabled : ScrollMode.Disabled;

            scrollView.ManipulationMode = enabled ? ManipulationModes.TranslateY : ManipulationModes.TranslateY;
            end();
        }

        private void ReDraw()
        {
            try
            {
                if (canvas != null)
                    canvas.Invalidate();
            }
            catch (Exception)
            {
            }

        }
    }
}
