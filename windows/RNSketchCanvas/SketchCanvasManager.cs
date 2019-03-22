using Newtonsoft.Json.Linq;
using ReactNative.UIManager;
using ReactNative.UIManager.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using ReactNative.Bridge;
using ReactNative.Views.Image;

namespace RNSketchCanvas
{
    public class SketchCanvasManager : SimpleViewManager<SketchCanvas>
    {
        public override string Name => "RNSketchCanvas";
        public SketchCanvas Canvas = null;

        protected override SketchCanvas CreateViewInstance(ThemedReactContext reactContext)
        {
            this.Canvas = new SketchCanvas(reactContext);
            return this.Canvas;
        }


        public override void SetDimensions(SketchCanvas view, Dimensions dimensions)
        {
            view.onSizeChanged((int)dimensions.Width, (int)dimensions.Height, (int)this.Canvas.Width, (int)this.Canvas.Height);
            base.SetDimensions(view, dimensions);

        }

        [ReactProp("localSourceImage")]
        public void LocalSourceImage(SketchCanvas view, JObject options)
        {
            if (options != null)
            {
                var filename = options["filename"]?.Value<string>();
                var directory = options["directory"]?.Value<string>();
                var mode = options["mode"]?.Value<string>();
                var uiManager = view.GetReactContext().GetNativeModule<UIManagerModule>();

                try
                {
                    uiManager.AddUIBlock(new UIBlock(async (nativeViewHierarchyManager) =>
                    {
                        var uiview = (SketchCanvas)nativeViewHierarchyManager.ResolveView(view.GetTag());

                        await uiview.openImageFile(filename, directory, mode);
                    }
                    ));
                }
                catch (Exception)
                {

                }

            }
        }

        public override void ReceiveCommand(SketchCanvas view, int commandId, JArray args)
        {
            base.ReceiveCommand(view, commandId, args);
            int id = 0;

            switch ((Commands)commandId)
            {
                case Commands.addPoint:
                    view.addPoint(new Point(args));
                    break;
                case Commands.newPath:
                    id = args[0].Value<int>();
                    var color = args[1].Value<UInt32>();
                    var strokeWidth = args[2].Value<float>();

                    view.newPath(id, color, (int)strokeWidth);
                    break;
                case Commands.clear:
                    view.clear();
                    break;
                case Commands.addPath:
                    //fixme
                    view.addPath(0, 0, 0, new List<Point>());
                    break;
                case Commands.deletePath:
                    id = args[0].Value<int>();
                    view.deletePath(id);
                    break;
                case Commands.save:
                    var format = args[0].Value<string>();
                    var folder = args[1].Value<string>();
                    var filename = args[2].Value<string>();
                    var transparent = args[3].Value<bool>();
                    var includeImage = args[4].Value<bool>();
                    var includeText = args[5].Value<bool>();
                    var cropToImageSize = args[6].Value<bool>();

                    view.save(format, folder, filename, transparent, includeImage, includeText, cropToImageSize);
                    break;
                case Commands.endPath:
                    view.end();
                    break;
                default:
                    break;
            }

            Debug.WriteLine(((Commands)commandId).ToString());
        }

        private enum Commands
        {
            addPoint = 1,
            newPath = 2,
            clear = 3,
            addPath = 4,
            deletePath = 5,
            save = 6,
            endPath = 7,
        }

        public override IReadOnlyDictionary<string, object> CommandsMap => new Dictionary<string, object> {
            {  Commands.addPoint.ToString(), (int)Commands.addPoint },
            {  Commands.newPath.ToString(), (int)Commands.newPath },
            {  Commands.clear.ToString(), (int)Commands.clear },
            {  Commands.addPath.ToString(), (int)Commands.addPath },
            {  Commands.deletePath.ToString(), (int)Commands.deletePath },
            {  Commands.save.ToString(), (int)Commands.save },
            {  Commands.endPath.ToString(), (int)Commands.endPath },
        };
    }
}
