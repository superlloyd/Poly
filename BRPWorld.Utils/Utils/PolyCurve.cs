using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRPWorld.Utils.Utils
{
	/// <summary>
	/// Multi dimensional polynomial. Representing a parametric curve.
	/// </summary>
	public class PolyCurve
	{
		Polynomial[] coordinates;

		public PolyCurve(params Polynomial[] polys)
		{
			if (polys == null || polys.Length == 0)
				throw new ArgumentNullException();
			coordinates = polys;
			for (int i = 0; i < coordinates.Length; i++)
				if (coordinates[i] == null)
					coordinates[i] = new Polynomial();
		}
		public PolyCurve(params double[] point)
		{
			if (point == null || point.Length == 0)
				throw new ArgumentNullException();
			coordinates = point.Select(x => new Polynomial(x)).ToArray();
		}

		public int Dimension { get { return coordinates.Length; } }
		public Polynomial this[int index]
		{
			get { return coordinates[index]; }
			set
			{
				if (value == null)
					value = new Polynomial();
				coordinates[index] = value;
			}
		}

		public double[] Compute(double x)
		{
			var res = new double[coordinates.Length];
			var xcoef = new double[coordinates.Length];
			for (int j = 0; j < coordinates.Length; j++)
			{
				xcoef[j] = 1;
				var poly = coordinates[j];
				for (int i = 0; i <= poly.Order; i++)
				{
					res[j] += poly[i] * xcoef[j];
					xcoef[j] *= x;
				}
			}
			return res;
		}

		#region math: * +

		public static PolyCurve operator *(PolyCurve p, double m) { return m * p; }
		public static PolyCurve operator *(double m, PolyCurve p)
		{
			var res = new Polynomial[p.coordinates.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = m * p.coordinates[i];
			return new PolyCurve(res);
		}
		public static PolyCurve operator /(PolyCurve p, double m)
		{
			var res = new Polynomial[p.coordinates.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = p.coordinates[i] / m;
			return new PolyCurve(res);
		}
		public static PolyCurve operator *(PolyCurve a, Polynomial b) { return b * a; }
		public static PolyCurve operator *(Polynomial a, PolyCurve b)
		{
			var res = new Polynomial[b.coordinates.Length];
			for (var i = 0; i < b.coordinates.Length; i++)
				res[i] = a * b.coordinates[i];
			return new PolyCurve(res);
		}
		public static PolyCurve operator +(PolyCurve a, PolyCurve b)
		{
			if (a.coordinates.Length != b.coordinates.Length)
				throw new ArgumentException("Argument PolyCurves have different Dimension.");
			var res = new Polynomial[a.coordinates.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = a.coordinates[i] + b.coordinates[i];
			return new PolyCurve(res);
		}
		public static PolyCurve operator -(PolyCurve a, PolyCurve b)
		{
			if (a.coordinates.Length != b.coordinates.Length)
				throw new ArgumentException("Argument PolyCurves have different Dimension.");
			var res = new Polynomial[a.coordinates.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = a.coordinates[i] - b.coordinates[i];
			return new PolyCurve(res);
		}
		public static PolyCurve operator -(PolyCurve a)
		{
			var res = new Polynomial[a.coordinates.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = -a.coordinates[i];
			return new PolyCurve(res);
		}
		public static PolyCurve operator +(PolyCurve a) { return a; }

		public static PolyCurve operator +(PolyCurve b, double[] a) { return a + b; }
		public static PolyCurve operator +(double[] a, PolyCurve b)
		{
			if (a.Length != b.coordinates.Length)
				throw new ArgumentException("Point and PolyCurve have different Dimension.");
			var res = new Polynomial[a.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] += a[i] + b.coordinates[i];
			return new PolyCurve(res);
		}
		public static PolyCurve operator -(double[] a, PolyCurve b) { return a + (-b); }
		public static PolyCurve operator -(PolyCurve a, double[] b)
		{
			if (a.coordinates.Length != b.Length)
				throw new ArgumentException("Point and PolyCurve have different Dimension.");
			var res = new Polynomial[a.coordinates.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = a.coordinates[i] - b[i];
			return new PolyCurve(res);
		}

		#endregion

        #region ParameterizedSquareDistance() ClosestParameter() ClosestPoint() DistanceTo()

        public Polynomial ParameterizedSquareDistance(double[] point)
        {
            if (point.Length != coordinates.Length)
                throw new ArgumentException("Point and PolyCurve have different Dimension.");
            var res = new Polynomial();
            for (int i = 0; i < coordinates.Length; i++)
            {
                var p = coordinates[i] - point[i];
                p = p * p;
                res += p;
            }
            return res;
        }

        public double ClosestParameter(double[] point)
        {
            var dsquare = ParameterizedSquareDistance(point);
            var deriv = dsquare.Derivate().Normalize();
            var derivRoots = deriv.FindRoots();
            return derivRoots
                .Where(x => Math.Abs(x.Imaginary) < Polynomial.Epsilon)
                .Select(x => (double)x.Real)
                .OrderBy(x => dsquare.Compute(x))
                .First();
        }

        public double ClosestParameter(double[] point, double pmin, double pmax)
        {
            var dsquare = ParameterizedSquareDistance(point);
            var deriv = dsquare.Derivate().Normalize();
            var derivRoots = deriv.FindRoots();
            return derivRoots
                .Where(x => Math.Abs(x.Imaginary) < Polynomial.Epsilon)
                .Select(x => (double)x.Real)
                .Where(x => x > pmin && x < pmax)
                .Concat(new double[] { pmin, pmax })
                .OrderBy(x => dsquare.Compute(x))
                .First();
        }

        public double[] ClosestPoint(double[] point)
        {
            var p = ClosestParameter(point);
            return this.Compute(p);
        }

        public double[] ClosestPoint(double[] point, double pmin, double pmax)
        {
            var p = ClosestParameter(point, pmin, pmax);
            return this.Compute(p);
        }

        public double DistanceTo(double[] point)
        {
            var dsquare = ParameterizedSquareDistance(point);
            var deriv = dsquare.Derivate().Normalize();
            var derivRoots = deriv.FindRoots();
            return derivRoots
                .Where(x => Math.Abs(x.Imaginary) < Polynomial.Epsilon)
                .Select(x => (double)x.Real)
                .Select(x => Math.Sqrt(dsquare.Compute(x)))
                .OrderBy(x => x)
                .First();
        }

        public double DistanceTo(double[] point, double pmin, double pmax)
        {
            var dsquare = ParameterizedSquareDistance(point);
            var deriv = dsquare.Derivate().Normalize();
            var derivRoots = deriv.FindRoots();
            return derivRoots
                .Where(x => Math.Abs(x.Imaginary) < Polynomial.Epsilon)
                .Select(x => (double)x.Real)
                .Where(x => x > pmin && x < pmax)
                .Concat(new double[] { pmin, pmax })
                .Select(x => Math.Sqrt(dsquare.Compute(x)))
                .OrderBy(x => x)
                .First();
        }

        #endregion

        #region interpolation: BezierCurves

        // good extensive tutorial on Bezier curves http://pomax.github.io/bezierinfo/

        public static PolyCurve Segment(double[] p0, double[] p1)
		{
            var T = new Polynomial(0, 1);
            return (1 - T) * new PolyCurve(p0) + T * new PolyCurve(p1);
        }
		public static PolyCurve QuadraticBezierCurve(double[] p0, double[] p1, double[] p2)
		{
			var T = new Polynomial(0, 1);
			return (1 - T) * Segment(p0, p1) + T * Segment(p1, p2);
		}
        public static PolyCurve CubicBezierCurve(double[] p0, double[] p1, double[] p2, double[] p3)
        {
            var T = new Polynomial(0, 1);
            return (1 - T) * QuadraticBezierCurve(p0, p1, p2) + T * QuadraticBezierCurve(p1, p2, p3);
        }
        public static PolyCurve Bezier(params double[][] cpts)
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
                return (1 - T) * Bezier(sub0) + T * Bezier(sub1);
            }
        }

        // one algorithm to split bezier in detail (not really foloow it, just followed geometric explanation)
        // https://en.wikipedia.org/wiki/De_Casteljau%27s_algorithm

        /// <summary>
        /// Returns a double[2][NPoint][dim] of control point for the 2 sub curves
        /// </summary>
        /// <param name="t">A number in [0,1] where the bezier curve was cut</param>
        /// <param name="cpts">Control point of the bezier curve.</param>
        /// <returns>2 set of control point for a same order bezier curve</returns>
        public static double[][][] SplitBezier(double t, params double[][] cpts)
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
            return new[] { result0.ToArray(), result1.ToArray() };
        }

		#endregion

        #region Interpolate()

        /// <summary>
        /// Construct a PolyCurve P such as points[i] = P.Compute(i)
        /// </summary>
        public static PolyCurve Interpolate(params double[][] points)
        {
            if (points == null || points.Length < 2)
                throw new ArgumentNullException("At least 2 different points must be given");

            var ys = new double[points.Length];
            var res = new Polynomial[points[0].Length];
            for (int i = 0; i < res.Length; i++)
            {
                for (int j = 0; j < points.Length; j++)
                    ys[j] = points[j][i];
                res[i] = Polynomial.Interpolate(ys);
            }

            return new PolyCurve(res);
        }

        #endregion
    }
}
