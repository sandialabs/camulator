using commonlib;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Net;
using System.Threading.Tasks;
using videolib;
using System.Threading;
using loglib;
using System.IO;

namespace camsim
{
    public static class Extensions
    {
        public static IPlayable CreateStream(this RTSP rtsp, IPAddress localIP, CancellationTokenSource cancel)
        {
            if (rtsp.IsValid() == false)
                return null;

            IPlayable playable = null;
            FileInfo fi = rtsp.GetFileInfo();

            if (fi != null)
            {
                FilePlayer player = new FilePlayer();
                FileMedia media = new FileMedia(localIP, rtsp.Port.Value, fi.FullName, ushort.MaxValue);
                player.SetMedia(media);
                playable = player;
            }
            else if (string.IsNullOrEmpty(rtsp.Text) == false)
            {
                playable = new TextPlayer(rtsp.Text, localIP, rtsp.Port.Value, rtsp.FPS, cancel);
            }

            return playable;
        }

        public static List<IPlayable> GetStreams(this Settings settings, IPAddress localIP, CancellationTokenSource cancel)
        {
            if (localIP == null || settings.Streams == null)
                return null;

            List<IPlayable> playables = new List<IPlayable>();

            int nextPort = settings.StartingPort ?? -1;

            Stopwatch sw = Stopwatch.StartNew();

            Parallel.ForEach(settings.Streams, v =>
            {
                IPlayable stream = CreateStream(v, localIP, cancel);

                if (stream != null)
                {
                    lock(playables)
                        playables.Add(stream);
                }
            });

            Logger.Default().Debug($"Loading {playables.Count} streams took {sw.Elapsed.TotalSeconds:f2} s");

            return playables;
        }

        //private static FileMedia BuildMedia(this RTSP v, IPAddress localIP, ref int nextPort)
        //{
        //    FileMedia media = null;
        //    int port = -1;

        //    // If a port is specified, use it. Otherwise, look to see if the StartingPoint is
        //    // specified, and if so use it.
        //    if (v.Port.HasValue)
        //        port = v.Port.Value;
        //    else if (nextPort > 0)
        //        port = Interlocked.Increment(ref nextPort) - 1;

        //    if (port > 0)
        //    {
        //        try
        //        {
        //            Logger.Default().Info($"Playing {v.File} on port {port}");
        //            media = new FileMedia(localIP, port, v.File, ushort.MaxValue);
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Default().Error($"Error: could not load media/player for {v.File}");
        //            Logger.Default().Error(e);
        //        }
        //    }
        //    else
        //        Logger.Default().Error($"Error: could not determine port for {v.File}");

        //    return media;
        //}

        public static StreamData FromMediaInfo(this MediaInfo info)
        {
            StreamData sd = new StreamData()
            {
                Filename = info.Filename,
                Size = info.Size,
                Codec = info.Codec,
            };

            if (info.DurationMS.HasValue)
                sd.DurationMS = info.DurationMS.Value;
            if(info.FPS.HasValue)
                sd.FramesPerSecond = (int)info.FPS.Value;
            if(info.Width.HasValue && info.Height.HasValue)
            {
                sd.Width = info.Width.Value;
                sd.Height = info.Height.Value;
            }

            return sd;
        }
    }
}
