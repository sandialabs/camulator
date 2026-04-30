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
            EnsureCapacity(bytes);

            _stream.Position = 0;

            int total = 0;
            while (total < bytes)
            {
                int n = _stream.Read(_array, total, bytes - total);

                // EOF or stream changed under us
                if (n == 0)
                    break;
                total += n;
            }

            _array.AsMemory(0, total).CopyTo(mem);

            return total;
        }

        private void EnsureCapacity(int bytes)
        {
            if (bytes <= _arrayLength)
                return;

            _arrayLength = bytes + (bytes / 10);
            _array = new byte[_arrayLength];
        }
    }
}
