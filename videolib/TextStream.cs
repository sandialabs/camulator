using loglib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace videolib
{
    public class TextStream : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public MemoryStream JPEGStream
        { get { return _jpegOutputMemoryStream; } }

        private MemoryStream _jpegOutputMemoryStream;
        private Func<string> _func;
        private Bitmap _bitmap;
        private Graphics _graphics;
        private Font _font;
        private StringFormat _strFormat;
        private SolidBrush _brush;
        private Rectangle _padding;

        public TextStream(Func<string> func, int width = 800, int height = 600)
        {
            _jpegOutputMemoryStream = new MemoryStream();

            Width = width;
            Height = height;

            _func = func;

            _bitmap = new Bitmap(Width, Height);
            _graphics = Graphics.FromImage(_bitmap);
            _font = new Font(FontFamily.GenericSansSerif, 48);

            // Center horizontally and vertically
            _strFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            _brush = new SolidBrush(Color.White);
            _padding = new Rectangle(0, 0, Width, Height);
        }

        public void Output()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string str = FixText(_func());

            _graphics.Clear(Color.DarkSlateGray);
            _graphics.DrawString(str, _font, _brush, _padding, _strFormat);

            _bitmap.Save(_jpegOutputMemoryStream, ImageFormat.Jpeg);

            Logger.Default().Debug($"TextStream.Output: {str} took {stopwatch.ElapsedMilliseconds} ms").Flush();
        }

        public override string ToString()
        {
            return $"TextStream: {_func()}";
        }

        public void Dispose()
        {
            _jpegOutputMemoryStream?.Dispose();
            _jpegOutputMemoryStream = null;

            _font?.Dispose();
            _font = null;

            _graphics?.Dispose();
            _graphics = null;

            _bitmap?.Dispose();
            _bitmap = null;
        }

        private static string FixText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            DateTime now = DateTime.Now;

            (string, string)[] replacements =
            {
                ("{{datetime}}", now.ToString("yyyy/MM/dd\nhh:mm:ss.fff")),
                ("{{date}}", now.ToString("yyyy/MM/dd")),
                ("{{time}}", now.ToString("hh:mm:ss.fff"))
            };

            foreach ((string, string) replace in replacements)
                text = text.Replace(replace.Item1, replace.Item2);

            return text;
        }
    }
}
