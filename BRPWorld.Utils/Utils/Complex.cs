using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRPWorld.Utils.Utils
{
	public struct Complex
	{
		public float Real, Imaginary;

		public static readonly Complex I = new Complex { Real = 0, Imaginary = 1 };

		public Complex(float r, float i)
		{
			Real = r;
			Imaginary = i;
		}

		public static implicit operator Complex(float d) { return new Complex(d, 0); }
		public static implicit operator Complex(double x) { return new Complex((float)x, 0); }

		#region math: - + * /  == !=

		public static Complex operator -(Complex c) { return new Complex(-c.Real, -c.Imaginary); }
		public static Complex operator +(Complex c1, Complex c2) { return new Complex(c1.Real + c2.Real, c1.Imaginary + c2.Imaginary); }
		public static Complex operator -(Complex c1, Complex c2) { return new Complex(c1.Real - c2.Real, c1.Imaginary - c2.Imaginary); }
		public static Complex operator *(Complex c1, Complex c2) { return new Complex(c1.Real * c2.Real - c1.Imaginary * c2.Imaginary, c1.Real * c2.Imaginary + c1.Imaginary * c2.Real); }
		public static Complex operator /(Complex c1, Complex c2)
		{
			var div = c2.Real * c2.Real + c2.Imaginary * c2.Imaginary;
			return new Complex((c1.Real * c2.Real + c1.Imaginary * c2.Imaginary) / div, (c1.Imaginary * c2.Real - c1.Real * c2.Imaginary) / div);
		}

		public static bool operator ==(Complex c1, Complex c2) { return c1.Real == c2.Real && c1.Imaginary == c2.Imaginary; }
		public static bool operator !=(Complex c1, Complex c2) { return c1.Real != c2.Real || c1.Imaginary != c2.Imaginary; }

		#endregion

		public Complex Conjugate() { return new Complex(Real, -Imaginary); }
		public static Complex operator !(Complex c) { return c.Conjugate(); }

		#region overrides

		public override bool Equals(object obj)
		{
			if (!(obj is Complex))
				return false;
			return this == (Complex)obj;
		}
		public override int GetHashCode() { unchecked { return Real.GetHashCode() ^ Imaginary.GetHashCode(); } }
		public override string ToString()
		{
			if (Imaginary == 0)
				return Real.ToString();
			if (Real == 0)
			{
				if (Imaginary < 0)
					return "-i" + (-Imaginary);
				return "i" + Imaginary;
			}
			if (Imaginary < 0)
				return string.Format("{0}-i{1}", Real, -Imaginary);
			return string.Format("{0}+i{1}", Real, Imaginary);
		}

		#endregion
	}
}
