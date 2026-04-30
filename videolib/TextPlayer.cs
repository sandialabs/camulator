using LibVLCSharp.Shared;
using loglib;
using System;
using System.Diagnostics;
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

        public TextPlayer(string text, IPAddress ipAddress, int port, int fps) : base(ipAddress, port, fps)
        {
            _text = text;
            _ts = new TextStream(() => text + $"\n\n{ipAddress}:{port}");
            _mp = new MediaPlayer(_media)
            {
                EnableHardwareDecoding = true
            };
            _cancel = new CancellationTokenSource();
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                _mp?.Dispose();
                _mp = null;

                _ts?.Dispose();
                _ts = null;
            }
            base.Dispose(disposing);
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
                Stopwatch sw = Stopwatch.StartNew();
                long delayMS = 1000 / Math.Max(1, _fps);
                long nextTick = 0;

                _mp.Play();

                while (_cancel.IsCancellationRequested == false)
                {
                    long now = sw.ElapsedMilliseconds;

                    if (now > nextTick)
                    {
                        _ts.Output();
                        (bool isCompleted, bool isCanceled) result = await streamWriter.Write(_cancel.Token);

                        nextTick = now + delayMS;
                    }

                    Thread.Sleep(25);
                }
            }, _cancel.Token);

            try
            {
                _mp.Stop();
            }
            catch (Exception ex)
            {
                Logger.Default().Error(ex);
            }

            return true;
        }

        public void Stop()
        {
            Task waitForTask = _task;

            _cancel.Cancel();

            waitForTask?.Wait(1000);

            _task = null;
        }
    }
}
