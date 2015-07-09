using BRPWorld.Utils.Utils;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using U = BRPWorld.Utils.Utils;

namespace BezierSegmentDemo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        private readonly Random random;
        private bool isAnimated;
        private double speed;

        // note: no INotifyPropertyChanged for this very basic example...
        public double Speed
        {
            get
            {
                return this.speed;
            }
            set
            {
                this.speed = value;
            }
        }

		public Point MeasurePoint
		{
			get { return mMeasurePoint; }
			set
			{
				mMeasurePoint = value;
				OnPropertyChanged();
				UpdateMeasure();
			}
		}
		Point mMeasurePoint = new Point(10, 10);

		void UpdateMeasure()
		{
			var bezier = PolygonUtils.CubicBezier(figure.StartPoint, figure.StartBezierPoint, figure.EndBezierPoint, figure.EndPoint);
			MeasureMessage = string.Format("Distance to Curve: {0}", bezier.DistanceTo(MeasurePoint.ToArray(), 0, 1));
			OnPropertyChanged("MeasureMessage");
		}
		public string MeasureMessage { get; set; }

		void OnPropertyChanged([CallerMemberName]string name = null)
		{
			var e = PropertyChanged;
			if (e != null)
				e(this, new PropertyChangedEventArgs(name));
		}
		public event PropertyChangedEventHandler PropertyChanged;

		public int NumPoints
		{
			get { return mNumPoints; }
			set
			{
				mNumPoints = value;
				UpdatePoints();
			}
		}
		int mNumPoints = 10;

        public bool IsAnimated
        {
            get
            {
                return this.isAnimated;
            }
            set
            {
                this.isAnimated = value;
                if(value)
                {
                    this.AnimateAll();
                }
            }
        }

        public Window1()
        {
            InitializeComponent();

            this.DataContext = this;
            this.Speed = 1;

            this.random = new Random((int)DateTime.Now.Ticks);

			ListenToFigure();
			UpdatePoints();
			UpdateMeasure();
		}

		void ListenToFigure()
		{
			ListenToFigure(BezierFigure.StartPointProperty);
			ListenToFigure(BezierFigure.StartBezierPointProperty);
			ListenToFigure(BezierFigure.EndBezierPointProperty);
			ListenToFigure(BezierFigure.EndPointProperty);
		}
		void ListenToFigure(DependencyProperty p)
		{
			var dpd = DependencyPropertyDescriptor.FromProperty(p, typeof(BezierFigure));
			if (dpd != null)
			{
				dpd.AddValueChanged(figure, (o, e) => { UpdatePoints(); });
			}
		}

        private PointAnimation CreateRandomPointAnimation()
        {
            // create a PointAnimation that can animate a point in the window
            return new PointAnimation(new Point(
                this.random.NextDouble() * this.ActualWidth,
                this.random.NextDouble() * this.ActualHeight),
                TimeSpan.FromSeconds(this.Speed));
        }

        private void AnimateAll()
        {
            // the first time the animations start, they will reset the binding set in
            // the BezierFigure control and drag will no longer work
            this.Animate(BezierFigure.StartPointProperty);
            this.Animate(BezierFigure.StartBezierPointProperty);
            this.Animate(BezierFigure.EndPointProperty);
            this.Animate(BezierFigure.EndBezierPointProperty);
        }

        private void Animate(DependencyProperty property)
        {
            var animation = this.CreateRandomPointAnimation();

            if(this.IsAnimated)
            {
                animation.Completed += (s, e) => this.Animate(property);
            }

            this.figure.BeginAnimation(property, animation);
        }

		public void UpdatePoints()
		{
			var bp = U.PolygonUtils.CubicBezier(
				figure.StartPoint, 
				figure.StartBezierPoint,
				figure.EndBezierPoint,
				figure.EndPoint
			);
			overlay.Children.Clear();
			var dt = 1.0 / NumPoints;
			for (int i = 0; i <= NumPoints; i++)
			{
				var t = i * dt;
				var p = PolygonUtils.PointFromArray(bp.Compute(t));
				var it = new ThumbPoint { Point = p, Background = Brushes.CornflowerBlue };
				overlay.Children.Add(it);
			}
			UpdateMeasure();
		}
    }
}
