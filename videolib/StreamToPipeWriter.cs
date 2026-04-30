using loglib;
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

        public async Task<(bool isCompleted, bool isCanceled)> Write(CancellationToken cancel)
        {
            long length = _stream.Length;
            if (length <= 0)
                return (false, false);

            Memory<byte> memory = _writer.GetMemory((int)length);

            _stream.Position = 0;

            // Make the frame available to the reader
            int bytesRead = _stm.Copy(ref memory, (int)length);

            if (bytesRead != (int)length)
            {
                // Don’t feed partial frame into VLC
                _stream.SetLength(0);

                Logger.Default().Error($"StreamToPipeWriter.Write didn't copy everything -- bytesRead: {bytesRead}, length: {length}");

                return (false, false);
            }

            _writer.Advance(bytesRead);
            _stream.SetLength(0);

            ValueTask<FlushResult> flush = _writer.FlushAsync(cancel);
            Task<FlushResult> flushTask = flush.AsTask();

            Task done = await Task.WhenAny(flushTask, Task.Delay(2000, cancel)).ConfigureAwait(false);
            if (done != flushTask)
                throw new TimeoutException("Pipe flush timed out (VLC likely not reading)");

            FlushResult flushResult = await flushTask.ConfigureAwait(false);

            return (flushResult.IsCompleted, flushResult.IsCanceled);
        }
    }
}
