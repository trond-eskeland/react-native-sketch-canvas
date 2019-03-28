using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNSketchCanvas.Model
{

    /*
     * 
     imageTextCurrent: {
      text: '',
      fontColor: '#FF3B3B',
      fontSize: 15,
      anchor: { x: 0, y: 0 },
      renderPosition: { x: 0, y: 0 },
      position: { x: 0, y: 0 },
      mode: 'none',
     },
     */
    class ImageText
    {
        public ImageText()
        {

        }
        public ImageText(JToken json)
        {
            this.text = json["text"].Value<string>();
            this.fontColor = json["fontColor"].Value<uint>();
            this.fontSize = json["fontSize"].Value<int>();

            var test = json["position"].ToObject<Model.Point>();

        }

        public string text { get; set; }
        public uint fontColor { get; set; }
        public int fontSize { get; set; }
        public Point anchor { get; set; }
        public Point position { get; set; }
    }
}
