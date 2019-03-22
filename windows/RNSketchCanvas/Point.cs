using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNSketchCanvas
{
    public class Point
    {
        public Point()
        {
        }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(JArray args)
        {
            this.x = (int)args[0].Value<double>();
            this.y = (int)args[1].Value<double>();
        }

        public int x { get; set; }
        public int y { get; set; }

    }
}
