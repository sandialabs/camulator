using LibVLCSharp.Shared;
using System;
using System.Net;

namespace videolib
{
    public abstract class VLCMedia : IDisposable
    {
        protected IPAddress _ipAddress;
        protected int _port;
        protected Media _media;

        public IPAddress IPAddr { get { return _ipAddress; } }
        public int Port { get { return _port; } }

        public Media TheMedia
        { get { return _media; } }

        public virtual TimeSpan? Duration
        { get { return null; } }

        public VLCMedia(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _media.Dispose();
                _media = null;
            }
        }
    }
}
