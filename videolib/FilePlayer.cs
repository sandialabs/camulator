using LibVLCSharp.Shared;
using loglib;
using System;
using System.Diagnostics;
using System.Threading;

namespace videolib
{
    public class FilePlayer : IPlayable, IDisposable
    {
        // Turns out there should only be one LibVLC created for the
        // entire application:
        // https://github.com/videolan/libvlcsharp/blob/3.x/docs/best_practices.md
        public static LibVLC _libVLC;

        protected MediaPlayer _player;
        protected FileMedia _media;

        protected TimeSpan _currentTime;
        protected ManualResetEvent _playingEvent;
        protected OnStateChange _stateChange;

        public EVideoPlayerState State { get; private set; }

        public TimeSpan? Duration
        { get { return _media?.Duration; } }

        public TimeSpan? CurrentTime
        { get { return _currentTime; } }

        static FilePlayer()
        {
            _libVLC = new LibVLC(enableDebugLogs: false);
            _libVLC.Log += (obj, args) =>
            {
                Logger.Default().Debug("VLCPlayer: " + args.Message);
            };
        }

        public delegate void OnStateChange(EVideoPlayerState state);

        public void Subscribe(OnStateChange state)
        {
            _stateChange = state;
        }

        public FilePlayer()
        {
            _player = new MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true,
            };
            _playingEvent = new ManualResetEvent(false);
            State = EVideoPlayerState.Initializing;

            _player.Playing += (_, __) =>
            {
                _playingEvent.Set();
                ChangeState(EVideoPlayerState.Playing);
            };
            _player.Stopped += (_, __) => ChangeState(EVideoPlayerState.Stopped);
            _player.MediaChanged += (_, __) => ChangeState(EVideoPlayerState.MediaChanged);
            _player.EndReached += (_, __) => ChangeState(EVideoPlayerState.EndReached);
            _player.PositionChanged += (_, posChanged) =>
            {
                if (_media?.Duration != null)
                    _currentTime = TimeSpan.FromSeconds(_media.Duration.Value.TotalSeconds * posChanged.Position);

                //Logger.Default().Debug($"Position changed: {_mediaFilename} => {posChanged.Position} is {_currentTime?.TotalSeconds}");
            };
            _player.EncounteredError += (_, err) =>
            {
                if (_media == null)
                    Logger.Default().Error("Null media");
                else
                    Logger.Default().Error($"EncouteredError: {_media}");
                Logger.Default().Error(err.ToString());
            };
        }

        public bool SetMedia(FileMedia media)
        {
            if (media == null)
            {
                Logger.Default().Error("VLCPlayer.Play: null media");
                return false;
            }
            if (_player == null)
            {
                Logger.Default().Error("VLCPlayer.Play: null player");
                return false;
            }

            _media = media;

            return true;
        }

        public bool Play()
        {
            Logger.Default().Info($"Streaming {_media.Filename} at rtsp://{_media.IPAddr}:{_media.Port}/");

            _playingEvent.Reset();

            Stopwatch sw = Stopwatch.StartNew();

            bool started = _player.Play(_media.TheMedia);

            // WaitOne returns true if the event was signaled; false if it timed out
            bool playing = _playingEvent.WaitOne(3000);

            Logger.Default().Debug($"VLCPlayer.Play: Play {_media} took {sw.ElapsedMilliseconds} ms, started == {started}, playing == {playing}");

            return playing;
        }

        //public void Pause()
        //{
        //    _player?.Pause();
        //    ChangeState(EVideoPlayerState.Paused);
        //}

        public void Stop()
        {
            Logger.Default().Info($"Stopping {_media}").Flush(true);

            Stopwatch sw = Stopwatch.StartNew();

            _player?.Stop();
            ChangeState(EVideoPlayerState.Stopped);

            Logger.Default().Debug($"Stopping {_media} took {sw.ElapsedMilliseconds} ms").Flush(true);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        private void ChangeState(EVideoPlayerState state)
        {
            Logger.Default().Debug($"VLCPlayer.ChangeState: {State} --> {state}");

            State = state;
            _stateChange?.Invoke(State);
        }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                _player?.Dispose();
                _player = null;

                _playingEvent.Dispose();
            }
        }
    }
}
