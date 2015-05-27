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

            r = new Random();

            src = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "Assets", "sony-center.jpg")));

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
            SetFocus(src.PixelWidth / 2, src.PixelHeight / 2);

            DrawingGroup drawings = new DrawingGroup();

            img1 = new ImageDrawing();
            drawings.Children.Add(img1);
            img2 = new ImageDrawing();
            drawings.Children.Add(img2);

            DrawingImage drawingImage = new DrawingImage(drawings);
            imageElem.Source = drawingImage;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (img1 != null)
            {
                width2 = canvas.RenderSize.Width / 2;
                height2 = canvas.RenderSize.Height / 2;

                SetFocus(focus.X, focus.Y);
                SetCurrentView();
            }
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
            if (focus.X - width2 < 0)
            {
                double aWidth = focus.X + width2;
                double bWidth = canvas.RenderSize.Width - aWidth;
                double ax = 0;
                double bx = src.PixelWidth - bWidth;
                double y = focus.Y - height2;

                cbmp1 = new CroppedBitmap(src, new Int32Rect(
                    (int)ax,
                    (int)y,
                    (int)aWidth, 
                    (int)canvas.RenderSize.Height));

                img1.Rect = new Rect(bWidth, 0, aWidth, canvas.RenderSize.Height);
                img1.ImageSource = cbmp1;

                cbmp2 = new CroppedBitmap(src, new Int32Rect(
                    (int)bx,
                    (int)y,
                    Math.Max((int)bWidth, 1),
                    (int)canvas.RenderSize.Height));

                img2.Rect = new Rect(0, 0, bWidth, canvas.RenderSize.Height);
                img2.ImageSource = cbmp2;
            }
            else if (focus.X + width2 >= src.PixelWidth)
            {
                double diff = src.PixelWidth - focus.X;
                double aWidth = width2 + diff;
                double bWidth = canvas.RenderSize.Width - aWidth;
                double ax = focus.X - width2;
                double bx = 0;
                double y = focus.Y - height2;

                cbmp1 = new CroppedBitmap(src, new Int32Rect(
                    (int)ax,
                    (int)y,
                    (int)aWidth,
                    (int)canvas.RenderSize.Height));

                img1.Rect = new Rect(0, 0, aWidth, canvas.RenderSize.Height);
                img1.ImageSource = cbmp1;

                cbmp2 = new CroppedBitmap(src, new Int32Rect(
                    (int)bx,
                    (int)y,
                    Math.Max((int)bWidth, 1),
                    (int)canvas.RenderSize.Height));

                img2.Rect = new Rect(aWidth, 0, bWidth, canvas.RenderSize.Height);
                img2.ImageSource = cbmp2;
            }
            else
            {
                cbmp1 = new CroppedBitmap(src, new Int32Rect(
                    (int)(focus.X - width2), 
                    (int)(focus.Y - height2), 
                    (int)canvas.RenderSize.Width, 
                    (int)canvas.RenderSize.Height));

                img1.Rect = new Rect(0, 0, canvas.RenderSize.Width, canvas.RenderSize.Height);
                img1.ImageSource = cbmp1;

                img2.Rect = new Rect(0, 0, 0, 0);
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

        /*
        Stopwatch stopwatch = new Stopwatch();
        long frameCounter;
        double lastSeconds;
        double colorStep = 30000.0;
        double currentPhase;
        protected void Render(object sender, EventArgs e)
        {
            if (frameCounter++ == 0)
            {
                stopwatch.Start();
            }

            double frameRate = frameCounter / stopwatch.Elapsed.TotalSeconds;
            if (frameRate > 0)
            {
                this.Title = frameRate.ToString("F2");
                lastSeconds = stopwatch.Elapsed.TotalSeconds;
            }

            double delta = stopwatch.Elapsed.TotalSeconds - lastSeconds;
            currentPhase += colorStep * delta;
            currentPhase = currentPhase > 3 ? currentPhase - 3 : currentPhase;

            byte red = (byte)((
                currentPhase <= 1 ? Lerp(0, 1, currentPhase) : 
                currentPhase <= 2 ? Lerp(1, 0, currentPhase - 1) : 0) * 255);
            byte green = (byte)((
                currentPhase >= 1 ? (
                    currentPhase <= 2 ? Lerp(0, 1, currentPhase - 1) : 
                    currentPhase <= 3 ? Lerp(1, 0, currentPhase - 2) : 0) : 0)  * 255);
            byte blue = (byte)((
                currentPhase <= 1 ? Lerp(1, 0, currentPhase) : 
                currentPhase >= 2 ? Lerp(0, 1, currentPhase - 2) : 0) * 255);

            Console.WriteLine(currentPhase);

            //canvas.Background = new SolidColorBrush(Color.FromRgb(red, green, blue));
            lastSeconds = stopwatch.Elapsed.TotalSeconds;
        }

        public static double Lerp(double a, double b, double t)
        {
            return (1 - t) * a + t * b;
        }
         * */
    }
}
