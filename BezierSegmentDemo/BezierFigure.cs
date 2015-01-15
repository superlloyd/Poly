using System.Windows;
using System.Windows.Controls;

namespace BezierSegmentDemo
{
    /// <summary>
    /// The BezierFigure control is very simple: it just has 4 dependency properties of type Point. 
    /// The interesting part of the BezierFigure control is its template: see Window1.xaml.
    /// </summary>
    public class BezierFigure : Control
    {
        #region StartPoint
        /// <summary>
        /// StartPoint Dependency Property
        /// </summary>
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
                "StartPoint", 
                typeof(Point), 
                typeof(BezierFigure),
                new FrameworkPropertyMetadata(new Point()));

        /// <summary>
        /// Gets or sets the StartPoint property
        /// </summary>
        public Point StartPoint
        {
            get { return (Point)GetValue(StartPointProperty); }
            set { SetValue(StartPointProperty, value); }
        }
        #endregion

        #region EndPoint
        /// <summary>
        /// EndPoint Dependency Property
        /// </summary>
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
                "EndPoint",
                typeof(Point),
                typeof(BezierFigure),
                new FrameworkPropertyMetadata(new Point()));

        /// <summary>
        /// Gets or sets the EndPoint property
        /// </summary>
        public Point EndPoint
        {
            get { return (Point)GetValue(EndPointProperty); }
            set { SetValue(EndPointProperty, value); }
        }
        #endregion

        #region StartBezierPoint
        /// <summary>
        /// StartBezierPoint Dependency Property
        /// </summary>
        public static readonly DependencyProperty StartBezierPointProperty = DependencyProperty.Register(
                "StartBezierPoint",
                typeof(Point),
                typeof(BezierFigure),
                new FrameworkPropertyMetadata(new Point()));

        /// <summary>
        /// Gets or sets the StartBezierPoint property
        /// </summary>
        public Point StartBezierPoint
        {
            get { return (Point)GetValue(StartBezierPointProperty); }
            set { SetValue(StartBezierPointProperty, value); }
        }
        #endregion

        #region EndBezierPoint
        /// <summary>
        /// StartBezierPoint Dependency Property
        /// </summary>
        public static readonly DependencyProperty EndBezierPointProperty = DependencyProperty.Register(
                "EndBezierPoint",
                typeof(Point),
                typeof(BezierFigure),
                new FrameworkPropertyMetadata(new Point()));

        /// <summary>
        /// Gets or sets the StartBezierPoint property
        /// </summary>
        public Point EndBezierPoint
        {
            get { return (Point)GetValue(EndBezierPointProperty); }
            set { SetValue(EndBezierPointProperty, value); }
        }
        #endregion

        static BezierFigure()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BezierFigure), new FrameworkPropertyMetadata(typeof(BezierFigure)));
        }
    }
}
