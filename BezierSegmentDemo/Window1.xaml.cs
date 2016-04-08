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
			var bezier = new BezierFragment(figure.StartPoint, figure.StartBezierPoint, figure.EndBezierPoint, figure.EndPoint);
			MeasureMessage = string.Format("Distance to Curve: {0}", bezier.DistanceTo(MeasurePoint));
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
                OnPropertyChanged("NumPoints");
            }
        }
        int mNumPoints = 10;

        public double CutPoint
        {
            get { return mCutPoint; }
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 1)
                    value = 1;
                mCutPoint = value;
                OnPropertyChanged("CutPoint");
                UpdateCuts();
            }
        }
        double mCutPoint = 0.5;

        public Rect BezierBounds
        {
            get { return mBezierBounds; }
            private set
            {
                mBezierBounds = value;
                OnPropertyChanged("BezierBounds");
            }
        }
        Rect mBezierBounds;

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
			var bp = new BezierFragment(
				figure.StartPoint, 
				figure.StartBezierPoint,
				figure.EndBezierPoint,
				figure.EndPoint
			);
            BezierBounds = bp.BoundingBox();
			overlay.Children.Clear();
			var dt = 1.0 / NumPoints;
			for (int i = 0; i <= NumPoints; i++)
			{
				var t = i * dt;
				var p = bp.Compute(t);
				var it = new ThumbPoint { Point = p, Background = Brushes.CornflowerBlue };
				overlay.Children.Add(it);
			}
			UpdateMeasure();
            UpdateCuts();
        }
        public void UpdateCuts()
        {
            var bf = new BezierFragment(
                figure.StartPoint,
                figure.StartBezierPoint,
                figure.EndBezierPoint,
                figure.EndPoint);
            var split = bf.Split(CutPoint);

            var first = new BezierFigure
            {
                Foreground = Brushes.Red,
                StartPoint = split[0].ControlPoints[0],
                StartBezierPoint = split[0].ControlPoints[1],
                EndBezierPoint = split[0].ControlPoints[2],
                EndPoint = split[0].ControlPoints[3],
            };
            var last = new BezierFigure
            {
                Foreground = Brushes.Blue,
                StartPoint = split[1].ControlPoints[0],
                StartBezierPoint = split[1].ControlPoints[1],
                EndBezierPoint = split[1].ControlPoints[2],
                EndPoint = split[1].ControlPoints[3],
            };
            overlay2.Children.Clear();
            overlay2.Children.Add(first);
            overlay2.Children.Add(last);
        }
    }
}
