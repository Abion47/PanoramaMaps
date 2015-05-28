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
        private ImageDrawing img1;
        private ImageDrawing img2;
        private CroppedBitmap cbmp1;
        private CroppedBitmap cbmp2;

        // Static UI Info
        double reticleWidth2;
        double reticleHeight2;
        double arrowWidth2;
        double arrowHeight2;

        // Viewport Info
        Point focus;
        Point velocity;
        double velArrow = 200;
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

            r = new Random();

            using (System.Drawing.Bitmap temp = (System.Drawing.Bitmap)System.Drawing.Image.FromFile("Assets/mountain-lake-96dpi.jpg"))
            {
                temp.SetResolution(96, 96);

                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    temp.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                    stream.Position = 0;
                    src = new BitmapImage();
                    src.BeginInit();
                    src.StreamSource = stream;
                    src.CacheOption = BitmapCacheOption.OnLoad;
                    src.EndInit();
                }
            }

            Console.WriteLine(src.DpiX + " : " + src.DpiY);

            image1.Source = src;
            image2.Source = src;

            reticleWidth2 = reticle.ActualWidth / 2;
            reticleHeight2 = reticle.ActualHeight / 2;
            arrowWidth2 = leftArrow.ActualWidth / 2;
            arrowHeight2 = leftArrow.ActualHeight / 2;

            focus = new Point();
            velocity = new Point();

            stopwatch = new Stopwatch();
            stopwatch.Start();

            deltas = new double[4];

            CompositionTarget.Rendering += Render;
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

            reticle.SetValue(Canvas.LeftProperty, width2 - reticleWidth2);
            reticle.SetValue(Canvas.TopProperty, height2 - reticleHeight2);

            leftArrow.SetValue(Canvas.LeftProperty, 12.0);
            leftArrow.SetValue(Canvas.TopProperty, height2 - arrowHeight2);

            rightArrow.SetValue(Canvas.LeftProperty, canvas.RenderSize.Width - rightArrow.RenderSize.Width - 12.0);
            rightArrow.SetValue(Canvas.TopProperty, height2 - arrowHeight2);

            CheckImageFit();
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
            if (canvas.RenderSize.Height > image1.ActualHeight)
            {
                Console.WriteLine(image1.RenderSize.Height);

                scale1.CenterX = focus.X;
                scale1.CenterY = focus.Y;
                scale2.CenterX = focus.X;
                scale2.CenterY = focus.Y;

                double newScale = canvas.RenderSize.Height / image1.RenderSize.Height;

                scale1.ScaleX = newScale;
                scale1.ScaleY = newScale;
                scale2.ScaleX = newScale;
                scale2.ScaleY = newScale;

                Console.WriteLine(image1.RenderSize.Height);

                SetFocus(focus.X * scale1.ScaleX, focus.Y * scale1.ScaleY);
            }
        }

        private void SetFocus(double x, double y)
        {
            focus.X = x;
            focus.Y = y;

            if (focus.X < 0)
            {
                focus.X += src.PixelWidth;
            }

            if (focus.Y - height2 < 0)
            {
                focus.Y = height2;
            }

            if (focus.X >= src.PixelWidth)
            {
                focus.X -= src.PixelWidth;
            }

            if (focus.Y + height2 >= src.PixelHeight)
            {
                focus.Y = src.PixelHeight - height2;
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

        private void leftArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                velocity.X = -velArrow;
            }
        }

        private void leftArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                velocity.X = 0;
            }
        }

        private void rightArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                velocity.X = velArrow;
            }
        }

        private void rightArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                velocity.X = 0;
            }
        }
    }
}
