using commonlib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using videolib;

namespace camsim
{
    public class BaseDirectoryController : ApiController
    {
        // GET api/basedirectory
        public List<StreamData> Get(string baseDirectory)
        {
            if(string.IsNullOrEmpty(baseDirectory))
                return new List<StreamData>();

            List<string> files = new List<string>(Directory.EnumerateFiles(baseDirectory));
            List<StreamData> streams = new List<StreamData>();
            Stopwatch sw = Stopwatch.StartNew();

            Parallel.ForEach(files, (file) =>
            {
                FileInfo fi = new FileInfo(file);
                MediaInfo mi = new MediaInfo(fi);
                Task<MediaInfo> task = mi.ParseMedia();
                task.Wait(TimeSpan.FromSeconds(3));

                if(task.IsCompleted)
                {
                    lock (streams)
                        streams.Add(task.Result.FromMediaInfo());
                }
            });

            Trace.WriteLine($"BaseDirectoryController: {baseDirectory} took {sw.ElapsedMilliseconds} ms:");
            streams.ForEach(stream => Trace.WriteLine($"    {stream}"));

            return streams;
        }
    }
}
