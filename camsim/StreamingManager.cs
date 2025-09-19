using commonlib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using videolib;

namespace camsim
{
    public class StreamingManager
    {
        private SettingsManager _settings;
        private IPAddress _localIP;
        private CancellationTokenSource _cancel;
        private List<IPlayable> _players;

        public StreamingManager(SettingsManager settings, IPAddress localIP)
        {
            if (settings == null)
                throw new Exception("StreamingController: null settings");

            _settings = settings;
            _localIP = localIP;

            _settings.OnChanges += OnChanges;
        }

        public bool Start()
        {
            Stop();

            _cancel = new CancellationTokenSource();
            _players = _settings.TheSettings.GetStreams(_localIP, _cancel);

            if (_players == null)
                return false;

            _players.ForEach(player => player.Play());

            return true;
        }

        public bool Stop()
        {
            if (_players == null || _cancel == null)
                return false;

            _cancel.Cancel();
            _players.ForEach(player => player.Stop());

            return true;
        }

        private void OnChanges(SettingsManager.EChanges change, RTSP rtsp)
        {
            switch (change)
            {
                // A single RTSP was added to the settings. Simply start that one
                // and get him in the set of players.
                case SettingsManager.EChanges.RTSPAdded:
                    if(rtsp != null)
                    {
                        IPlayable playable = rtsp.CreateStream(_localIP, _cancel);
                        if(playable != null)
                        {
                            _players.Add(playable);
                            playable.Play();
                        }
                    }
                    break;

                // The entire settings have changed. Just stop and restart.
                case SettingsManager.EChanges.SettingsChanged:
                    Stop();
                    Start();
                    break;
            }
        }
    }
}
