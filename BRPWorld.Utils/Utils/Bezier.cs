using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BRPWorld.Utils.Utils
{
    /// <summary>
    /// Utility class about Bezier curves.
    /// Extensive Bezier explanation can be found at http://pomax.github.io/bezierinfo/
    /// </summary>
    public static class Bezier
    {
        public static Polynomial Line(double p0, double p1)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * p0 + T * p1;
        }
        public static PolyCurve Line(Point start, Point end)
        {
            return Line(start.ToArray(), end.ToArray());
        }
        public static PolyCurve Line(double[] p0, double[] p1)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * new PolyCurve(p0) + T * new PolyCurve(p1);
        }

        public static Polynomial Quadratic(double p0, double p1, double p2)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * Line(p0, p1) + T * Line(p1, p2);
        }
        public static PolyCurve Quadratic(Point p1, Point p2, Point p3)
        {
            return Quadratic(p1.ToArray(), p2.ToArray(), p3.ToArray());
        }
        public static PolyCurve Quadratic(double[] p0, double[] p1, double[] p2)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * Line(p0, p1) + T * Line(p1, p2);
        }

        public static Polynomial Cubic(double p0, double p1, double p2, double p3)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * Quadratic(p0, p1, p2) + T * Quadratic(p1, p2, p3);
        }
        public static PolyCurve Cubic(Point start, Point cp1, Point cp2, Point end)
        {
            return Cubic(
                start.ToArray(),
                cp1.ToArray(),
                cp2.ToArray(),
                end.ToArray()
            );
        }
        public static PolyCurve Cubic(double[] p0, double[] p1, double[] p2, double[] p3)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * Quadratic(p0, p1, p2) + T * Quadratic(p1, p2, p3);
        }

        public static Polynomial Curve(params double[] cpts)
        {
            if (cpts == null || cpts.Length == 0)
                throw new ArgumentNullException();
            var T = new Polynomial(0, 1);
            if (cpts.Length == 1)
            {
                return new Polynomial(cpts[0]);
            }
            else if (cpts.Length == 2)
            {
                return (1 - T) * new Polynomial(cpts[0]) + T * new Polynomial(cpts[1]);
            }
            else
            {
                var sub0 = new double[cpts.Length - 1];
                var sub1 = new double[cpts.Length - 1];
                for (int i = 0; i < cpts.Length - 1; i++)
                {
                    sub0[i] = cpts[i];
                    sub1[i] = cpts[i + 1];
                }
                return (1 - T) * Curve(sub0) + T * Curve(sub1);
            }
        }
        public static PolyCurve Curve(params Point[] points)
        {
            return Curve(points.Select(x => x.ToArray()).ToArray());
        }
        public static PolyCurve Curve(params double[][] cpts)
        {
            if (cpts == null || cpts.Length == 0)
                throw new ArgumentNullException();
            var T = new Polynomial(0, 1);
            if (cpts.Length == 1)
            {
                return new PolyCurve(cpts[0]);
            }
            else if (cpts.Length == 2)
            {
                return (1 - T) * new PolyCurve(cpts[0]) + T * new PolyCurve(cpts[1]);
            }
            else
            {
                var sub0 = new double[cpts.Length - 1][];
                var sub1 = new double[cpts.Length - 1][];
                for (int i = 0; i < cpts.Length - 1; i++)
                {
                    sub0[i] = cpts[i];
                    sub1[i] = cpts[i + 1];
                }
                return (1 - T) * Curve(sub0) + T * Curve(sub1);
            }
        }

        /// <summary>
        /// Returns a double[2][NPoint] of control points for the 2 sub curves, using "de Casteljau" algorithm
        /// </summary>
        public static double[][] Split(double t, params double[] cpts)
        {
            if (t < 0 || t > 1)
                throw new ArgumentOutOfRangeException();
            var lp = cpts.ToList();
            var result0 = new List<double>();
            var result1 = new List<double>();
            while (lp.Count > 0)
            {
                result0.Add(lp.First());
                result1.Add(lp.Last());
                var next = new List<double>(lp.Count - 1);
                for (int i = 0; i < lp.Count - 1; i++)
                {
                    var p0 = lp[i];
                    var p1 = lp[i + 1];
                    var p = p0 * (1 - t) + t * p1;
                    next.Add(p);
                }
                lp = next;
            }
            result1.Reverse(); // make 2nd curve same orientation
            return new[] { result0.ToArray(), result1.ToArray() };
        }
        /// <summary>
        /// Returns a double[2][NPoint] of control points for the 2 sub curves, using "de Casteljau" algorithm
        /// </summary>
        public static Point[][] Split(double t, params Point[] points)
        {
            var split = Split(t, points.Select(x => x.ToArray()).ToArray());
            return new Point[2][]
            {
                split[0].Select(x => PolygonUtils.PointFromArray(x)).ToArray(),
                split[1].Select(x => PolygonUtils.PointFromArray(x)).ToArray(),
            };
        }
        /// <summary>
        /// Returns a double[2][NPoint][dim] of control points for the 2 sub curves, using "de Casteljau" algorithm
        /// </summary>
        /// <param name="t">A number in [0,1] where the bezier curve was cut</param>
        /// <param name="cpts">Control point of the bezier curve.</param>
        /// <returns>2 set of control point for a same order bezier curve</returns>
        public static double[][][] Split(double t, params double[][] cpts)
        {
            if (t < 0 || t > 1)
                throw new ArgumentOutOfRangeException();

            var lp = cpts.ToList();
            var result0 = new List<double[]>();
            var result1 = new List<double[]>();
            while (lp.Count > 0)
            {
                result0.Add(lp.First());
                result1.Add(lp.Last());
                var next = new List<double[]>(lp.Count - 1);
                for (int i = 0; i < lp.Count - 1; i++)
                {
                    var p0 = lp[i];
                    var p1 = lp[i + 1];
                    var p = new double[p0.Length];
                    for (int j = 0; j < p.Length; j++)
                        p[j] = p0[j] * (1 - t) + t * p1[j];
                    next.Add(p);
                }
                lp = next;
            }
            result1.Reverse(); // make 2nd curve same orientation
            return new[] { result0.ToArray(), result1.ToArray() };
        }
    }
}
