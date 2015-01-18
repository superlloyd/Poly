using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace BRPWorld.Utils.Utils
{
	public static class PolygonUtils
	{
		#region distances

		public static double DistanceToPoint(this Point p, Point p2)
		{
			return Math.Sqrt((p.X - p2.X) * (p.Y - p2.Y));
		}

		public static Point ClosestPointOnSegment(this Point p, Point start, Point end)
		{
			return ClosestPointOnCurve(p, SegmentCurve(start, end));
		}

		public static Point ClosestPointOnBezier(this Point p, Point start, Point cp1, Point cp2, Point end)
		{
			return ClosestPointOnCurve(p, CubicBezierCurve(start, cp1, cp2, end));
		}

		public static Point ClosestPointOnCurve(this Point p, Tuple<Polynomial, Polynomial> curve)
		{
			var dsquare = (curve.Item1 - p.X) * (curve.Item1 - p.X) + (curve.Item2 - p.Y) * (curve.Item2 - p.Y);
			var deriv = dsquare.Derivate().Normalize();
			var deriveRoots = deriv.FindRoots();
			return deriveRoots
				.Where(x => Math.Abs(x.Imaginary) < Polynomial.Epsilon && x.Real > 0 && x.Real < 1)
				.Select(x => (double)x.Real)
				.Concat(new double[] { 0, 1 })
				.OrderBy(x => dsquare.Compute(x))
				.Select(x => new Point(curve.Item1.Compute(x), curve.Item2.Compute(x)))
				.First();
		}

		public static double DistanceToSegment(this Point p, Point start, Point end)
		{
			return DistanceToCurve(p, SegmentCurve(start, end));
		}

		public static double DistanceToBezier(this Point p, Point start, Point cp1, Point cp2, Point end)
		{
			return DistanceToCurve(p, CubicBezierCurve(start, cp1, cp2, end));
		}

		public static double DistanceToCurve(this Point p, Tuple<Polynomial, Polynomial> curve)
		{
			var dsquare = (curve.Item1 - p.X) * (curve.Item1 - p.X) + (curve.Item2 - p.Y) * (curve.Item2 - p.Y);
			var deriv = dsquare.Derivate().Normalize();
			var deriveRoots = deriv.FindRoots();
			return deriveRoots
				.Where(x => Math.Abs(x.Imaginary) < Polynomial.Epsilon && x.Real > 0 && x.Real < 1)
				.Select(x => (double)x.Real)
				.Concat(new double[] { 0, 1 })
				.Select(x => Math.Sqrt(dsquare.Compute(x)))
				.OrderBy(x => x)
				.First();
		}

		public static Tuple<Polynomial, Polynomial> SegmentCurve(Point start, Point end)
		{
			return Tuple.Create(
				Polynomial.LinearBezierCurve(start.X, end.X),
				Polynomial.LinearBezierCurve(start.Y, end.Y)
			);
		}
		public static Tuple<Polynomial, Polynomial> CubicBezierCurve(Point start, Point cp1, Point cp2, Point end)
		{
			return Tuple.Create(
				Polynomial.CubicBezierCurve(start.X, cp1.X, cp2.X, end.X),
				Polynomial.CubicBezierCurve(start.Y, cp1.Y, cp2.Y, end.Y)
			);
		}

		#endregion
	}
}
