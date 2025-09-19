using System;
using System.IO;

namespace videolib
{
    // Looks like in .NET Framework 4, there's no built-in way to
    // copy data from a Stream to a Memory<byte>. Perhaps I just
    // missed it.
    // Here's a quick class that does it.
    public class StreamToMemory
    {
        private Stream _stream;
        private byte[] _array;
        private int _arrayLength;

        public StreamToMemory(Stream str)
        {
            _stream = str;

            _arrayLength = 65536;
            _array = new byte[_arrayLength];
        }

        public int Copy(ref Memory<byte> mem, int bytes)
        {
            if (bytes > _arrayLength)
            {
                _arrayLength = bytes + (int)(bytes * 0.1);
                _array = new byte[_arrayLength];
            }

            _stream.Position = 0;
            int bytesRead = _stream.Read(_array, 0, bytes);
            _array.AsMemory(0, bytesRead).CopyTo(mem);

            return bytesRead;
        }
    }
}
