using System;
using System.Diagnostics;
using System.Drawing;
using System.Timers;
using Color = System.Drawing.Color;

namespace DoomFireNet
{
    public class DoomFire
    {
        public Bitmap BITMAP { get; private set; }

        public event EventHandler FrameRenderd;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Timer Timer { get; private set; }

        public Color[] FIRE_PAL { get; private set; } = new Color[37];
        public int[] FIRE_PIXELS { get; private set; }
        public int[] FIRE_RGB { get; private set; } = {
            0x07, 0x07, 0x07, 0x1F, 0x07, 0x07, 0x2F, 0x0F, 0x07, 0x47, 0x0F, 0x07, 0x57, 0x17, 0x07, 0x67,
            0x1F, 0x07, 0x77, 0x1F, 0x07, 0x8F, 0x27, 0x07, 0x9F, 0x2F, 0x07, 0xAF, 0x3F, 0x07, 0xBF, 0x47,
            0x07, 0xC7, 0x47, 0x07, 0xDF, 0x4F, 0x07, 0xDF, 0x57, 0x07, 0xDF, 0x57, 0x07, 0xD7, 0x5F, 0x07,
            0xD7, 0x5F, 0x07, 0xD7, 0x67, 0x0F, 0xCF, 0x6F, 0x0F, 0xCF, 0x77, 0x0F, 0xCF, 0x7F, 0x0F, 0xCF,
            0x87, 0x17, 0xC7, 0x87, 0x17, 0xC7, 0x8F, 0x17, 0xC7, 0x97, 0x1F, 0xBF, 0x9F, 0x1F, 0xBF, 0x9F,
            0x1F, 0xBF, 0xA7, 0x27, 0xBF, 0xA7, 0x27, 0xBF, 0xAF, 0x2F, 0xB7, 0xAF, 0x2F, 0xB7, 0xB7, 0x2F,
            0xB7, 0xB7, 0x37, 0xCF, 0xCF, 0x6F, 0xDF, 0xDF, 0x9F, 0xEF, 0xEF, 0xC7, 0xFF, 0xFF, 0xFF };



        public DoomFire(int width, int height, int tps)
        {
            this.Height = height;
            this.Width = width;
            this.FIRE_PIXELS = new int[Width * Height];

            this.BITMAP = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            this.Timer = new Timer(1000 / tps);
            this.Timer.Elapsed += _timer_Elapsed;
            this.Timer.AutoReset = true;

            // Fill Color pallete
            for (var i = 0; i < FIRE_PAL.Length; i++)
            {
                FIRE_PAL[i] = Color.FromArgb((byte)FIRE_RGB[i * 3 + 0], (byte)FIRE_RGB[i * 3 + 1], (byte)FIRE_RGB[i * 3 + 2]);
            }

            // Preset Frame
            for (var i = 0; i < Width * Height; i++)
            {
                FIRE_PIXELS[i] = 0;
            }

            // Preset "fire source" in frame
            for (var i = 0; i < Width; i++)
            {
                FIRE_PIXELS[(Height - 1) * Width + i] = 36;
            }

            this.Timer.Enabled = true;
        }

        protected virtual void OnFrameRendered(EventArgs e)
        {
            var handler = FrameRenderd;
            handler?.Invoke(this, e);
        }


        private void DoFire()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 1; y < Height; y++)
                {
                    SpreadFire(y * Width + x);
                }
            }
        }

        private void SpreadFire(int from)
        {
            var to = FIRE_PIXELS[from];

            if (to == 0)
            {
                FIRE_PIXELS[from - Width] = 0;
                return;
            }

            to = from - Width;
            FIRE_PIXELS[to] = FIRE_PIXELS[from] - 1;
        }

        private int DrawPixel(int x, int y, int pixel)
        {
            //if (x >= BITMAP.Height || y >= BITMAP.Width)
            //{
            //    throw new ArgumentOutOfRangeException($"DrawPixel at x:{x} y:{y} while Bitmap is x:{BITMAP.Height} y:{BITMAP.Width}");
            //}

            BITMAP.SetPixel(x, y, FIRE_PAL[pixel]);
            return pixel;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoFire();


            for (var h = 0; h < Height; h++)
            {
                for (var w = 0; w < Width; w++)
                {
                    var color = FIRE_PIXELS[h * Width + w];
                    DrawPixel(w, h, color);
                }
            }

            OnFrameRendered(EventArgs.Empty);
        }
    }
}
