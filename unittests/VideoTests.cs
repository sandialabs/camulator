using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using videolib;
using Xunit;

namespace unittests
{
    /// <summary>
    /// Tests playing videos and receiving streamed videos
    ///
    /// The researchpark.mp4 video that is used is exactly 2 seconds long.
    /// </summary>
    public class VideoTests
    {
        [Fact]
        public void ShouldPlay()
        {
            using (FileMedia media = new FileMedia(IPAddress.Loopback, 3737, "researchpark.mp4"))
            using (FilePlayer player = new FilePlayer())
            {
                Assert.NotNull(media.Duration);

                bool playSuccess = player.SetMedia(media);

                Assert.True(playSuccess);
            }
        }

        [Fact]
        public void ShouldFailToPlay()
        {
            using (FileMedia media = new FileMedia(IPAddress.Loopback, 3737, "bogus.mp4"))
            using (FilePlayer player = new FilePlayer())
            {
                Assert.Null(media.Duration);

                bool playSuccess = player.SetMedia(media);

                Assert.False(playSuccess);
            }
        }

        [Fact]
        public void ShouldPlayFor2Seconds()
        {
            using (FileMedia media = new FileMedia(IPAddress.Loopback, 4141, "researchpark.mp4"))
            using (FilePlayer player = new FilePlayer())
            {
                ManualResetEvent e2 = new ManualResetEvent(false);

                // Have to use a StringBuilder, or something similar, because the state changes
                // coming back into the Subscribe are from a different thread and don't
                // get displayed via Trace. So we append them to the StringBuilder and do a single
                // Trace.WriteLine at the end in the test's thread.
                StringBuilder sb = new StringBuilder();

                Stopwatch sw = Stopwatch.StartNew();
                long stoppingElapsed = 0;
                player.Subscribe(s =>
                {
                    switch (s)
                    {
                        case EVideoPlayerState.Initializing:
                            sb.AppendLine($"Initializing took {sw.ElapsedMilliseconds} ms");
                            break;

                        case EVideoPlayerState.Paused:
                            break;

                        case EVideoPlayerState.Stopped:
                            sb.AppendLine($"Stopped took {sw.ElapsedMilliseconds} ms");
                            break;

                        case EVideoPlayerState.Playing:
                            sb.AppendLine($"Playing took {sw.ElapsedMilliseconds} ms");
                            break;

                        case EVideoPlayerState.EndReached:
                            stoppingElapsed = sw.ElapsedMilliseconds;
                            sb.AppendLine($"EndReached took {stoppingElapsed} ms");
                            e2.Set();
                            break;
                    }
                });

                // WaitOne returns true if the event gets set; it returns false
                // if the timeout occurred.

                bool success = player.SetMedia(media);
                bool waitStatus = e2.WaitOne(TimeSpan.FromSeconds(3));
                long elapsedMS = sw.ElapsedMilliseconds;

                sb.AppendLine($"ShouldPlayFor2Seconds: stopping {stoppingElapsed} ms, {elapsedMS} ms");

                Trace.WriteLine(sb.ToString());

                Assert.True(success);
                Assert.True(waitStatus);

                // Turns out the elapsed time, when the EndReached state is reached, is typically less than the
                // actual video time. The researchpark.mp4 video is exactly 2 seconds long, but we get the
                // EndReached state call sometimes as soon as 900 ms after the video starts. Apparently, the VLC
                // library periodically checks its state and it 'knows' the video will stop before the next
                // time it checks the state, so it sends EndReached out a bit early.

                Assert.True(elapsedMS > 800);
                Assert.True(elapsedMS < 3000);
            }
        }

        [Fact]
        public void ShouldPlayFor4Seconds()
        {
            using (FileMedia media = new FileMedia(IPAddress.Loopback, 3741, "researchpark.mp4", 2))
            using (FilePlayer player = new FilePlayer())
            {
                ManualResetEvent e = new ManualResetEvent(false);
                player.Subscribe(s =>
                {
                    if (s == EVideoPlayerState.EndReached)
                        e.Set();
                });

                Stopwatch sw = Stopwatch.StartNew();
                bool success = player.SetMedia(media);

                // WaitOne will return true if the event was set; it will
                // return false if it timed out.
                bool waitStatus = e.WaitOne(TimeSpan.FromSeconds(5));
                long elapsedMS = sw.ElapsedMilliseconds;

                Trace.WriteLine($"ShouldPlayFor4Seconds: {elapsedMS} ms");

                Assert.True(success);
                Assert.True(waitStatus);

                // See the comment in ShouldPlayFor2Seconds about the durations. But note
                // that here we roughly double the duration than in ShouldPlayFor2Seconds
                // since we are repeating.

                Assert.True(elapsedMS > 800 * 2);
                Assert.True(elapsedMS < (2000 * 2) + 1000);
            }
        }

        //[Fact]
        //public void ShouldPlayFor6Seconds()
        //{
        //    using (VLCMedia media = new VLCMedia(IPAddress.Loopback, 3741, "researchpark.mp4"))
        //    using (VLCPlayer player = new VLCPlayer())
        //    {
        //        ManualResetEvent e = new ManualResetEvent(false);
        //        player.Subscribe(s =>
        //        {
        //            if (s == EVideoPlayerState.EndReached)
        //                e.Set();
        //        });

        //        Stopwatch sw = Stopwatch.StartNew();
        //        bool success = player.Play(media);

        //        // WaitOne will return true if the event was set; it will
        //        // return false if it timed out.
        //        bool waitStatus = e.WaitOne(TimeSpan.FromSeconds(7));
        //        long elapsedMS = sw.ElapsedMilliseconds;

        //        Trace.WriteLine($"ShouldPlayFor6Seconds: {elapsedMS} ms");

        //        Assert.True(success);
        //        Assert.True(waitStatus);

        //        // See the comment in ShouldPlayFor2Seconds about the durations. But note
        //        // that here we roughly triple the duration than in ShouldPlayFor2Seconds
        //        // since we are repeating twice.

        //        Assert.True(elapsedMS > 800 * 3);
        //        Assert.True(elapsedMS < 2000 * 3);
        //    }
        //}

        [Fact]
        public void ShouldBeRestartable()
        {
            using (FileMedia media = new FileMedia(IPAddress.Loopback, 4137, "researchpark.mp4", 1))
            using (FilePlayer player = new FilePlayer())
            {
                ManualResetEvent e = new ManualResetEvent(false);
                player.Subscribe(s =>
                {
                    // We want to make sure it can be restarted when we get to the EndReached
                    // state. But we can't do it within the callback since that comes
                    // from a different thread. So we'll just set the event here, and
                    // start it again after the first play has ended.
                    if (s == EVideoPlayerState.EndReached)
                        e.Set();
                });

                Stopwatch sw = Stopwatch.StartNew();
                bool success = player.SetMedia(media);

                // WaitOne will return true if the event was set; it will
                // return false if it timed out.
                bool waitStatus = e.WaitOne(TimeSpan.FromSeconds(3));
                long elapsedMS = sw.ElapsedMilliseconds;

                Trace.WriteLine($"ShouldBeRestartable [A]: {elapsedMS} ms");

                Assert.True(success);
                Assert.True(waitStatus);

                // Now restart it
                sw.Restart();
                e.Reset();
                success = player.SetMedia(media);

                waitStatus = e.WaitOne(TimeSpan.FromSeconds(3));

                Trace.WriteLine($"ShouldBeRestartable [B]: {elapsedMS} ms");

                Assert.True(success);
                Assert.True(waitStatus);
            }
        }

        [Fact]
        public void ShouldGetProperDuration()
        {
            using (FileMedia media = new FileMedia(IPAddress.Loopback, 4137, "researchpark.mp4"))
            {
                Assert.NotNull(media.Duration);

                TimeSpan duration = media.Duration.Value;

                Assert.True(duration.TotalSeconds >= 1.8f);
                Assert.True(duration.TotalSeconds <= 2.2f);
            }
        }

        [Fact]
        public void ShouldReconnectToStream()
        {
            int port = 3741;
            bool completed = false;
            int receiverRestartCount = 0;
            StringBuilder sb = new StringBuilder();

            using (FileMedia media = new FileMedia(IPAddress.Loopback, port, "researchpark.mp4", 1))
            using (FilePlayer player = new FilePlayer())
            {
                bool restartStreamer = false;
                bool restartReceiver = false;

                player.Subscribe((state) =>
                {
                    // When the stream ends, keep restarting it until we're done.
                    if ((state == EVideoPlayerState.Stopped || state == EVideoPlayerState.EndReached) && restartStreamer == false)
                    {
                        sb.AppendLine($"Streamer stopped -- restarting: {state}");
                        restartStreamer = true;
                    }
                });

                using (RTSPReceiver receiver = new RTSPReceiver(IPAddress.Loopback, port))
                {
                    receiver.Subscribe((state2) =>
                    {
                        if ((state2 == EVideoPlayerState.Stopped || state2 == EVideoPlayerState.EndReached) && restartReceiver == false)
                        {
                            sb.AppendLine($"Receiver stopped -- restarting: {state2}");
                            restartReceiver = true;
                        }
                    });

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    player.SetMedia(media);
                    receiver.Play();

                    while (!completed)
                    {
                        if (restartStreamer)
                        {
                            restartStreamer = false;
                            player.SetMedia(media);
                        }

                        if (restartReceiver)
                        {
                            restartReceiver = false;
                            if (++receiverRestartCount > 2)
                                completed = true;
                            sb.AppendLine($"Receiver Play {receiverRestartCount}");
                            receiver.Play();
                        }

                        Thread.Sleep(100);

                        if (stopwatch.Elapsed.TotalSeconds > 8)
                            break;
                    }
                }
            }

            Trace.Write(sb.ToString());

            Assert.True(completed);
            Assert.True(receiverRestartCount == 3);
        }
    }
}
