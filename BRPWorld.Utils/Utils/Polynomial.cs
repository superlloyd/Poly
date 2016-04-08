using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BRPWorld.Utils.Utils
{
    // Don't worry about Equals() without GetHashCode()
#pragma warning disable 659

    public class Polynomial
    {
        double[] coefficients;

        /// <summary>
        /// Empty constructor to enable activation
        /// </summary>
        public Polynomial()
        {
            this.coefficients = new double[1];
        }

        public Polynomial(params double[] coefficients)
        {
            this.coefficients = coefficients;
            if (coefficients == null || coefficients.Length == 0)
                this.coefficients = new double[1];
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
                if (index < 0)
                    throw new ArgumentOutOfRangeException();
                if (index >= this.coefficients.Length)
                    return 0;
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

        #region Normalize() Trim() RealOrder() Epsilon

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

        public int RealOrder() { return RealOrder(coefficients); }
        public static int RealOrder(params double[] coefficients)
        {
            if (coefficients == null)
                return 0;
            int order = 0;
            for (int i = 0; i < coefficients.Length; i++)
            {
                if (Math.Abs(coefficients[i]) > Epsilon)
                {
                    order = i;
                }
            }
            return order;
        }

        public static double Epsilon
        {
            get { return sEpsilon; }
            set { sEpsilon = Math.Abs(value); }
        }
        [ThreadStatic]
        static double sEpsilon = 0.00001;

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

        #region FindRoots()

        /// <summary>
        /// This method use the Durand-Kerner aka Weierstrass algorithm to find approximate root of this polynomial.
        /// http://en.wikipedia.org/wiki/Durand%E2%80%93Kerner_method
        /// </summary>
        public Complex[] FindRoots()
        {
            var p = this.Normalize();
            if (p.coefficients.Length == 1) return new Complex[0];

            Complex x0 = 1;
            Complex xMul = 0.4 + 0.9 * Complex.ImaginaryOne;
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
            Func<bool> closeEnough = () =>
            {
                for (int i = 0; i < R0.Length; i++)
                {
                    var c = R0[i] - R1[i];
                    if (Math.Abs(c.Real) > Epsilon || Math.Abs(c.Imaginary) > Epsilon) return false;
                }
                return true;
            };
            bool close = false;
            do
            {
                step();
                close = closeEnough();

                var tmp = R0;
                R0 = R1;
                R1 = tmp;
            }
            while (!close);

            return R0;
        }

        #endregion

        #region SolveRealRoots() SolveOrFindRealRoots()

        public bool CanSolveRealRoot()
        {
            var o = RealOrder();
            if (o <= 4)
                return true;
            return false;
        }

        /// <summary>
        /// This will solve analytically polynomial up to 4th order
        /// </summary>
        public IEnumerable<double> SolveRealRoots()
        {
            return SolveRealRoots(coefficients);
        }
        /// <summary>
        /// This will solve analytically polynomial up to 4th order
        /// </summary>
        public static IEnumerable<double> SolveRealRoots(params double[] poly)
        {
            switch (RealOrder(poly))
            {
                case 0:
                    break;
                case 1:
                    yield return -poly[0] / poly[1];
                    break;
                case 2:
                    {
                        var delta = poly[1] * poly[1] - 4 * poly[2] * poly[0];
                        if (delta < 0)
                            yield break;
                        var sd = Math.Sqrt(delta);
                        yield return (-poly[1] - sd) / 2 / poly[2];
                        if (sd > Epsilon)
                            yield return (-poly[1] + sd) / 2 / poly[2];
                    }
                    break;
                case 3:
                    {
                        // http://www.trans4mind.com/personal_development/mathematics/polynomials/cubicAlgebra.htm
                        // http://pomax.github.io/bezierinfo/#extremities
                        // x^3 + a x^2 + b x + c = 0
                        var a = poly[2] / poly[3];
                        var b = poly[1] / poly[3];
                        var c = poly[0] / poly[3];
                        // x = t - a/3
                        // t3 + p t + q = 0
                        var p = -a * a / 3 + b;
                        var q = (2 * a * a * a - 9 * a * b + 27 * c) / 27;
                        if (Math.Abs(p) < Epsilon)
                        {
                            // t^3 + q = 0  => t = -q^1/3 => x = -q^1/3 - a/3
                            yield return -Crt(p) - a / 3;
                        }
                        else if (Math.Abs(q) < Epsilon)
                        {
                            // t^3 + pt = 0  => t (t^2 + p) = 0
                            // t = 0, t = +/- (-p)^1/2
                            yield return -a / 3;
                            if (p < 0)
                            {
                                var root = Crt(p);
                                yield return root - a / 3;
                                yield return -root - a / 3;
                            }
                        }
                        else
                        {
                            var disc = q * q / 4 + p * p * p / 27;
                            if (disc < -Epsilon)
                            {
                                // 3 roots
                                var r = Math.Sqrt(-p * p * p / 27);
                                var phi = Math.Acos(MinMax(-q / 2 / r, -1, 1));
                                var t1 = 2 * Crt(r);
                                yield return t1 * Math.Cos(phi / 3) - a / 3;
                                yield return t1 * Math.Cos((phi + 2 * Math.PI) / 3) - a / 3;
                                yield return t1 * Math.Cos((phi + 4 * Math.PI) / 3) - a / 3;
                            }
                            else if (disc < Epsilon)
                            {
                                // 2 real roots
                                var cq = Crt(q / 2);
                                yield return -2 * cq - a / 3;
                                yield return cq - a / 3;
                            }
                            else
                            {
                                // 1 real root
                                var sd = Math.Sqrt(disc);
                                yield return Crt(-q / 2 + sd) - Crt(q / 2 + sd) - a / 3;
                            }
                        }
                    }
                    break;
                case 4:
                    {
                        // https://en.wikipedia.org/wiki/Quartic_function#General_formula_for_roots
                        // x^4 + b x^3 + c x^2 + d x + e = 0
                        var b = poly[3] / poly[4];
                        var c = poly[2] / poly[4];
                        var d = poly[1] / poly[4];
                        var e = poly[0] / poly[4];
                        // <=> y^4 + p x^2 + q x + r = 0, 
                        // where x = y - b / 4
                        var p = c - 3 * b * b / 8;
                        var q = (b * b * b - 4 * b * c + 8 * d) / 8;
                        var r = (-3 * b * b * b * b + 256 * e - 64 * b * d + 16 * b * b * c) / 256;
                        if (Math.Abs(q) <= Epsilon)
                        {
                            // z = y^2, x = +/- sqrt(z) - b/4, z^2 + p z + r = 0
                            foreach (var z in SolveRealRoots(r, p, 1))
                            {
                                if (z < -Epsilon)
                                {
                                    continue;
                                }
                                if (z <= Epsilon)
                                {
                                    yield return -b / 4;
                                    continue;
                                }
                                var y = Math.Sqrt(z);
                                yield return y - b / 4;
                                yield return -y - b / 4;
                            }
                            yield break;
                        }
                        // <=> (y^2 + p + m)^2 = (p + 2m) y^2 -q y + (m^2 + 2 m p + p^2 - r)
                        // where m is **arbitrary** choose it to make perfect square
                        // i.e. m^3 + 5/2 p m^2 + (2 p^2 - r) m + (p^3/2 - p r /2 - q^2 / 8) = 0
                        var m = SolveRealRoots(p * p * p / 2 - p * r / 2 - q * q / 8, 2 * p * p - r, 5.0 / 2.0 * p, 1)
                            .Where(x => p + 2 * x > Epsilon)
                            .First();
                        // <=> (y^2 + y Sqrt(p+2m) + p+m - q/2/sqrt(p+2m)) (y^2 - y Sqrt(p+2m) + p+m + q/2/sqrt(p+2m))
                        var sqrt = Math.Sqrt(p + 2 * m);
                        var poly1 = SolveRealRoots(p + m - q / 2 / sqrt, sqrt, 1);
                        var poly2 = SolveRealRoots(p + m + q / 2 / sqrt, -sqrt, 1);
                        foreach (var y in poly1.Concat(poly2))
                            yield return y - b / 4;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Crt(double x)
        {
            if (x < 0)
                return -Math.Pow(-x, 1.0 / 3.0);
            return Math.Pow(x, 1.0 / 3.0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double MinMax(double x, double min, double max)
        {
            if (x < min)
                return min;
            if (x > max)
                return max;
            return x;
        }

        /// <summary>
        /// Will try to solve root analytically, and if it can will use numerical approach.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> SolveOrFindRealRoot()
        {
            if (CanSolveRealRoot())
                return SolveRealRoots();
            return FindRoots().Where(c => Math.Abs(c.Imaginary) < Epsilon).Select(c => c.Real);
        }

        #endregion

        #region Interpolate()

        /// <summary>
        /// Construct a polynomial P such as ys[i] = P.Compute(i).
        /// </summary>
        public static Polynomial Interpolate(params double[] ys)
        {
            if (ys == null || ys.Length < 2)
                throw new ArgumentNullException("At least 2 different points must be given");

            var res = new Polynomial();
            for (int i = 0; i < ys.Length; i++)
            {
                var e = new Polynomial(1);
                for (int j = 0; j < ys.Length; j++)
                {
                    if (j == i)
                        continue;
                    e *= new Polynomial(-j, 1) / (i - j);
                }
                res += ys[i] * e;
            }
            return res.Trim();
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

#pragma warning restore 659
}
