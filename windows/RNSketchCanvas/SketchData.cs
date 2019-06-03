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
        private UInt32 strokeColor;
        public int strokeWidth;
        public bool isTranslucent;


        public WriteableBitmap mPaint { get; set; }

        //private Path mPath;
        //private React mDirty = null;

        public static Point midPoint(Point p1, Point p2)
        {
            return new Point((int)((p1.x + p2.x) * 0.5), (int)((p1.y + p2.y) * 0.5));
        }

        public SketchData(int id, UInt32 strokeColor, int strokeWidth)
        {
            this.id = id;
            this.strokeColor = strokeColor;
            this.strokeWidth = strokeWidth;
            //this.isTranslucent = ((strokeColor >> 24) & 0xff) != 255 && strokeColor != Color.TRANSPARENT;
            //mPath = this.isTranslucent ? new Path() : null;
        }

        public SketchData(int id, uint strokeColor, int strokeWidth, List<Point> points)
        {
            this.id = id;
            this.strokeColor = strokeColor;
            this.strokeWidth = strokeWidth;
            this.points = points;
            //this.isTranslucent = ((strokeColor >> 24) & 0xff) != 255 && strokeColor != Color.TRANSPARENT;
            //mPath = this.isTranslucent ? evaluatePath() : null;
        }

        public void addPoint(Point p)
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

        public void drawLastPoint(WriteableBitmap canvas)
        {
            int pointsCount = points.Count;
            if (pointsCount < 1)
            {
                return;
            }

            draw(canvas, pointsCount - 1);
        }

        public void draw(WriteableBitmap canvas)
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
                    draw(canvas, i);
                }
            }


        }

        private WriteableBitmap getPaint()
        {
            //if (mPaint == null)
            //{
            //    boolean isErase = strokeColor == Color.TRANSPARENT;

            //    mPaint = new Paint();
            //    mPaint.setColor(strokeColor);
            //    mPaint.setStrokeWidth(strokeWidth);
            //    mPaint.setStyle(Paint.Style.STROKE);
            //    mPaint.setStrokeCap(Paint.Cap.ROUND);
            //    mPaint.setStrokeJoin(Paint.Join.ROUND);
            //    mPaint.setAntiAlias(true);
            //    mPaint.setXfermode(new PorterDuffXfermode(isErase ? PorterDuff.Mode.CLEAR : PorterDuff.Mode.SRC_OVER));
            //}

            if (mPaint == null)
            {
                this.mPaint = BitmapFactory.New(strokeWidth, strokeWidth);
                this.mPaint.FillEllipse(0, 0, strokeWidth, strokeWidth, (int)strokeColor);
            }
            return mPaint;
        }

        private void draw(WriteableBitmap canvas, int pointIndex)
        {

            int pointsCount = points.Count;
            if (pointIndex >= pointsCount)
            {
                return;
            }

            Debug.WriteLine("points:" + pointsCount);

            if (pointsCount >= 3 && pointIndex >= 2)
            {
                //Point c = points[pointIndex - 2];
                Point a = points[pointIndex - 1];
                Point b = points[pointIndex];
                Debug.WriteLine($"draw line 3 pt point: {pointIndex}, {pointIndex - 1}");

                //canvas.DrawLineBresenham(a.x, a.y, b.x, b.y, (int)strokeColor);
                canvas.DrawLinePenned(a.x, a.y, b.x, b.y, this.getPaint());

                //
                // Summary:
                //     Draws a Cardinal spline (cubic) defined by a point collection. The cardinal spline
                //     passes through each point in the collection.
                //
                // Parameters:
                //   bmp:
                //     The WriteableBitmap.
                //
                //   points:
                //     The points for the curve in x and y pairs, therefore the array is interpreted
                //     as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).
                //
                //   tension:
                //     The tension of the curve defines the shape. Usually between 0 and 1. 0 would
                //     be a straight line.
                //
                //   color:
                //     The color for the spline.


                //canvas.DrawCurve(new int[] { a.x, a.y, b.x, b.y, c.x, c.y }, 1, (int)strokeColor);

            }
            else if (pointsCount >= 2 && pointIndex >= 1)
            {
                Point a = points[pointIndex - 1];
                Point b = points[pointIndex];
                Debug.WriteLine("draw line");
                canvas.DrawLinePenned(a.x, a.y, b.x, b.y, this.getPaint());
            }
            else if (pointsCount >= 1)
            {
                Point a = points[pointIndex];
                Debug.WriteLine("draw dot");
                canvas.FillEllipse(a.x, a.y, 5, 5, Color.FromArgb(255, 0, 255, 255));
            }

            //if (pointsCount >= 3 && pointIndex >= 2)
            //{
            //    Point a = points[pointIndex - 2];
            //    Point b = points[pointIndex - 1];
            //    Point c = points[pointIndex];
            //    Point prevMid = midPoint(a, b);
            //    Point currentMid = midPoint(b, c);

            //    // Draw a curve
            //    //Path path = new Path();
            //    //path.moveTo(prevMid.x, prevMid.y);
            //    //path.quadTo(b.x, b.y, currentMid.x, currentMid.y);

            //    //canvas.drawPath(path, getPaint());

            //}
            //else if (pointsCount >= 2 && pointIndex >= 1)
            //{
            //    Point a = points[pointIndex - 1];
            //    Point b = points[pointIndex];
            //    Point mid = midPoint(a, b);

            //    // Draw a line to the middle of points a and b
            //    // This is so the next draw which uses a curve looks correct and continues from there
            //    //canvas.drawLine(a.x, a.y, mid.x, mid.y, getPaint());
            //    canvas.DrawLinePenned(a.x, a.y, mid.x, mid.y, this.getPaint());
            //}
            //else if (pointsCount >= 1)
            //{
            //    Point a = points[pointIndex];

            //    // Draw a single point
            //    //canvas.drawPoint(a.x, a.y, getPaint());
            //    canvas.FillEllipse(a.x, a.y, 5, 5, Color.FromArgb(255, 0, 255, 255));
            //}
        }

        //private Path evaluatePath()
        //{
        //    int pointsCount = points.size();
        //    Path path = new Path();

        //    for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
        //    {
        //        if (pointsCount >= 3 && pointIndex >= 2)
        //        {
        //            PointF a = points.get(pointIndex - 2);
        //            PointF b = points.get(pointIndex - 1);
        //            PointF c = points.get(pointIndex);
        //            PointF prevMid = midPoint(a, b);
        //            PointF currentMid = midPoint(b, c);

        //            // Draw a curve
        //            path.moveTo(prevMid.x, prevMid.y);
        //            path.quadTo(b.x, b.y, currentMid.x, currentMid.y);
        //        }
        //        else if (pointsCount >= 2 && pointIndex >= 1)
        //        {
        //            PointF a = points.get(pointIndex - 1);
        //            PointF b = points.get(pointIndex);
        //            PointF mid = midPoint(a, b);

        //            // Draw a line to the middle of points a and b
        //            // This is so the next draw which uses a curve looks correct and continues from there
        //            path.moveTo(a.x, a.y);
        //            path.lineTo(mid.x, mid.y);
        //        }
        //        else if (pointsCount >= 1)
        //        {
        //            PointF a = points.get(pointIndex);

        //            // Draw a single point
        //            path.moveTo(a.x, a.y);
        //            path.lineTo(a.x, a.y);
        //        }
        //    }
        //    return path;
        //}

        //private void addPointToPath(Path path, PointF tPoint, PointF pPoint, PointF point)
        //{
        //    PointF mid1 = new PointF((pPoint.x + tPoint.x) * 0.5f, (pPoint.y + tPoint.y) * 0.5f);
        //    PointF mid2 = new PointF((point.x + pPoint.x) * 0.5f, (point.y + pPoint.y) * 0.5f);
        //    path.moveTo(mid1.x, mid1.y);
        //    path.quadTo(pPoint.x, pPoint.y, mid2.x, mid2.y);
        //}


    }
}
