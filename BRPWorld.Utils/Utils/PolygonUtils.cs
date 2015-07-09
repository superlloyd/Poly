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
		public static double DistanceToPoint(this Point p, Point p2)
		{
			var dx = p.X - p2.X;
			var dy = p.Y - p2.Y;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		public static Point PointFromArray(double[] p) 
		{
			if (p.Length != 2)
				throw new ArgumentException();
			return new Point(p[0], p[1]); 
		}
		public static double[] ToArray(this Point p) { return new[] { p.X, p.Y }; }

		public static PolyCurve Segment(Point start, Point end)
		{
			return PolyCurve.Segment(start.ToArray(), end.ToArray());
		}

		public static PolyCurve CubicBezier(Point start, Point cp1, Point cp2, Point end)
		{
			return PolyCurve.CubicBezierCurve(
				start.ToArray(),
				cp1.ToArray(),
				cp2.ToArray(),
				end.ToArray()
			);
		}

		#region obsolete legacy for Github users

		[Obsolete]
		public static Point ClosestPointOnSegment(this Point p, Point start, Point end)
		{
			return ClosestPointOnCurve(p, SegmentCurve(start, end));
		}

		[Obsolete]
		public static Point ClosestPointOnBezier(this Point p, Point start, Point cp1, Point cp2, Point end)
		{
			return ClosestPointOnCurve(p, CubicBezierCurve(start, cp1, cp2, end));
		}

		[Obsolete]
		public static Point ClosestPointOnCurve(this Point p, Tuple<Polynomial, Polynomial> curve)
		{
			var pcurve = new PolyCurve(curve.Item1, curve.Item2);
			var pres = pcurve.ClosestPoint(p.ToArray(), 0, 1);
			return PointFromArray(pres);
		}

		[Obsolete]
		public static double DistanceToSegment(this Point p, Point start, Point end)
		{
			return DistanceToCurve(p, SegmentCurve(start, end));
		}

		[Obsolete]
		public static double DistanceToBezier(this Point p, Point start, Point cp1, Point cp2, Point end)
		{
			return DistanceToCurve(p, CubicBezierCurve(start, cp1, cp2, end));
		}

		[Obsolete]
		public static double DistanceToCurve(this Point p, Tuple<Polynomial, Polynomial> curve)
		{
			var pcurve = new PolyCurve(curve.Item1, curve.Item2);
			return pcurve.DistanceTo(p.ToArray(), 0, 1);
		}

		[Obsolete]
		public static Tuple<Polynomial, Polynomial> SegmentCurve(Point start, Point end)
		{
			var curve = Segment(start, end);
			return Tuple.Create(curve[0], curve[1]);
		}

		[Obsolete]
		public static Tuple<Polynomial, Polynomial> CubicBezierCurve(Point start, Point cp1, Point cp2, Point end)
		{
			var curve = CubicBezier(start, cp1, cp2, end);
			return Tuple.Create(curve[0], curve[1]);
		}

		#endregion
	}
}
