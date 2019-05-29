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
      this.image.Stretch = Stretch.None;

      scrollView.ZoomMode = ZoomMode.Enabled;
      scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
      scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      scrollView.Content = image;


      //image.ImageOpened += (o, eventArgs) =>
      //{
      //  var ratioWidth = scrollView.ViewportWidth / image.ActualWidth;
      //  var ratioHeight = scrollView.ViewportHeight / image.ActualHeight;
      //  var zoomFactor = (ratioWidth >= 1 && ratioHeight >= 1)
      //      ? 1F
      //      : (float)(Math.Min(ratioWidth, ratioHeight));
      //  scrollView.MinZoomFactor = zoomFactor;
      //  scrollView.ChangeView(null, null, zoomFactor);
      //};


      scrollView.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, (s, e) =>
      {
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

      this.Children.Add(scrollView);


    }

    private void dispatchZoomEvent()
    {
      this.GetReactContext()
        .GetNativeModule<UIManagerModule>()
        .EventDispatcher
        .DispatchEvent(
            new SketchCanvasZoomChangedEvent(
                this.GetTag(),
                scrollView.ZoomFactor,
                 scrollView.HorizontalOffset,
                 scrollView.VerticalOffset));
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

      if (finalSize.Width > 0 )
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



    void CreateScrollViewer()
    {
      Canvas canvas1 = new Canvas();
      canvas1.Height = 640;
      canvas1.Width = 480;
      canvas1.Margin = new Thickness(0, 0, 0, 0);
      //canvas1.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x25, 0x72, 0x93));
      canvas1.HorizontalAlignment = HorizontalAlignment.Left;
      canvas1.VerticalAlignment = VerticalAlignment.Top;

      ScrollViewer scrV = new ScrollViewer();
      scrV.Height = 100;
      scrV.Width = 200;
      scrV.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
      Canvas.SetTop(scrV, 60);
      Canvas.SetLeft(scrV, 60);

      TextBlock tBlk = new TextBlock();
      tBlk.Width = 300;
      tBlk.TextWrapping = TextWrapping.Wrap;
      tBlk.Text = "But how am I supposed to add elements like this to a scrollViewer. I tried adding stackpanel to scrollviewer using xaml and then add elements in that stackpanel using c# but when I did this there is no scroll available";
      scrV.Content = tBlk;
      canvas1.Children.Add(scrV);
      this.stack.Children.Add(canvas1);
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

    public void addPoint(Model.Point point)
    {

      var zoom = scrollView.ZoomFactor;
      var zoomPoint = new Model.Point
        ((int)((point.x + scrollView.HorizontalOffset) / zoom),
        (int)((point.y + scrollView.VerticalOffset) / zoom)

        );


      Debug.WriteLine($"zoomFactor: {zoom}, point x: {point.x}, point y: {point.y}, calc x: {zoomPoint.x}, calc y: {zoomPoint.y}");


     

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
      var ratioWidth = scrollView.ViewportWidth / image.ActualWidth;
      var ratioHeight = scrollView.ViewportHeight / image.ActualHeight;
      var zoomFactor = (ratioWidth >= 1 && ratioHeight >= 1)
          ? 1F
          : (float)(Math.Min(ratioWidth, ratioHeight));
      scrollView.MinZoomFactor = zoomFactor;
      scrollView.ChangeView(null, null, zoomFactor);


      return mBackgroundImage != null ? mBackgroundImage.Clone() : BitmapFactory.New((int)this.Width, (int)this.Height);
      //return BitmapFactory.New((int)this.Width, (int)this.Height);
    }

    private async void reDraw()
    {

      this.UpdateLayout();

      await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
      () =>
      {
        bitmap = createImage(false,false,false,false);

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
      });
    }

  }
}

