using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DoomFireNet
{
    public class DoomFire
    {
        public BitmapPalette Palette { get; private set; }
        public WriteableBitmap Writer { get; private set; }



        public event EventHandler FpsUpdated;
        public event EventHandler<BitmapSource> FrameRenderd;

        public int TotalFramesRendered { get; private set; }
        private int _framesRendered;

        private int _fps;

        public int Fps
        {
            get => _fps;
            private set
            {
                _fps = value;
                OnFpsUpdate();
            }
        }

        private DateTime _lastTime; // marks the beginning the measurement began

        public DateTime RunningSince { get; private set; } = new DateTime();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public long MaxTicksPerSecond { get; private set;}

        private readonly Random _random = new Random();

        private readonly int[] _fireRgb = 
        {
            0x07, 0x07, 0x07, 0x1F, 0x07, 0x07, 0x2F, 0x0F, 0x07, 0x47, 0x0F, 0x07, 0x57, 0x17, 0x07, 0x67,
            0x1F, 0x07, 0x77, 0x1F, 0x07, 0x8F, 0x27, 0x07, 0x9F, 0x2F, 0x07, 0xAF, 0x3F, 0x07, 0xBF, 0x47,
            0x07, 0xC7, 0x47, 0x07, 0xDF, 0x4F, 0x07, 0xDF, 0x57, 0x07, 0xDF, 0x57, 0x07, 0xD7, 0x5F, 0x07,
            0xD7, 0x5F, 0x07, 0xD7, 0x67, 0x0F, 0xCF, 0x6F, 0x0F, 0xCF, 0x77, 0x0F, 0xCF, 0x7F, 0x0F, 0xCF,
            0x87, 0x17, 0xC7, 0x87, 0x17, 0xC7, 0x8F, 0x17, 0xC7, 0x97, 0x1F, 0xBF, 0x9F, 0x1F, 0xBF, 0x9F,
            0x1F, 0xBF, 0xA7, 0x27, 0xBF, 0xA7, 0x27, 0xBF, 0xAF, 0x2F, 0xB7, 0xAF, 0x2F, 0xB7, 0xB7, 0x2F,
            0xB7, 0xB7, 0x37, 0xCF, 0xCF, 0x6F, 0xDF, 0xDF, 0x9F, 0xEF, 0xEF, 0xC7, 0xFF, 0xFF, 0xFF
        };


        public DoomFire(int width, int height, int targetFps)
        {
            this.Height = height;
            this.Width = width;
            this.MaxTicksPerSecond = TimeSpan.TicksPerSecond / targetFps;

            var colors = _fireRgb.Length / 4;
            var e = new System.Windows.Media.Color[colors];

            for (var i = 0; i < colors; i++)
            {
                e[i] = System.Windows.Media.Color.FromRgb((byte) this._fireRgb[i * 3 + 0],
                    (byte) this._fireRgb[i * 3 + 1], (byte) this._fireRgb[i * 3 + 2]);
            }

            this.Palette = new BitmapPalette(e);

            this.Writer = new WriteableBitmap(Width, Height, 96.0, 96.0, System.Windows.Media.PixelFormats.Indexed8, Palette);

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

        protected virtual void OnFrameRendered(BitmapSource frame)
        {
            Debug.Print($"Rendered Frame {this.TotalFramesRendered}");
            var handler = FrameRenderd;
            handler?.Invoke(this, frame);
        }


        private int max_y = 0;

        private bool _initialized = false;

        public unsafe static int GetPixel(IntPtr pBackBuffer, int pitch, int x, int y)
        {
            return *(int*) (pBackBuffer + y * pitch + x);
        }

        public unsafe static void SetPixel(IntPtr pBackBuffer, int pitch, int x, int y, int i)
        { 
            *(int*) (pBackBuffer + y * pitch + x) = i;
        }

        public void RenderFrame()
        {
            try
            {
                // Reserve the back buffer for updates.
                Writer.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = Writer.BackBuffer;
                    var hello = pBackBuffer + 1;

                    for (var x = 0; x < this.Width; x++)
                    {
                        for (var y = 1; y < (this.Height - 1); y++)
                        {
                            if (!_initialized)
                            {
                                SetPixel(pBackBuffer, Writer.BackBufferStride, x, y, y == (this.Height - 2) ? 36 : 0);
                                continue;
                            }
                            var pixel = GetPixel(pBackBuffer, Writer.BackBufferStride, x, y);
                            if(pixel == 0) 
                                SetPixel(pBackBuffer, Writer.BackBufferStride, x, y - 1, 0);
                            else
                            {
                                var rnd = (int)(Math.Round(_random.NextDouble() * 3.0)) & 3;
                                SetPixel(pBackBuffer, Writer.BackBufferStride, x - rnd + 1, y - 1, pixel - (rnd % 2));
                            }
                        }
                    }
                }

                _initialized = true;

                // Specify the area of the bitmap that changed.
                Writer.AddDirtyRect(new Int32Rect(0, max_y, Width, Height - max_y));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                Writer.Unlock();
            }


            this._framesRendered++;
            this.TotalFramesRendered++;
            
            this.OnFrameRendered(Writer);

            if (!((DateTime.Now - this._lastTime).TotalSeconds >= 1)) return;
            // one second has elapsed 
            this.Fps = this._framesRendered;
            this._framesRendered = 0;
            this._lastTime = DateTime.Now;
        }
    }
}
