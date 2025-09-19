using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace videolib
{
    public class StreamToPipeWriter
    {
        private Stream _stream;
        private PipeWriter _writer;
        private StreamToMemory _stm;

        public StreamToPipeWriter(Stream stream, PipeWriter writer)
        {
            _stream = stream;
            _writer = writer;
            _stm = new StreamToMemory(stream);
        }

        public async Task<(bool isCompleted, bool isCanceled)> Write(CancellationTokenSource cancel)
        {
            long length = _stream.Position;
            Memory<byte> memory = _writer.GetMemory((int)length);

            _stream.Position = 0;

            // Make the frame available to the reader
            int bytesRead = _stm.Copy(ref memory, (int)length);
            _writer.Advance(bytesRead);

            FlushResult flushResult = await _writer.FlushAsync(cancel.Token);

            _stream.SetLength(0);

            return (flushResult.IsCompleted, flushResult.IsCanceled);
        }
    }
}
