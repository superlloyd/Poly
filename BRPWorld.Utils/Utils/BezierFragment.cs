using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BRPWorld.Utils.Utils
{
    /// <summary>
    /// Utility class about 2D Bezier curves. Representing either a 2,3 or 4 control point bezier curve. 
    /// Aka a segment, or quadratic or cubic bezier curve.
    /// Extensive Bezier explanation can be found at http://pomax.github.io/bezierinfo/
    /// </summary>
    public class BezierFragment
    {
        Point[] controlPoints;
        Polynomial curveX, curveY;

        /// <summary>
        /// Create a segment
        /// </summary>
        public BezierFragment(Point start, Point end)
        {
            controlPoints = new Point[] { start, end };
            curveX = Line(start.X, end.X);
            curveY = Line(start.Y, end.Y);
        }
        /// <summary>
        /// Create a quadratic Bezier curve
        /// </summary>
        public BezierFragment(Point start, Point cp, Point end)
        {
            controlPoints = new Point[] { start, cp, end };
            curveX = Quadratic(start.X, cp.X, end.X);
            curveY = Quadratic(start.Y, cp.Y, end.Y);
        }
        /// <summary>
        /// Create a cubic Bezier curve
        /// </summary>
        public BezierFragment(Point start, Point cp1, Point cp2, Point end)
        {
            controlPoints = new Point[] { start, cp1, cp2, end };
            curveX = Cubic(start.X, cp1.X, cp2.X, end.X);
            curveY = Cubic(start.Y, cp1.Y, cp2.Y, end.Y);
        }
        /// <summary>
        /// Create a linear, quadratic or cubic Bezier curve
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public BezierFragment(params Point[] ps)
        {
            controlPoints = ps;
            curveX = Bezier(ps.Select(p => p.X).ToArray());
            curveY = Bezier(ps.Select(p => p.Y).ToArray());
        }

        public Point Compute(double t)
        {
            var x = curveX.Compute(t);
            var y = curveY.Compute(t);
            return new Point(x, y);
        }

        public class ReadonlyPoints : IReadOnlyList<Point>
        {
            Point[] values;
            internal ReadonlyPoints(Point[] values) { this.values = values; }

            public Point this[int index] { get { return values[index]; } }

            public int Count { get { return values.Length; } }

            public IEnumerator<Point> GetEnumerator() { return values.Cast<Point>().GetEnumerator(); }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        public ReadonlyPoints ControlPoints
        {
            get
            {
                if (roPoints == null)
                    roPoints = new ReadonlyPoints(controlPoints);
                return roPoints;
            }
        }
        ReadonlyPoints roPoints;

        #region basic static operations

        public static Polynomial Bezier(params double[] ps)
        {
            if (ps.Length == 2) return Line(ps[0], ps[1]);
            else if (ps.Length == 3) return Quadratic(ps[0], ps[1], ps[2]);
            else if (ps.Length == 4) return Cubic(ps[0], ps[1], ps[2], ps[3]);
            else throw new NotSupportedException();
        }

        public static Polynomial Line(double p0, double p1)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * p0 + T * p1;
        }
        public static Polynomial Quadratic(double p0, double p1, double p2)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * Line(p0, p1) + T * Line(p1, p2);
        }
        public static Polynomial Cubic(double p0, double p1, double p2, double p3)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * Quadratic(p0, p1, p2) + T * Quadratic(p1, p2, p3);
        }
        public static IEnumerable<Point> SnapRotate(Point p0, Point p1, params Point[] points) { return SnapRotate(p0, p1, (IEnumerable<Point>)points); }
        public static IEnumerable<Point> SnapRotate(Point p0, Point p1, IEnumerable<Point> points)
        {
            if (points == null)
                yield break;

            p1 = new Point(p1.X - p0.X, p1.Y - p0.Y);
            p0 = new Point();

            var r = Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y);
            if (r < Polynomial.Epsilon)
                foreach (var p in points)
                    yield return p;

            // get rotation angle
            var cos = p1.X / r;
            var sin = p1.Y / r;

            // now rotate by "- angle"
            foreach (var p in points)
                yield return new Point(cos * p.X + sin * p.Y, -sin * p.X + cos * p.Y);
        }

        #endregion

        internal static readonly double[] Bezier01 = new double[] { 0, 1 };

        #region BoundingBox()

        public Rect BoundingBox()
        {
            double x0, x1, y0, y1;
            if (controlPoints.Length == 2)
            {
                GetMinMax(out x0, out x1, controlPoints[0].X, controlPoints[1].X);
                GetMinMax(out y0, out y1, controlPoints[0].Y, controlPoints[1].Y);
            }
            else
            {
                GetMinMax(out x0, out x1, Bezier01.Concat(curveX.Derivate().SolveRealRoots().Where(t => t >= 0 && t <= 1)).Select(t => curveX.Compute(t)));
                GetMinMax(out y0, out y1, Bezier01.Concat(curveY.Derivate().SolveRealRoots().Where(t => t >= 0 && t <= 1)).Select(t => curveY.Compute(t)));
            }
            return new Rect(x0, y0, x1 - x0, y1 - y0);
        }
        internal static bool GetMinMax(out double min, out double max, params double[] numbers) { return GetMinMax(out min, out max, (IEnumerable<double>)numbers); }
        internal static bool GetMinMax(out double min, out double max, IEnumerable<double> numbers)
        {
            min = max = 0;
            bool first = true;
            foreach (var x in numbers)
            {
                if (first)
                {
                    first = false;
                    min = max = x;
                }
                else
                {
                    if (x < min)
                        min = x;
                    else if (x > max)
                        max = x;
                }
            }
            return !first;
        }

        #endregion

        #region Split()

        /// <summary>
        /// Cut a <see cref="BezierFragment"/> in multiple fragment at the given t indices, using "De Casteljau" algorithm.
        /// <param name="t">The value at which to split the curve. Should be strictly inside ]0,1[ interval.</param>
        /// </summary>
        public BezierFragment[] Split(double t)
        {
            if (t <= 0 || t >= 1)
                throw new ArgumentOutOfRangeException();
            // http://pomax.github.io/bezierinfo/#decasteljau
            var r0 = new List<Point>();
            var r1 = new List<Point>();
            var lp = controlPoints.ToList();
            while (lp.Count > 0)
            {
                r0.Add(lp.First());
                r1.Add(lp.Last());
                var next = new List<Point>(lp.Count - 1);
                for (int i = 0; i < lp.Count - 1; i++)
                {
                    var p0 = lp[i];
                    var p1 = lp[i + 1];
                    var x = p0.X * (1 - t) + t * p1.X;
                    var y = p0.Y * (1 - t) + t * p1.Y;
                    next.Add(new Point(x, y));
                }
                lp = next;
            }
            return new[] { new BezierFragment(r0.ToArray()), new BezierFragment(r0.ToArray()) };
        }
        public BezierFragment[] Split(params double[] ts) { return Split((IEnumerable<double>)ts); }
        public BezierFragment[] Split(IEnumerable<double> ts)
        {
            if (ts == null)
                return new[] { this };
            var filtered = ts.Where(t => t > 0 && t < 1).Distinct().OrderBy(t => t).ToList();
            if (filtered.Count == 0)
                return new[] { this };

            var tLast = 0.0;
            var start = this;
            var list = new List<BezierFragment>(filtered.Count + 1);
            foreach (var t in filtered)
            {
                var relT = 1 - (1 - t) / (1 - tLast);
                tLast = t;
                var cut = start.Split(relT);
                list.Add(cut[0]);
                start = cut[1];
            }
            list.Add(start);
            return list.ToArray();
        }

        #endregion

        #region ParameterizedSquareDistance() ClosestParameter() DistanceTo()

        public Polynomial ParameterizedSquareDistance(Point p)
        {
            var vx = curveX - p.X;
            var vy = curveY - p.Y;
            return vx * vx + vy * vy;
        }

        public double ClosestParameter(Point point)
        {
            var dsquare = ParameterizedSquareDistance(point);
            var deriv = dsquare.Derivate().Normalize();
            var derivRoots = deriv.SolveOrFindRealRoot();
            return derivRoots
                .Where(t => t > 0 && t < 1)
                .Concat(Bezier01)
                .OrderBy(x => dsquare.Compute(x))
                .First();
        }

        public double DistanceTo(Point point)
        {
            var dsquare = ParameterizedSquareDistance(point);
            var deriv = dsquare.Derivate().Normalize();
            var derivRoots = deriv.SolveOrFindRealRoot();
            return derivRoots
                .Where(t => t > 0 && t < 1)
                .Concat(Bezier01)
                .Select(x => Math.Sqrt(dsquare.Compute(x)))
                .OrderBy(x => x)
                .First();
        }

        #endregion
    }
}
