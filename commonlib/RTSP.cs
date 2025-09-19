using loglib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace commonlib
{
    /// <summary>
    /// Used when serializing in the configuration file. Simply records
    /// which file to stream over which port, or what text to display over
    /// which port.
    /// </summary>
    public class RTSP
    {
        /// <summary>
        /// The file to stream
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Text to stream.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Which port to stream File over. Can be null to indicate
        /// a port should be assigned.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// When outputting text to the stream, output at this many frames-per-second
        /// </summary>
        public int FPS { get; set; }

        [JsonIgnore]
        public string Directory { get; set; }

        public RTSP()
        {
            File = null;
            Text = null;
            Port = null;
            FPS = 4;
        }

        public RTSP(int? port)
        {
            File = null;
            Text = null;
            Port = port;
            FPS = 4;
        }

        /// <summary>
        /// Create an RTSP with either a filename or the text
        /// to stream.
        /// </summary>
        /// <param name="str">Filename or text. Expands {{timedate}}, {{time}},
        /// or {{date}} as appropriate.</param>
        /// <param name="file">True if str is a filename; false if str is text</param>
        /// <param name="port">The port to stream over. If null, an appropriate
        /// port will be assigned.</param>
        /// <param name="fps">Frames-per-second when doing text streaming</param>
        public RTSP(string str, bool file, int? port, int fps)
        {
            File = file ? str : null;
            Text = file ? null : str;
            Port = port;
            FPS = fps;
        }

        public override string ToString()
        {
            return $"RTSP: File -- '{File}', Text -- '{Text}', Port -- {Port}, FPS -- {FPS}";
        }

        public bool IsValid(bool checkPort = true)
        {
            try
            {
                // Either the file or text should be enabled, but not both

                bool fileIsOK = false;
                if (string.IsNullOrEmpty(File) == false)
                {
                    FileInfo file = GetFileInfo();
                    if(file != null)
                        fileIsOK = file.Exists;
                }

                bool clockIsOK = string.IsNullOrEmpty(Text) == false;
                bool portOK = checkPort ? Port.HasValue && Port.Value > 0 : true;

                // xor for mutual exclusion
                return (fileIsOK ^ clockIsOK) && portOK;
            }
            catch (System.Exception)
            {
            }

            return false;
        }

        public FileInfo GetFileInfo()
        {
            return GetFileInfo(Directory);
        }

        public FileInfo GetFileInfo(string directory)
        {
            FileInfo streamFile = null;

            try
            {
                if (string.IsNullOrEmpty(File))
                    return streamFile;

                FileInfo fi = new FileInfo(File);

                // If the file doesn't exist, see if perhaps
                // the file exists in the directory specified.
                if (fi.Exists == false && string.IsNullOrEmpty(directory) == false)
                {
                    fi = new FileInfo(Path.Combine(directory, File));
                    if (fi.Exists)
                        streamFile = fi;
                }
            }
            catch (Exception ex)
            {
                Logger.Default().Error(ex);
            }

            return streamFile;
        }
    }
}
