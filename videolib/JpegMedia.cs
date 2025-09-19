using LibVLCSharp.Shared;
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
        protected static LibVLC _libVLC = new LibVLC(false, "--demux=mjpeg");

        private Pipe _pipe;
        private PipeMediaInput _mediaInput;
        protected int _fps;

        public PipeWriter Writer
        { get { return _pipe.Writer; } }

        public JpegMedia(IPAddress ipAddr, int port, int fps) : base(ipAddr, port)
        {
            // Sanity check
            int fixedFPS = Math.Max(1, Math.Min(30, fps));

            string[] options = new string[]
            {
                //$":sout=#transcode{{vcodec=h264,fps={fps}}}:rtp{{sdp=rtsp://{ipAddr}:{port}}}",
                $":sout=#transcode{{vcodec=h264,fps={fixedFPS}}}:rtp{{sdp=rtsp://@:{port}}}",
                ":no-sout-all",
                ":sout-keep"
            };

            _pipe = new Pipe();
            _mediaInput = new PipeMediaInput(_pipe.Reader);
            _media = new Media(_libVLC, _mediaInput, options);
            _fps = fps;
        }
    }
}
