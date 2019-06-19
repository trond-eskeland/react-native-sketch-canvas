using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactNative.UIManager;
using ReactNative.UIManager.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace RNSketchCanvas
{

    public static class RNHelper
    {
        public static ReactPointer CreateReactPointer(UIElement reactView, PointerPoint rootPoint, PointerRoutedEventArgs e, bool detectSubcomponent)
        {
            var viewPoint = e.GetCurrentPoint(reactView);
            var reactTag = detectSubcomponent ?
                reactView.GetReactCompoundView().GetReactTagAtPoint(reactView, viewPoint.Position) :
                reactView.GetTag();
            var pointer = new ReactPointer
            {
                Target = reactTag,
                PointerId = e.Pointer.PointerId,
                Identifier =  1,//++_pointerIDs,
                PointerType = e.Pointer.PointerDeviceType.GetPointerDeviceTypeName(),
                IsLeftButton = viewPoint.Properties.IsLeftButtonPressed,
                IsRightButton = viewPoint.Properties.IsRightButtonPressed,
                IsMiddleButton = viewPoint.Properties.IsMiddleButtonPressed,
                IsHorizontalMouseWheel = viewPoint.Properties.IsHorizontalMouseWheel,
                IsEraser = viewPoint.Properties.IsEraser,
                ReactView = reactView,
            };

            return pointer;
        }
    }

    public class ReactPointer
    {
        [JsonProperty(PropertyName = "target")]
        public int Target { get; set; }

        [JsonIgnore]
        public uint PointerId { get; set; }

        [JsonProperty(PropertyName = "identifier")]
        public uint Identifier { get; set; }

        [JsonIgnore]
        public UIElement ReactView { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public ulong Timestamp { get; set; }

        [JsonProperty(PropertyName = "locationX")]
        public float LocationX { get; set; }

        [JsonProperty(PropertyName = "locationY")]
        public float LocationY { get; set; }

        [JsonProperty(PropertyName = "pageX")]
        public float PageX { get; set; }

        [JsonProperty(PropertyName = "pageY")]
        public float PageY { get; set; }

        [JsonProperty(PropertyName = "pointerType")]
        public string PointerType { get; set; }

        [JsonProperty(PropertyName = "force")]
        public double Force { get; set; }

        [JsonProperty(PropertyName = "isLeftButton", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsLeftButton { get; set; }

        [JsonProperty(PropertyName = "isRightButton", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsRightButton { get; set; }

        [JsonProperty(PropertyName = "isMiddleButton", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsMiddleButton { get; set; }

        [JsonProperty(PropertyName = "isBarrelButtonPressed", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsBarrelButtonPressed { get; set; }

        [JsonProperty(PropertyName = "isHorizontalScrollWheel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsHorizontalMouseWheel { get; set; }

        [JsonProperty(PropertyName = "isEraser", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsEraser { get; set; }

        [JsonProperty(PropertyName = "shiftKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ShiftKey { get; set; }

        [JsonProperty(PropertyName = "ctrlKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool CtrlKey { get; set; }

        [JsonProperty(PropertyName = "altKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool AltKey { get; set; }
    }
    public class TouchEvent : Event
    {
        private readonly TouchEventType _touchEventType;
        private readonly JArray _touches;
        private readonly JArray _changedIndices;
        private readonly uint _coalescingKey;

        public TouchEvent(TouchEventType touchEventType, JArray touches, JArray changedIndices, uint coalescingKey)
            : base(-1)
        {
            _touchEventType = touchEventType;
            _touches = touches;
            _changedIndices = changedIndices;
            _coalescingKey = coalescingKey;
        }

        public override string EventName
        {
            get
            {
                return _touchEventType.GetJavaScriptEventName();
            }
        }

        public override bool CanCoalesce
        {
            get
            {
                return _touchEventType == TouchEventType.Move || _touchEventType == TouchEventType.PointerMove;
            }
        }

        public override short CoalescingKey
        {
            get
            {
                unchecked
                {
                    return (short)_coalescingKey;
                }
            }
        }

        public override void Dispatch(RCTEventEmitter eventEmitter)
        {
            eventEmitter.receiveTouches(EventName, _touches, _changedIndices);
        }
    }

    static class TouchEventTypeExtensions
    {
        public static string GetJavaScriptEventName(this TouchEventType eventType)
        {
            switch (eventType)
            {
                case TouchEventType.Start:
                    return "topTouchStart";
                case TouchEventType.End:
                    return "topTouchEnd";
                case TouchEventType.Move:
                    return "topTouchMove";
                case TouchEventType.Cancel:
                    return "topTouchCancel";
                case TouchEventType.Entered:
                    return "topMouseOver";
                case TouchEventType.Exited:
                    return "topMouseOut";
                case TouchEventType.PointerMove:
                    return "topMouseMoveCustom"; // Using a non-clashing name until this one propagates: https://github.com/facebook/react/commit/e96dc140599363029bd05565d58bcd4a432db370
                default:
                    throw new NotSupportedException("Unsupported touch event type.");
            }
        }
    }

    static class PointerDeviceTypeExtensions
    {
        public static string GetPointerDeviceTypeName(this PointerDeviceType pointerDeviceType)
        {
            switch (pointerDeviceType)
            {
                case PointerDeviceType.Touch:
                    return "touch";
                case PointerDeviceType.Pen:
                    return "pen";
                case PointerDeviceType.Mouse:
                    return "mouse";
                default:
                    return "unknown";
            }
        }
    }
}
