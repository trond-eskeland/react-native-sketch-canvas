using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using RNSketchCanvas.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace RNSketchCanvas
{
    public class SketchData
    {

        public List<Point> points = new List<Point>();
        public int id;
        private Windows.UI.Color strokeColor;
        public int strokeWidth;
        public bool isTranslucent;


        public Color mPaint { get; set; }

        //private Path mPath;
        //private React mDirty = null;

        public static Point MidPoint(Point p1, Point p2)
        {
            return new Point((int)((p1.x + p2.x) * 0.5), (int)((p1.y + p2.y) * 0.5));
        }

        public SketchData(int id, Windows.UI.Color strokeColor, int strokeWidth)
        {
            this.id = id;
            this.strokeColor = strokeColor;
            this.strokeWidth = strokeWidth;
            //this.isTranslucent = ((strokeColor >> 24) & 0xff) != 255 && strokeColor != Color.TRANSPARENT;
            //mPath = this.isTranslucent ? new Path() : null;
        }

        public SketchData(int id, Windows.UI.Color strokeColor, int strokeWidth, List<Point> points)
        {
            this.id = id;
            this.strokeColor = strokeColor;
            this.strokeWidth = strokeWidth;
            this.points = points;
            //this.isTranslucent = ((strokeColor >> 24) & 0xff) != 255 && strokeColor != Color.TRANSPARENT;
            //mPath = this.isTranslucent ? evaluatePath() : null;
        }

        public void AddPoint(Point p)
        {
            points.Add(p);
        }

        //public void addPoint(Point p)
        //{
        //    points.Add(p);

        //    //RectF updateRect;

        //    int pointsCount = points.Count();

        //    if (this.isTranslucent)
        //    {
        //        if (pointsCount >= 3)
        //        {
        //            addPointToPath(mPath,
        //                this.points.get(pointsCount - 3),
        //                this.points.get(pointsCount - 2),
        //                p);
        //        }
        //        else if (pointsCount >= 2)
        //        {
        //            addPointToPath(mPath, this.points.get(0), this.points.get(0), p);
        //        }
        //        else
        //        {
        //            addPointToPath(mPath, p, p, p);
        //        }

        //        float x = p.x, y = p.y;
        //        if (mDirty == null)
        //        {
        //            mDirty = new RectF(x, y, x + 1, y + 1);
        //            updateRect = new RectF(x - this.strokeWidth, y - this.strokeWidth,
        //                x + this.strokeWidth, y + this.strokeWidth);
        //        }
        //        else
        //        {
        //            mDirty.union(x, y);
        //            updateRect = new RectF(
        //                                mDirty.left - this.strokeWidth, mDirty.top - this.strokeWidth,
        //                                mDirty.right + this.strokeWidth, mDirty.bottom + this.strokeWidth
        //                                );
        //        }
        //    }
        //    else
        //    {
        //        if (pointsCount >= 3)
        //        {
        //            PointF a = points.get(pointsCount - 3);
        //            PointF b = points.get(pointsCount - 2);
        //            PointF c = p;
        //            PointF prevMid = midPoint(a, b);
        //            PointF currentMid = midPoint(b, c);

        //            updateRect = new RectF(prevMid.x, prevMid.y, prevMid.x, prevMid.y);
        //            updateRect.union(b.x, b.y);
        //            updateRect.union(currentMid.x, currentMid.y);
        //        }
        //        else if (pointsCount >= 2)
        //        {
        //            PointF a = points.get(pointsCount - 2);
        //            PointF b = p;
        //            PointF mid = midPoint(a, b);

        //            updateRect = new RectF(a.x, a.y, a.x, a.y);
        //            updateRect.union(mid.x, mid.y);
        //        }
        //        else
        //        {
        //            updateRect = new RectF(p.x, p.y, p.x, p.y);
        //        }

        //        updateRect.inset(-strokeWidth * 2, -strokeWidth * 2);

        //    }
        //    Rect integralRect = new Rect();
        //    updateRect.roundOut(integralRect);

        //    return integralRect;
        //}


        public void DrawLastPoint(CanvasDrawingSession canvas)
        {
            int pointsCount = points.Count;
            if (pointsCount < 1)
            {
                return;
            }

            Draw(canvas, pointsCount - 1);
        }


        public void Draw(CanvasDrawingSession canvas)
        {
            if (this.isTranslucent)
            {
                //FIXME
                //canvas.drawPath(mPath, getPaint());
                //canvas.FillEllipse(a.x, a.y, 5, 5, Color.FromArgb(255, 0, 255, 255));
            }
            else
            {
                int pointsCount = points.Count;
                for (int i = 0; i < pointsCount; i++)
                {
                    Draw(canvas, i);
                }
            }


        }

        private void Draw(CanvasDrawingSession canvas, int pointIndex)
        {

            int pointsCount = points.Count;
            if (pointIndex >= pointsCount)
            {
                return;
            }

            if (pointsCount >= 2 && pointIndex >= 1)
            {
                Point a = points[pointIndex - 1];
                Point b = points[pointIndex];
     
                CanvasSolidColorBrush brush = new CanvasSolidColorBrush(canvas.Device, strokeColor);
                CanvasStrokeStyle strokeStyle = new CanvasStrokeStyle();

                canvas.DrawLine(new System.Numerics.Vector2(a.x, a.y), new System.Numerics.Vector2(b.x, b.y), brush, (float)5);
            }
            else if (pointsCount >= 1)
            {
                Point a = points[pointIndex];

            }
        }
    }
}
