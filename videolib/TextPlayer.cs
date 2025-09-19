using LibVLCSharp.Shared;
using loglib;
using System;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace videolib
{
    public class TextPlayer : JpegMedia, IPlayable, IDisposable
    {
        private string _text;
        private TextStream _ts;
        private CancellationTokenSource _cancel;
        private Task _task;
        private MediaPlayer _mp;

        public TextPlayer(string text, IPAddress ipAddress, int port, int fps, CancellationTokenSource cancel) : base(ipAddress, port, fps)
        {
            _text = text;
            _ts = new TextStream(() => text + $"\n\n{ipAddress}:{port}");
            _mp = new MediaPlayer(_media)
            {
                EnableHardwareDecoding = true
            };
            _cancel = cancel;
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);

                Stop();

                _ts?.Dispose();
                _ts = null;
            }
        }

        public override string ToString()
        {
            return "TextMedia";
        }

        public bool Play()
        {
            if (_task != null)
                return true;

            Logger.Default().Info($"Streaming {_text} at rtsp://{_ipAddress}:{_port}/");

            _task = Task.Run(async () =>
            {
                StreamToPipeWriter streamWriter = new StreamToPipeWriter(_ts.JPEGStream, Writer);
                long delayMS = 1000 / _fps;
                DateTime? lastDT = null;

                _mp.Play();

                while (_cancel.IsCancellationRequested == false)
                {
                    DateTime now = DateTime.Now;

                    if (lastDT.HasValue == false || (now - lastDT.Value).TotalMilliseconds > delayMS)
                    {
                        _ts.Output();
                        (bool isCompleted, bool isCanceled) result = await streamWriter.Write(_cancel);

                        lastDT = now;
                    }

                    _cancel.Token.WaitHandle.WaitOne(25);
                }
            }, _cancel.Token);

            return true;
        }

        public void Stop()
        {
            _task = null;
        }
    }
}
