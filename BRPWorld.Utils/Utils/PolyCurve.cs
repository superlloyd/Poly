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

		#region ParameterizedSquareDistance() ClosestParameter() DistanceTo()

		Polynomial ParameterizedSquareDistance(double[] point)
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

		public double[] ClosestPoint(double[] point, double pmin, double pmax)
		{
			var p = ClosestParameter(point, pmin, pmax);
			return this.Compute(p);
		}

		public double DistanceTo(double[] point, double pmin, double pmax)
		{
			var dsquare = ParameterizedSquareDistance(point);
			var deriv = dsquare.Derivate().Normalize();
			var derivRoots = deriv.FindRoots();
			return derivRoots
				.Where(x => Math.Abs(x.Imaginary) < Polynomial.Epsilon && x.Real > 0 && x.Real < 1)
				.Select(x => (double)x.Real)
				.Where(x => x > pmin && x < pmax)
				.Concat(new double[] { pmin, pmax })
				.Select(x => Math.Sqrt(dsquare.Compute(x)))
				.OrderBy(x => x)
				.First();
		}

		#endregion

		#region interpolation: BezierCurves

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

		#endregion
	}
}
