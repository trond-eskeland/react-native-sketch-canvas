//using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using Newtonsoft.Json.Linq;
using ReactNative.Collections;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.Graphics.Imaging;
using ReactNative.UIManager;
using System.Diagnostics;

namespace RNSketchCanvas
{
    class SketchCanvasModule : ReactContextNativeModuleBase
    {
        private const string MODULE_NAME = "SketchCanvasModule";
        public SketchCanvasModule(ReactContext reactContext) : base(reactContext)
        {
        }

        public override string Name => MODULE_NAME;

        [ReactMethod]
        public void transferToBase64(int tag, string type, bool transparent, bool includeImage, bool includeText, bool cropToImageSize, ICallback callback)
        {

            var uiManager = Context.GetNativeModule<UIManagerModule>();
            try
            {
                uiManager.AddUIBlock(new UIBlock(async (nativeViewHierarchyManager) =>
                {
                    var view = (SketchCanvas)nativeViewHierarchyManager.ResolveView(tag);
                    var bitmap = view.bitmap;
                    var base64 = await view.getBase64(type, transparent, includeImage, includeText, cropToImageSize);


                    callback.Invoke(null, base64);
                }
                ));
            }
            catch (Exception ex)
            {
                callback.Invoke(ex.Message, null);
            }
        }


        //[ReactMethod]
        //public void getZoomAndOffset(int tag, ICallback callback)
        //{

        //  var uiManager = Context.GetNativeModule<UIManagerModule>();
        //  try
        //  {
        //    uiManager.AddUIBlock(new UIBlock(async (nativeViewHierarchyManager) =>
        //    {
        //      var view = (SketchCanvas)nativeViewHierarchyManager.ResolveView(tag);
        //      var base64 = await view.getBase64(type, transparent, includeImage, includeText, cropToImageSize);



        //      callback.Invoke(null, base64);
        //    }
        //    ));
        //  }
        //  catch (Exception ex)
        //  {
        //    callback.Invoke(ex.Message, null);
        //  }
        //}


    }
}


