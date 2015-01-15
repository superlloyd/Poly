using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRPWorld.Utils.Utils
{
#pragma warning disable 659
	public class Polynomial
	{
		double[] coefficients;

		public Polynomial(params double[] coefficients)
		{
			this.coefficients = coefficients;
			if (coefficients == null || coefficients.Length == 0)
				this.coefficients = new double[] { 0 };
		}

		public static Polynomial Term(int power, double coefficient = 1)
		{
			if (power < 0)
				throw new ArgumentOutOfRangeException();
			var res = new double[power + 1];
			res[power] = coefficient;
			return new Polynomial(res);
		}
		public static Polynomial X() { return new Polynomial(0, 1); }

		#region Order, this[]

		public int Order { get { return this.coefficients.Length - 1; } }

		public double this[int index]
		{
			get
			{
				if (index < 0 || index >= this.coefficients.Length)
					throw new ArgumentOutOfRangeException();
				return coefficients[index];
			}
			set
			{
				if (index < 0 || index > coefficients.Length)
					throw new ArgumentOutOfRangeException();
				coefficients[index] = value;
			}
		}

		#endregion

		#region Normalize(), Trim(), Epsilon

		public Polynomial Normalize()
		{
			int order = 0;
			double high = 1;
			for (int i = 0; i < coefficients.Length; i++)
			{
				if (Math.Abs(coefficients[i]) > Epsilon)
				{
					order = i;
					high = coefficients[i];
				}
			}
			var res = new double[order + 1];
			for (int i = 0; i < res.Length; i++)
			{
				if (Math.Abs(coefficients[i]) > Epsilon)
				{
					res[i] = coefficients[i] / high;
				}
			}
			return new Polynomial(res);
		}

		public Polynomial Trim() { return Trim(Epsilon); }
		public Polynomial Trim(double epsilon)
		{
			int order = 0;
			for (int i = 0; i < coefficients.Length; i++)
			{
				if (Math.Abs(coefficients[i]) > Epsilon)
				{
					order = i;
				}
			}
			var res = new double[order + 1];
			for (int i = 0; i < res.Length; i++)
			{
				if (Math.Abs(coefficients[i]) > Epsilon)
				{
					res[i] = coefficients[i];
				}
			}
			return new Polynomial(res);
		}

		public static double Epsilon
		{
			get { return sEpsilon; }
			set
			{
				if (value < double.Epsilon)
					throw new ArgumentOutOfRangeException();
				sEpsilon = value;
			}
		}
		[ThreadStatic]
		static double sEpsilon = 0.001;

		#endregion

		#region math: * + ^

		/// <summary>
		/// Raise a polynomial to power. Warning operator priority is wrong.
		/// </summary>
		public static Polynomial operator ^(Polynomial p, int pow) { return p.Pow(pow); }

		public static Polynomial operator *(Polynomial p, double m) { return m * p; }
		public static Polynomial operator *(double m, Polynomial p)
		{
			var res = new double[p.coefficients.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = m * p.coefficients[i];
			return new Polynomial(res);
		}
		public static Polynomial operator /(Polynomial p, double m)
		{
			var res = new double[p.coefficients.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = p.coefficients[i] / m;
			return new Polynomial(res);
		}
		public static Polynomial operator *(Polynomial a, Polynomial b)
		{
			var res = new double[a.coefficients.Length + b.coefficients.Length - 1];
			for (var i = 0; i < a.coefficients.Length; i++)
				for (var j = 0; j < b.coefficients.Length; j++)
				{
					var mul = a.coefficients[i] * b.coefficients[j];
					res[i + j] += mul;
				}
			return new Polynomial(res);
		}
		public static Polynomial operator +(Polynomial a, Polynomial b)
		{
			var res = new double[Math.Max(a.coefficients.Length, b.coefficients.Length)];
			for (int i = 0; i < res.Length; i++)
			{
				double p = 0;
				if (i < a.coefficients.Length) p += a.coefficients[i];
				if (i < b.coefficients.Length) p += b.coefficients[i];
				res[i] = p;
			}
			return new Polynomial(res);
		}
		public static Polynomial operator -(Polynomial a, Polynomial b)
		{
			var res = new double[Math.Max(a.coefficients.Length, b.coefficients.Length)];
			for (int i = 0; i < res.Length; i++)
			{
				double p = 0;
				if (i < a.coefficients.Length) p += a.coefficients[i];
				if (i < b.coefficients.Length) p -= b.coefficients[i];
				res[i] = p;
			}
			return new Polynomial(res);
		}
		public static Polynomial operator -(Polynomial a)
		{
			var res = new double[a.coefficients.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = -a.coefficients[i];
			return new Polynomial(res);
		}
		public static Polynomial operator +(Polynomial a) { return a; }

		public static Polynomial operator +(Polynomial b, double a) { return a + b; }
		public static Polynomial operator +(double a, Polynomial b)
		{
			var res = new double[b.coefficients.Length];
			for (int i = 0; i < res.Length; i++) res[i] = b.coefficients[i];
			res[0] += a;
			return new Polynomial(res);
		}
		public static Polynomial operator -(Polynomial a, double b) { return a + (-b); }
		public static Polynomial operator -(double b, Polynomial a)
		{
			var res = new double[a.coefficients.Length];
			for (int i = 0; i < res.Length; i++) res[i] = -a.coefficients[i];
			res[0] += b;
			return new Polynomial(res);
		}

		#endregion

		#region operations: Compute() Pow() Derivate() Integrate()

		public double Compute(double x)
		{
			double res = 0;
			double xcoef = 1;
			for (int i = 0; i < coefficients.Length; i++)
			{
				res += coefficients[i] * xcoef;
				xcoef *= x;
			}
			return res;
		}

		public Complex Compute(Complex x)
		{
			Complex res = 0;
			Complex xcoef = 1;
			for (int i = 0; i < coefficients.Length; i++)
			{
				res += coefficients[i] * xcoef;
				xcoef *= x;
			}
			return res;
		}

		public Polynomial Pow(int n)
		{
			if (n < 0)
				throw new ArgumentOutOfRangeException();
			var order = coefficients.Length - 1;
			var res = new double[order * n + 1];
			var tmp = new double[order * n + 1];
			res[0] = 1;
			for (int pow = 0; pow < n; pow++)
			{
				int porder = pow * order;
				for (var i = 0; i <= order; i++)
					for (var j = 0; j <= porder; j++)
					{
						var mul = coefficients[i] * res[j];
						tmp[i + j] += mul;
					}
				for (int i = 0; i <= porder + order; i++)
				{
					res[i] = tmp[i];
					tmp[i] = 0;
				}
			}
			return new Polynomial(res);
		}

		public Polynomial Derivate()
		{
			var res = new double[Math.Max(1, coefficients.Length - 1)];
			for (int i = 1; i < coefficients.Length; i++)
				res[i - 1] = i * coefficients[i];
			return new Polynomial(res);
		}

		public Polynomial Integrate(double term0 = 0)
		{
			var res = new double[coefficients.Length + 1];
			res[0] = term0;
			for (int i = 0; i < coefficients.Length; i++)
				res[i + 1] = coefficients[i] / (i + 1);
			return new Polynomial(res);
		}

		#endregion

		#region interpolation: BezierCurves

		public static Polynomial LinearBezierCurve(double p0, double p1)
		{
			var T = new Polynomial(0, 1);
			return p0 + T * (p1 - p0);
		}
		public static Polynomial QuadraticBezierCurve(double p0, double p1, double p2)
		{
			var T = new Polynomial(0, 1);
			return (1 - T) * LinearBezierCurve(p0, p1) + T * LinearBezierCurve(p1, p2);
		}
		public static Polynomial CubicBezierCurve(double p0, double p1, double p2, double p3)
		{
			var T = new Polynomial(0, 1);
			return (1 - T) * QuadraticBezierCurve(p0, p1, p2) + T * QuadraticBezierCurve(p1, p2, p3);
		}

		#endregion

		#region FindRoots()

		/// <summary>
		/// This method use the Durand-Kerner aka Weierstrass algorithm to find approximate root of this polynomial.
		/// http://en.wikipedia.org/wiki/Durand%E2%80%93Kerner_method
		/// </summary>
		public Complex[] FindRoots(int maxIteration = 10)
		{
			var p = this.Normalize();
			if (p.coefficients.Length == 1) return new Complex[0];

			Complex x0 = 1;
			Complex xMul = 0.4 + 0.9 * Complex.I;
			var R0 = new Complex[p.coefficients.Length - 1];
			for (int i = 0; i < R0.Length; i++)
			{
				R0[i] = x0;
				x0 *= xMul;
			}

			var R1 = new Complex[p.coefficients.Length - 1];
			Func<int, Complex> divider = i =>
			{
				Complex div = 1;
				for (int j = 0; j < R0.Length; j++)
				{
					if (j == i) continue;
					div *= R0[i] - R0[j];
				}
				return div;
			};
			Action step = () =>
			{
				for (int i = 0; i < R0.Length; i++)
				{
					R1[i] = R0[i] - p.Compute(R0[i]) / divider(i);
				}
			};
			Predicate<Complex> negligible = c => Math.Abs(c.Real) <= Epsilon && Math.Abs(c.Imaginary) <= Epsilon;
			Func<bool> closeEnough = () =>
			{
				for (int i = 0; i < R0.Length; i++)
				{
					var c = R0[i] - R1[i];
					if (!negligible(c)) return false;
				}
				return true;
			};
			int iStep = 0;
			bool close = false;
			while (!close && iStep++ < maxIteration)
			{
				step();
				close = closeEnough();

				var tmp = R0;
				R0 = R1;
				R1 = tmp;
			}

			return R0;
		}

		#endregion

		#region overrides ToString() Equals()

		public override string ToString()
		{
			var sb = new StringBuilder();
			for (int i = 0; i < coefficients.Length; i++)
			{
				var val = coefficients[i];
				if (Math.Abs(val) < Epsilon)
					continue;
				if (val > 0 && sb.Length > 0)
					sb.Append('+');
				if (i > 0 && (Math.Abs(val) - 1) < Epsilon)
				{
					if (val < 0)
						sb.Append('-');
				}
				else
				{
					sb.Append(val);
				}
				if (i > 0)
					sb.Append('x');
				if (i > 1)
					sb.Append('^').Append(i);
			}
			if (sb.Length == 0)
				sb.Append('0');
			return sb.ToString();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, this)) return true;
			var p = obj as Polynomial;
			if (p == null) return false;
			if (coefficients.Length != p.coefficients.Length) return false;
			for (int i = 0; i < coefficients.Length; i++)
				if (Math.Abs(coefficients[i] - p.coefficients[i]) > Epsilon) return false;
			return true;
		}

		#endregion
	}
#pragma warning restore
}
