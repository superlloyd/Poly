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
    /// Utility class about 2D Bezier curves
    /// Aka a segment, or quadratic or cubic bezier curve.
    /// Extensive Bezier explanation can be found at http://pomax.github.io/bezierinfo/
    /// </summary>
    public class BezierFragment
    {
        Point[] controlPoints;

        /// <summary>
        /// Create a linear, quadratic or cubic Bezier curve
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public BezierFragment(params Point[] ps)
        {
            if (ps == null)
                throw new ArgumentNullException();
            if (ps.Length < 2)
                throw new ArgumentException("Bezier curve need at least 2 points (segment).");
            controlPoints = ps;
        }

        #region Compute() CurveX CurveY

        public Point Compute(double t)
        {
            var x = CurveX.Compute(t);
            var y = CurveY.Compute(t);
            return new Point(x, y);
        }

        public Polynomial CurveX
        {
            get
            {
                if (mCurveX == null)
                {
                    mCurveX = Bezier(controlPoints.Select(p => p.X).ToArray());
                    mCurveX.IsReadonly = true;
                }
                return mCurveX;
            }
        }
        Polynomial mCurveX;

        public Polynomial CurveY
        {
            get
            {
                if (mCurveY == null)
                {
                    mCurveY = Bezier(controlPoints.Select(p => p.Y).ToArray());
                    mCurveY.IsReadonly = true;
                }
                return mCurveY;
            }
        }
        Polynomial mCurveY;

        #endregion

        #region ControlPoints

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

        #endregion

        #region basic static operations

        public static Polynomial Bezier(params double[] values)
        {
            if (values == null || values.Length < 1)
                throw new ArgumentNullException();
            return Bezier(0, values.Length - 1, values);
        }
        static Polynomial Bezier(int from, int to, double[] values)
        {
            if (from == to)
                return new Polynomial(values[from]);
            return OneMinusT * Bezier(from, to - 1, values) + T * Bezier(from + 1, to, values);
        }
        static readonly Polynomial T = new Polynomial(0, 1);
        static readonly Polynomial OneMinusT = 1 - T;

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
                GetMinMax(out x0, out x1, Bezier01.Concat(CurveX.Derivate().SolveRealRoots().Where(t => t >= 0 && t <= 1)).Select(t => CurveX.Compute(t)));
                GetMinMax(out y0, out y1, Bezier01.Concat(CurveY.Derivate().SolveRealRoots().Where(t => t >= 0 && t <= 1)).Select(t => CurveY.Compute(t)));
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
            if (t < 0 || t > 1)
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
            return new[] { new BezierFragment(r0.ToArray()), new BezierFragment(r1.ToArray()) };
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
            var vx = CurveX - p.X;
            var vy = CurveY - p.Y;
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
