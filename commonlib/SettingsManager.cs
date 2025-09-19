using loglib;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;

namespace commonlib
{
    public class SettingsManager
    {
        public Settings TheSettings { get; private set; }
        public FileInfo TheFile { get; private set; }

        public enum EChanges
        {
            RTSPAdded,
            RTSPRemoved,
            SettingsChanged
        }

        public delegate void SettingsChanged(EChanges change, RTSP rtsp);
        public event SettingsChanged OnChanges;

        public SettingsManager()
        {
        }

        public bool AddRTSP(RTSP rtsp)
        {
            try
            {
                TheSettings.Streams.Add(rtsp);
                // If the port coming in isn't a good one, fix it up
                TheSettings.AssignPorts();
                SaveFile();

                OnChanges?.Invoke(EChanges.RTSPAdded, rtsp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public bool AddFileStream(string filename, int? port)
        {
            try
            {
                RTSP rtsp = new RTSP(filename, true, port, 30);
                FileInfo fi = rtsp.GetFileInfo(TheSettings.BaseDirectory);
                if(fi != null && fi.Exists)
                {
                    TheSettings.Streams.Add(rtsp);
                    SaveFile();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public bool AddClock(int? port)
        {
            try
            {
                RTSP rtsp = new RTSP(port);
                TheSettings.Streams.Add(rtsp);
                SaveFile();

                OnChanges?.Invoke(EChanges.RTSPAdded, rtsp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        // Go through each RTSP object and see if the file exists. If it doesn't,
        // attempt to figure out where it resides.
        // Uses the path to the settings file so it can check relative paths.
        private void FixStreams()
        {
            TheSettings.Streams?.ForEach(stream =>
            {
                // First, see if the stream's file is fully qualified.
                FileInfo fi = stream.GetFileInfo(null);

                if(fi == null)
                {
                    // Nope, now see if it exists in the specified BaseDirectory
                    fi = stream.GetFileInfo(TheSettings.BaseDirectory);
                    if (fi != null)
                        stream.Directory = TheSettings.BaseDirectory;
                }

                if(fi == null)
                {
                    // Nope, now see if it exists in the same directory as
                    // the settings file.
                    fi = stream.GetFileInfo(TheFile.Directory.FullName);
                    if (fi != null)
                        stream.Directory = TheFile.Directory.FullName;
                }
            });
        }

        //private void FixStream(RTSP stream, string baseDirectory)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(stream.File))
        //            return;

        //        FileInfo streamFile = new FileInfo(stream.File);

        //        // If the file doesn't exist, see if perhaps
        //        // the file exists in the same folder as the
        //        // BaseDirectory folder.
        //        if (streamFile.Exists == false && string.IsNullOrEmpty(baseDirectory) == false)
        //        {
        //            streamFile = new FileInfo(Path.Combine(baseDirectory, stream.File));
        //            if (streamFile.Exists)
        //                stream.File = streamFile.FullName;
        //        }

        //        // If the file still doesn't exist, see if perhaps
        //        // it exists in the same folder as the
        //        // settings file itself.
        //        if(streamFile.Exists == false)
        //        {
        //            streamFile = new FileInfo(Path.Combine(TheFile.Directory.FullName, stream.File));
        //            if (streamFile.Exists)
        //                stream.File = streamFile.FullName;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Default().Error(ex);
        //    }
        //}

        public void LoadFile(string filename)
        {
            TheFile = new FileInfo(filename);
            TheSettings = Settings.LoadFile(TheFile.FullName);

            if (TheSettings == null)
                throw new Exception($"SettingsManager.LoadFile: Unable to load settings from {filename}");

            FixStreams();
            TheSettings.AssignPorts();
            //SaveFile();
        }

        public bool SaveFile()
        {
            try
            {
                string str = JsonConvert.SerializeObject(TheSettings, Formatting.Indented);

                // Make a backup of the existing file before saving
                string filename = Path.GetFileNameWithoutExtension(TheFile.FullName);
                string newFileName = TheFile.Directory + "\\" + filename + DateTime.Now.ToString("-yyyy-MM-dd-hh-mm-ss") + TheFile.Extension;
                File.Move(TheFile.FullName, newFileName);

                File.WriteAllText(TheFile.FullName, str);

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return false;
        }

        public void ChangeSettings(Settings settings)
        {
            TheSettings = settings;
            SaveFile();

            OnChanges?.Invoke(EChanges.SettingsChanged, null);
        }
    }
}
