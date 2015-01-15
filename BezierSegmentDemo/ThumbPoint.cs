using System.Windows;
using System.Windows.Controls.Primitives;

namespace BezierSegmentDemo
{
    /// <summary>
    /// Inherit Thumb control to be able to update a Point dependency property while the thumb
    /// is being dragged.
    /// </summary>
    public class ThumbPoint : Thumb
    {
        #region Point
        /// <summary>
        /// Point Dependency Property
        /// </summary>
        public static readonly DependencyProperty PointProperty = DependencyProperty.Register(
            "Point", 
            typeof(Point), 
            typeof(ThumbPoint),
            new FrameworkPropertyMetadata(new Point()));

        /// <summary>
        /// Gets or sets the Point property
        /// </summary>
        public Point Point
        {
            get { return (Point)GetValue(PointProperty); }
            set { SetValue(PointProperty, value); }
        }
        #endregion

        static ThumbPoint()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThumbPoint), new FrameworkPropertyMetadata(typeof(ThumbPoint)));
        }

        public ThumbPoint()
        {
            this.DragDelta += new DragDeltaEventHandler(this.OnDragDelta);
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            this.Point = new Point(this.Point.X + e.HorizontalChange, this.Point.Y + e.VerticalChange);
        }
    }
}
