using LibVLCSharp.Shared;
using loglib;
using System;
using System.IO.Pipelines;
using System.Net;

namespace videolib
{
    public class JpegMedia : VLCMedia
    {
        // Typically you only want one LibVLC per application, but in this case
        // I don't think we can. We need to specify the "--demux=mjpeg" option,
        // for this usage, so we need another one.
        protected static LibVLC _libVLCJpeg;

        private Pipe _pipe;
        private PipeMediaInput _mediaInput;
        protected int _fps;

        public PipeWriter Writer
        { get { return _pipe.Writer; } }

        static JpegMedia()
        {
            _libVLCJpeg = new LibVLC(true, "--demux=mjpeg");
            _libVLCJpeg.Log += (obj, args) =>
            {
                Logger.Default().Debug("JpegMedia: " + args.Message);
            };
        }

        public JpegMedia(IPAddress ipAddr, int port, int fps) : base(ipAddr, port)
        {
            // Sanity check
            int fixedFPS = Math.Max(1, Math.Min(30, fps));

            string[] options = new string[]
            {
                //$":sout=#transcode{{vcodec=h264,fps={fps}}}:rtp{{sdp=rtsp://{ipAddr}:{port}}}",
                $":sout=#transcode{{vcodec=h264,fps={fixedFPS}}}:rtp{{sdp=rtsp://@:{port}}}",
                ":no-sout-all",
                //":sout-keep"
            };

            _pipe = new Pipe();
            _mediaInput = new PipeMediaInput(_pipe.Reader);
            _media = new Media(_libVLCJpeg, _mediaInput, options);
            _fps = fps;
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _pipe?.Writer.Complete();
                }
                catch(Exception ex)
                {
                    Logger.Default().Error(ex);
                }

                try
                {
                    _pipe?.Reader.Complete();
                }
                catch(Exception ex)
                {
                    Logger.Default().Error(ex);
                }

                _mediaInput?.Dispose();
                _mediaInput = null;

                _pipe = null;
            }
            base.Dispose(disposing);
        }
    }
}
