using LibVLCSharp.Shared;
using loglib;
using System;
using System.Net;

namespace videolib
{
    public class RTSPReceiver : IDisposable
    {
        public MediaPlayer Player
        { get { return _player; } }

        public EVideoPlayerState State { get; private set; }

        private MediaPlayer _player;
        private Media _media;
        protected FilePlayer.OnStateChange _stateChange;

        public RTSPReceiver(IPAddress ipAddress, int port)
        {
            _player = new MediaPlayer(FilePlayer._libVLC)
            {
                EnableHardwareDecoding = true,
            };
            _media = new Media(FilePlayer._libVLC, new System.Uri($"rtsp://{ipAddress}:{port}/"));

            _player.Playing += (_, __) => ChangeState(EVideoPlayerState.Playing);
            _player.Stopped += (_, __) => ChangeState(EVideoPlayerState.Stopped);
            _player.MediaChanged += (_, __) => ChangeState(EVideoPlayerState.MediaChanged);
            _player.EndReached += (_, __) => ChangeState(EVideoPlayerState.EndReached);
        }

        public bool Play()
        {
            return _player.Play(_media);
        }

        public void Stop()
        {
            _player.Stop();
        }

        public void Subscribe(FilePlayer.OnStateChange state)
        {
            _stateChange += state;
        }

        private void ChangeState(EVideoPlayerState state)
        {
            Logger.Default().Debug($"VLCReceiver.ChangeState: {State} --> {state}");

            State = state;
            _stateChange?.Invoke(State);
        }

        public void Dispose()
        {
            _media?.Dispose();
            _player?.Dispose();

            _media = null;
            _player = null;
        }
    }
}
