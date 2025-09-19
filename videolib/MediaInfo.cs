using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace videolib
{
    public class MediaInfo
    {
        private FileInfo _fileInfo;
        private Media _media;

        public bool Success { get; set; }
        public string Filename { get{ return _fileInfo.Name; } }
        public long Size { get{ return _fileInfo.Length; } }
        public long? DurationMS { get; set; }
        public double? FPS { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Codec { get; set; }
        public TimeSpan? ParseDuration { get; set; }

        public MediaInfo(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        /// <summary>
        /// Because LibVLCSharp does the parsing in its own thread, and does a callback
        /// once the parse has completed, let's do a Task here so we can easily wait
        /// for the parse to finish.
        /// </summary>
        /// <returns></returns>
        public Task<MediaInfo> ParseMedia()
        {
            _media = new Media(FilePlayer._libVLC, _fileInfo.FullName, FromType.FromPath);

            Task<MediaInfo> success = Task<MediaInfo>.Run(() =>
            {
                ManualResetEvent parseEvent = new ManualResetEvent(false);
                Stopwatch sw = Stopwatch.StartNew();

                // Just creating the Media object doesn't really do anything. To find out
                // how long the video file is, we need to tell VLC to parse it. But it
                // does it asyncronously, and makes a callback once it's done parsing.
                // So we need to hook into the ParsedChanged event, tell VLC to parse it,
                // then note the duration once it's finished parsing.
                _media.ParsedChanged += (_, args) =>
                {
                    if (args.ParsedStatus == MediaParsedStatus.Done)
                    {
                        DurationMS = _media.Duration;

                        MediaTrack[] tracks = _media.Tracks;

                        foreach(MediaTrack track in tracks)
                        {
                            switch (track.TrackType)
                            {
                                case TrackType.Video:
                                    FPS = (double)track.Data.Video.FrameRateNum / (double)track.Data.Video.FrameRateDen;
                                    Width = (int)track.Data.Video.Width;
                                    Height = (int)track.Data.Video.Height;
                                    Codec = track.Codec.Decode();
                                    break;

                                default:
                                    break;
                            }
                        }
                        
                        ParseDuration = sw.Elapsed;

                        Success = true;
                        parseEvent.Set();
                    }
                };
                _media.Parse();

                parseEvent.WaitOne(TimeSpan.FromSeconds(3));

                return this;
            });

            return success;
        }
    }
}
