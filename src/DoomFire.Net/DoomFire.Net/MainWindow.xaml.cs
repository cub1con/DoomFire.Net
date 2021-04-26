using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

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
            //this.Fire = new DoomFire(this.ImgFire.Width, this.ImgFire.Height, 60);
            this.Fire = new DoomFire(200, 100, 60);
            this.Fire.FrameRenderd += Fire_FrameRenderd;
        }

        private void Fire_FrameRenderd(object sender, EventArgs e)
        {
            this.ImgFire.Source = Convert(Fire.BITMAP);
        }


        public BitmapImage Convert(Bitmap src)
        {
            var ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
