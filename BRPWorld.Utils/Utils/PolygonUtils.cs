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

		public static PolyCurve CubicBezierCurve(Point start, Point cp1, Point cp2, Point end)
		{
			return PolyCurve.CubicBezierCurve(
				start.ToArray(),
				cp1.ToArray(),
				cp2.ToArray(),
				end.ToArray()
			);
		}
	}
}
