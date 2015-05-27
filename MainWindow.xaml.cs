using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PanoramaMaps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Drawing Info
        private BitmapImage src;

        // Viewport Info
        Point focus;
        Point velocity;
        double velStep = 1.8;
        double width2;
        double height2;

        // Mouse Info
        Point mouseDownPoint;
        bool lmb;
        bool rmb;

        // Time Info
        Stopwatch stopwatch;
        double oldTime;
        double newTime;
        double delta;

        private Random r;

        public MainWindow()
        {
            InitializeComponent();

            /*r = new Random();

            src = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "Assets", "open-desert-test.jpg")));

            image1.Source = src;
            image2.Source = src;

            focus = new Point();
            velocity = new Point();

            stopwatch = new Stopwatch();
            stopwatch.Start();

            deltas = new double[4];

            CompositionTarget.Rendering += Render;*/
        }

        double[] deltas;
        int idx;
        protected void Render(object sender, EventArgs e)
        {
            oldTime = newTime;
            newTime = stopwatch.ElapsedMilliseconds;
            delta = (newTime - oldTime) / 1000;

            SetFocus(focus.X + (velocity.X * delta), focus.Y + (velocity.Y * delta));
            SetCurrentView();

            deltas[(idx = (idx + 1) % 4)] = delta;

            this.Title = (1 / Average(deltas)).ToString("F4");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            width2 = canvas.RenderSize.Width / 2;
            height2 = canvas.RenderSize.Height / 2;

            CheckImageFit();
            SetFocus(src.PixelWidth / 2, src.PixelHeight / 2);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width2 = canvas.RenderSize.Width / 2;
            height2 = canvas.RenderSize.Height / 2;

            //reticle.SetValue(Canvas.LeftProperty, canvas.RenderSize.Width / 2 - reticle.ActualWidth / 2);
            //reticle.SetValue(Canvas.TopProperty, canvas.RenderSize.Height / 2 - reticle.ActualHeight / 2);

            CheckImageFit();
            SetFocus(focus.X, focus.Y);
        }

        private void imageElem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                lmb = true;
                mouseDownPoint = e.GetPosition(canvas);
            }

            if (e.ChangedButton == MouseButton.Right) rmb = true;
        }

        private void imageElem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                lmb = false;
                velocity.X = 0;
                velocity.Y = 0;
            }

            if (e.ChangedButton == MouseButton.Right) rmb = false;
        }

        private void imageElem_MouseMove(object sender, MouseEventArgs e)
        {
            if (lmb)
            {
                Point dragPoint = e.GetPosition(canvas);
                double xmag = mouseDownPoint.X - dragPoint.X;
                double ymag = mouseDownPoint.Y - dragPoint.Y;

                velocity.X = -xmag * velStep;
                velocity.Y = -ymag * velStep;
            }
        }

        #region Utility Setters
        private void CheckImageFit()
        {
            /*if (canvas.RenderSize.Height > image1.ActualHeight)
            {
                Console.WriteLine(image1.ActualHeight);

                double newScale = canvas.RenderSize.Height / src.PixelHeight;
                scaleTransform1.ScaleX = newScale;
                scaleTransform1.ScaleY = newScale;
                scaleTransform2.ScaleX = newScale;
                scaleTransform2.ScaleY = newScale;

                Console.WriteLine(image1.ActualHeight);
            }*/
        }

        private void SetFocus(double x, double y)
        {
            focus.X = x;
            focus.Y = y;

            if (focus.X < 0)
            {
                focus.X += image1.ActualWidth;
            }
            
            if (focus.Y - height2 < 0) 
            {
                focus.Y = height2;
            }

            if (focus.X >= image1.ActualWidth)
            {
                focus.X -= image1.ActualWidth;
            }

            if (focus.Y + height2 >= image1.ActualHeight)
            {
                focus.Y = image1.ActualHeight - height2;
            }
        }

        private void SetCurrentView()
        {
            image1.SetValue(Canvas.LeftProperty, -(focus.X - width2));
            image1.SetValue(Canvas.TopProperty, -(focus.Y - height2));

            if (focus.X - width2 < 0)
            {
                if (image2.Visibility == System.Windows.Visibility.Hidden)
                   image2.Visibility = System.Windows.Visibility.Visible;

                image2.SetValue(Canvas.LeftProperty, -(focus.X - width2) - image1.ActualWidth);
                image2.SetValue(Canvas.TopProperty, -(focus.Y - height2));
            }
            else if (focus.X + width2 >= src.PixelWidth)
            {
                if (image2.Visibility == System.Windows.Visibility.Hidden)
                    image2.Visibility = System.Windows.Visibility.Visible;

                image2.SetValue(Canvas.LeftProperty, -(focus.X - width2) + image1.ActualWidth);
                image2.SetValue(Canvas.TopProperty, -(focus.Y - height2));
            }
            else
            {
                if (image2.Visibility != System.Windows.Visibility.Hidden)
                    image2.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        #endregion

        #region Utility
        private double Distance(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private double Average(params double[] values)
        {
            double val = 0;

            for (int i = 0; i < values.Length; i++)
                val += values[i];

            return val / values.Length;
        }
        #endregion
    }
}
