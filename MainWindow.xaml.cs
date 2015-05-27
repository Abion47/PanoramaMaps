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
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private void imageElem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void imageElem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void imageElem_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        
    }
}
