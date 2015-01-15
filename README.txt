This class is an excerpt from my personal project.

It contains all the code needed to Point to BezierSegment distance measurement.
It turned out to be quite a lot of Math!

One need:
- Complex arithmetic (check, Complex.cs)
- Polynomial class with the following operation
	- + - * /
	Derivate()
	FindRoots() (i.e. value for which the Polynomial compute to 0)
	Compute(x), of course! for double and Complex
	BezierEquations

Finally in PolygonUtils there are the (now very simple) measurement methods



CREDITS
=======
Special Thanks to "Jeremy Alles" for his Bezier Demo
http://www.japf.fr/2009/07/beziersegment-demo-application/
Which I used as the basis of my visual test.