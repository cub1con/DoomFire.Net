using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DoomFireNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DoomFire Fire;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (Fire != null)
            {
                Fire.RenderFrame();
                drawingContext.DrawImage(Fire.Writer, new Rect(new Point(0, 0), new Size(800, 600)));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += delegate(object sender, RoutedEventArgs args)
            {
                //this.Fire = new DoomFire((int)this.ActualWidth, (int)this.ActualHeight, 27);
                this.Fire = new DoomFire(800, 600, 60);
                this.Fire.FrameRenderd += Fire_FrameRenderd;
                this.Fire.FpsUpdated += Fire_FpsUpdated;
                this.Fire.RenderFramesAtInterval();
            };
        }

        private void Fire_FpsUpdated(object sender, EventArgs e)
        {
            this.Title = $"DoomFire.Net ~({this.Fire.Fps}fps / {this.Fire.TotalFramesRendered}frames total  / running for {DateTime.Now - this.Fire.RunningSince})";
        }

        private void Fire_FrameRenderd(object sender, BitmapSource frame)
        {
            Dispatcher.Invoke(() =>
            {
                this.ImgFire.Source = frame;
                this.ImgFire.InvalidateVisual();
            }, DispatcherPriority.ContextIdle);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
