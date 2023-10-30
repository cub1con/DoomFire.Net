using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace DoomFireNet
{
    public class DoomFire
    {
        public WriteableBitmap Frame { get; private set; }

        private int FramesRendered;

        private int _fps;

        public int FPS
        {
            get { return _fps;}
            private set
            {
                _fps = value;
                OnFpsUpdate();
            }
        }

        public int TotalFramesRendered { get; private set; }

        public event EventHandler FrameRenderd;

        public event EventHandler FpsUpdated;

        public DateTime RunningSince { get; private set; } = new DateTime();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public long MaxTicksPerSecond { get; private set;}

        private Random random = new Random();

        public Color[] FireColors { get; private set; } = new Color[37];
        public int[] FirePixels { get; private set; }
        private readonly int[] FireRgb = 
        {
            0x07, 0x07, 0x07, 0x1F, 0x07, 0x07, 0x2F, 0x0F, 0x07, 0x47, 0x0F, 0x07, 0x57, 0x17, 0x07, 0x67,
            0x1F, 0x07, 0x77, 0x1F, 0x07, 0x8F, 0x27, 0x07, 0x9F, 0x2F, 0x07, 0xAF, 0x3F, 0x07, 0xBF, 0x47,
            0x07, 0xC7, 0x47, 0x07, 0xDF, 0x4F, 0x07, 0xDF, 0x57, 0x07, 0xDF, 0x57, 0x07, 0xD7, 0x5F, 0x07,
            0xD7, 0x5F, 0x07, 0xD7, 0x67, 0x0F, 0xCF, 0x6F, 0x0F, 0xCF, 0x77, 0x0F, 0xCF, 0x7F, 0x0F, 0xCF,
            0x87, 0x17, 0xC7, 0x87, 0x17, 0xC7, 0x8F, 0x17, 0xC7, 0x97, 0x1F, 0xBF, 0x9F, 0x1F, 0xBF, 0x9F,
            0x1F, 0xBF, 0xA7, 0x27, 0xBF, 0xA7, 0x27, 0xBF, 0xAF, 0x2F, 0xB7, 0xAF, 0x2F, 0xB7, 0xB7, 0x2F,
            0xB7, 0xB7, 0x37, 0xCF, 0xCF, 0x6F, 0xDF, 0xDF, 0x9F, 0xEF, 0xEF, 0xC7, 0xFF, 0xFF, 0xFF
        };



        DateTime _lastTime; // marks the beginning the measurement began

        public DoomFire(int width, int height, int targetFps)
        {
            this.Height = height;
            this.Width = width;
            this.FirePixels = new int[this.Width * this.Height];

            this.Frame = new WriteableBitmap(this.Width, this.Height, 96, 96, PixelFormats.Bgra32, null);

            this.MaxTicksPerSecond = TimeSpan.TicksPerSecond / targetFps;

            // Fill Color pallete
            for (var i = 0; i < this.FireColors.Length; i++)
            {
                this.FireColors[i] = Color.FromArgb((byte)this.FireRgb[i * 3 + 0], (byte)this.FireRgb[i * 3 + 1], (byte)this.FireRgb[i * 3 + 2]);
            }

            // Preset Frame
            for (var i = 0; i < Width * Height; i++)
            {
                this.FirePixels[i] = 0;
            }

            // Preset "fire source" in frame
            for (var i = 0; i < Width; i++)
            {
                this.FirePixels[(Height - 1) * Width + i] = 36;
            }
        }

        public async Task RenderFramesAtInterval()
        {
            this.RunningSince = DateTime.Now;
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16.6f));

            while (await timer.WaitForNextTickAsync())
            {
                RenderFrame();
            }
        }

        public async Task RenderFrames()
        {
            this.RunningSince = DateTime.Now;
            while (true)
            {
                var startTicks = Environment.TickCount;
                RenderFrame();
                await Task.Delay(Convert.ToInt32(Environment.TickCount - startTicks));
            }
        }

        protected virtual void OnFpsUpdate()
        {
            this.FpsUpdated.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnFrameRendered()
        {
            //Debug.Print($"Rendered Frame {this.TotalFramesRendered}");
            this.FrameRenderd.Invoke(this, EventArgs.Empty);
        }


        private void DoFire()
        {
            for (var x = 0; x < this.Width; x++)
            {
                for (var y = 1; y < this.Height; y++)
                {
                    this.SpreadFire(y * this.Width + x);
                }
            }
        }

        private void SpreadFire(int src)
        {
            var to = this.FirePixels[src];

            if (to == 0)
            {
                this.FirePixels[src - Width] = 0;
                return;
            }

            var rand = (random.Next() * 3) & 3;

            to = (src - Width) + 1 - rand;
            if (to < 0)
                to = 0;

            this.FirePixels[to] = this.FirePixels[src] - (rand & 1);
        }

        private void DrawImage()
        {
            this.Frame.Lock();

            for (var h = 0; h < this.Height; h++)
            {
                for (var w = 0; w < this.Width; w++)
                {
                    var color = this.FirePixels[h * Width + w];

                    IntPtr backbuffer = this.Frame.BackBuffer;
                    backbuffer += h * this.Frame.BackBufferStride;
                    backbuffer += w * 4;
                    System.Runtime.InteropServices.Marshal.WriteInt32(backbuffer, this.FireColors[color].ToArgb());

                }
            }

            this.Frame.AddDirtyRect(new Int32Rect(0,0, this.Width,this.Height));
            this.Frame.Unlock();
        }

        public WriteableBitmap RenderFrame()
        {
            this.DoFire();
            this.DrawImage();


            this.FramesRendered++;
            this.TotalFramesRendered++;
            
            OnFrameRendered();

            if ((DateTime.Now - _lastTime).TotalSeconds >= 1)
            {
                // one second has elapsed 
                FPS = this.FramesRendered;
                this.FramesRendered = 0;
                _lastTime = DateTime.Now;
            }
            
            return this.Frame;
        }
    }
}
