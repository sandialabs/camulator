using LibVLCSharp.Shared;
using loglib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace videolib
{
    public class FileMedia : VLCMedia
    {
        private string _filename;

        public override TimeSpan? Duration
        {
            get
            {
                TimeSpan? dur = null;
                if (_media?.Duration > 0)
                    dur = TimeSpan.FromMilliseconds(_media.Duration);
                return dur;
            }
        }

        public string Filename
        { get { return _filename; } }

        public FileMedia(IPAddress ipAddress, int port, string filename, ushort loopCount = 0) : base(ipAddress, port)
        {
            _filename = filename;

            Stopwatch sw = Stopwatch.StartNew();
            _media = new Media(FilePlayer._libVLC, filename, FromType.FromPath, getMediaStringOptions(loopCount));

            ManualResetEvent parseEvent = new ManualResetEvent(false);

            // Just creating the Media object doesn't really do anything. To find out
            // how long the video file is, we need to tell VLC to parse it. But it
            // does it asyncronously, and makes a callback once it's done parsing.
            // So we need to hook into the ParsedChanged event, tell VLC to parse it,
            // then note the duration once it's finished parsing.
            _media.ParsedChanged += (_, args) =>
            {
                if (args.ParsedStatus == MediaParsedStatus.Done)
                {
                    parseEvent.Set();

                    Logger.Default().Info($"{filename} duration is {Duration?.TotalSeconds} s");
                }
            };
            _media.Parse();

            bool parsed = parseEvent.WaitOne(TimeSpan.FromSeconds(3));

            Logger.Default().Debug($"new Media({filename}) took {sw.ElapsedMilliseconds} ms -- parsed: {parsed}");
        }

        public override string ToString()
        {
            return $"FileMedia: {Filename}";
        }

        /// <summary>
        /// Specify the options to pass to the Media object as it's being constructed.
        /// These options all come from the documentation at
        /// https://wiki.videolan.org/Documentation:Streaming_HowTo/Advanced_Streaming_Using_the_Command_Line/
        /// </summary>
        /// <param name="loopCount">If the count is 0 or 1, the media will play once. If you want it to
        /// repeat, specify a # greater than 1. Set it to ushort.MaxValue to repeat it as many times
        /// as the VLC library allows</param>
        /// <returns>An array of string of options</returns>
        private string[] getMediaStringOptions(ushort loopCount)
        {
            List<string> options = new List<string>
            {
                //$":sout=#rtp{{sdp=rtsp://{_ipAddress}:{_port}}}",
                $":sout=#rtp{{sdp=rtsp://@:{_port}}}",
                ":no-sout-all",
                ":sout-keep",
            };

            if (loopCount > 1)
                options.Add($":input-repeat={loopCount}");

            return options.ToArray();
        }
    }
}
