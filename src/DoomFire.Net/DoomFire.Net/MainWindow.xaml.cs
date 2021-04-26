using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
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

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += delegate(object sender, RoutedEventArgs args)
            {
                //this.Fire = new DoomFire((int)this.ActualWidth, (int)this.ActualHeight, 27);
                this.Fire = new DoomFire(256, 144, 27);
                this.Fire.FrameRenderd += Fire_FrameRenderd;
                this.Fire.FpsUpdated += Fire_FpsUpdated;
                this.Fire.RenderFramesAtInterval();
            };
        }

        private void Fire_FpsUpdated(object sender, EventArgs e)
        {
            this.Title = $"DoomFire.Net ~({this.Fire.FPS}fps / {this.Fire.TotalFramesRendered}frames total  / running for {DateTime.Now - this.Fire.RunningSince})";
        }

        private void Fire_FrameRenderd(object sender, Bitmap frame)
        {
            var convertedFrame = Convert(frame);

            Dispatcher.Invoke(() =>
            {
                this.ImgFire.Source = convertedFrame;
            }, DispatcherPriority.ContextIdle);
        }


        public BitmapImage Convert(Bitmap src)
        {
            var sp = new Stopwatch();
            sp.Start();

            var ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            sp.Stop();
            Debug.WriteLine($"Converting Image took {sp.Elapsed}");

            return image;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
