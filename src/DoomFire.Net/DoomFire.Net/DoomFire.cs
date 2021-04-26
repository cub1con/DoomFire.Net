using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Color = System.Drawing.Color;

namespace DoomFireNet
{
    public class DoomFire
    {
        public Bitmap Frame { get; private set; }

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

        public event EventHandler<Bitmap> FrameRenderd;

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
            this.FirePixels = new int[Width * Height];

            this.Frame = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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

        public void RenderFramesAtInterval()
        {
            this.RunningSince = DateTime.Now;
            while (true)
            {
                var startTicks = Environment.TickCount;
                RenderFrame();
                var workTicks = Environment.TickCount - startTicks;
                var remainingTicks = MaxTicksPerSecond - workTicks;
                Thread.Sleep(Math.Max(Convert.ToInt32(remainingTicks / 10000), 0));
            }
        }

        public void RenderFrames()
        {
            this.RunningSince = DateTime.Now;
            while (true)
            {
                RenderFrame();
            }
        }

        protected virtual void OnFpsUpdate()
        {
            var handler = FpsUpdated;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnFrameRendered(Bitmap frame)
        {
            Debug.Print($"Rendered Frame {this.TotalFramesRendered}");
            var handler = FrameRenderd;
            handler?.Invoke(this, frame);
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

        private int DrawPixel(int x, int y, int pixel)
        {
            this.Frame.SetPixel(x, y, FireColors[pixel]);
            return pixel;
        }

        public void RenderFrame()
        {
            this.DoFire();

            for (var h = 0; h < this.Height; h++)
            {
                for (var w = 0; w < this.Width; w++)
                {
                    var color = this.FirePixels[h * Width + w];
                    this.DrawPixel(w, h, color);
                }
            }


            this.FramesRendered++;
            this.TotalFramesRendered++;
            
            this.OnFrameRendered(Frame);

            if (!((DateTime.Now - _lastTime).TotalSeconds >= 1)) return;
            // one second has elapsed 
            FPS = this.FramesRendered;
            this.FramesRendered = 0;
            _lastTime = DateTime.Now;
        }
    }
}
