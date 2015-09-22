﻿using BRPWorld.Utils.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Tests
{
	[TestFixture]
	public class MathTest
	{
		[Test]
		public void TestPoly()
		{
			var p = new Polynomial(1, -1);
			var p3 = p.Pow(3);
			var p3p = p3.Derivate();
			var ppp = p3p - new Polynomial(2, 3);
			var p4 = new Polynomial(2, 3) * new Polynomial(1, -2, 2);

			var a1 = new Polynomial(1, -3, 3, -1);
			Assert.AreEqual(a1, p3);
			Assert.AreEqual(p3, p3p.Integrate(p3[0]));
			var a2 = new Polynomial(-3, 6, -3);
			Assert.AreEqual(a2, p3p);
			var a3 = new Polynomial(-5, 3, -3);
			Assert.AreEqual(a3, ppp);
			var a4 = new Polynomial(2, -1, -2, 6);
			Assert.AreEqual(a4, p4);
			Assert.AreEqual(new Polynomial(0, 0, 2.5), Polynomial.Term(2, 2.5));
		}

		[Test]
		public void TestPolyOp()
		{
			var X = new Polynomial(0, 1);
			Assert.AreEqual(new Polynomial(1, 1), 1 + X);
			Assert.AreEqual(new Polynomial(1, -1), 1 - X);
			Assert.AreEqual(new Polynomial(-1, 1), X - 1);
			Assert.AreEqual(new Polynomial(1, 1), X + 1);
			Assert.AreEqual(new Polynomial(1, 0, 1), 1 + (X ^ 2));
		}

        [Test]
        public void TestPolyBezier()
        {
            Action<Polynomial, double, double> check = (poly, p0, p1) =>
            {
                Assert.IsTrue(Math.Abs(poly.Compute(0) - p0) < 0.0001);
                Assert.IsTrue(Math.Abs(poly.Compute(1) - p1) < 0.0001);
            };
            check(Polynomial.LinearBezierCurve(0, 1), 0, 1);
            check(Polynomial.QuadraticBezierCurve(0, -1, 1), 0, 1);
            check(Polynomial.CubicBezierCurve(0, 2, 0.5, 1), 0, 1);
        }

        [Test]
        public void TestGeneralBezier()
        {
            var q1 = Polynomial.CubicBezierCurve(1, 2, 4, 8);
            var q2 = Polynomial.Bezier(1, 2, 4, 8);
            Assert.AreEqual(q1.Order, q2.Order);
            for (int i = 0; i < q1.Order; i++)
                Assert.IsTrue(Math.Abs(q1[i] - q2[i]) < 0.0001);
        }

        [Test]
		public void TestPolyFind()
		{
			TestPolyFind(new Polynomial(0.5, 1, -1, 0, 1));

			var p = new Polynomial(1, 1, 1);
			p[2] = 0;
			TestPolyFind(p);
		}
		public void TestPolyFind(Polynomial p)
		{
			var roots = p.FindRoots();

			Predicate<Complex> negligible = c => Math.Abs(c.Real) <= Polynomial.Epsilon && Math.Abs(c.Imaginary) <= Polynomial.Epsilon;
			foreach (var r in roots)
			{
				Assert.IsTrue(negligible(p.Compute(r)));
			}
		}
	}
}
