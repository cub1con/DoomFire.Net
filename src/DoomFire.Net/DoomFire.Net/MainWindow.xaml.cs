using System;
using System.ComponentModel;
using System.Windows;
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


            this.Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                //this.Fire = new DoomFire((int)this.ActualWidth, (int)this.ActualHeight, 27);

                this.Fire = new DoomFire(800, 640, 15);
                this.Fire.FrameRenderd += Fire_FrameRenderd;
                this.Fire.FpsUpdated += Fire_FpsUpdated;
                //this.ImgFire.Source = this.Fire.Frame;
                //await this.Fire.RenderFrames();
                await this.Fire.RenderFramesAtInterval();
            };

        }

        private void Fire_FpsUpdated(object sender, EventArgs e)
        {
            this.Title = $"DoomFire.Net ~({this.Fire.FPS}fps / {this.Fire.TotalFramesRendered}frames total  / running for {DateTime.Now - this.Fire.RunningSince})";
        }

        private void Fire_FrameRenderd(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                this.ImgFire.Source = this.Fire.Frame;
            }, DispatcherPriority.ContextIdle);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
