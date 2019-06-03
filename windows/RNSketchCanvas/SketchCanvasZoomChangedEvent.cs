
using Newtonsoft.Json.Linq;
using ReactNative.UIManager.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNSketchCanvas
{
    public class SketchCanvasZoomChangedEvent : Event
    {

        private float zoomFactor;
        private double horizontalOffset;
        private double verticalOffset;

        public SketchCanvasZoomChangedEvent(int viewTag, float zoomFactor, double horizontalOffset, double verticalOffset) : base(viewTag)
        {
            this.zoomFactor = zoomFactor;
            this.horizontalOffset = horizontalOffset;
            this.verticalOffset = verticalOffset;
        }

        public override string EventName => "topChange";

        public override void Dispatch(RCTEventEmitter eventEmitter)
        {

            var zoomOffset = new JObject
            {
                { "zoomFactor", zoomFactor },
                { "horizontalOffset", horizontalOffset },
                { "verticalOffset", verticalOffset },
            };

            var eventData = new JObject
            {
                { "zoomOffset", zoomOffset },
            };

            eventEmitter.receiveEvent(ViewTag, EventName, eventData);
        }
    }
}
