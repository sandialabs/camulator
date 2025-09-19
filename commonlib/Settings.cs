using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace commonlib
{
    /// <summary>
    /// Used when serializing in the settings file.
    /// Contains an optional starting port which is used
    /// if a port isn't specified in the RTSP object.
    /// </summary>
    public class Settings
    {
        [JsonIgnore]
        public FileInfo JSONFile { get; private set; }

        /// <summary>
        /// The RTSP object may specify which port to use.
        /// If it doesn't, see if a starting port has been
        /// specified and attempt to use it, incrementing as
        /// it goes.
        /// </summary>
        public int? StartingPort { get; set; }

        public List<RTSP> Streams { get; set; }
        public string BaseDirectory { get; set; }

        public Settings()
        {
            StartingPort = null;
            Streams = new List<RTSP>();
            BaseDirectory = null;
        }

        /// <summary>
        /// If a port isn't specified, use the StartingPort to assign one.
        /// If a port is mistakenly re-used, throw an exception.
        /// </summary>
        public void AssignPorts()
        {
            if (StartingPort == null || StartingPort.Value <= 0)
                return;

            // Make sure there are no mistakes or collisions
            Dictionary<int, RTSP> rtspMap = new Dictionary<int, RTSP>();
            int port = StartingPort.Value;

            Streams.ForEach(rtsp =>
            {
                int tentativePort = rtsp.Port ?? port++;
                if (rtspMap.ContainsKey(tentativePort))
                {
                    RTSP existing = rtspMap[tentativePort];
                    throw new Exception($"Settings.AssignPort: invalid Port for {rtsp}, already used in {existing}");
                }
                rtsp.Port = tentativePort;
                rtspMap[tentativePort] = rtsp;
            });
        }

        public bool SaveFile()
        {
            if (JSONFile == null || Streams.Count == 0)
                return false;

            string settings = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(JSONFile.FullName, settings);

            return true;
        }

        public static Settings LoadFile(string filename)
        {
            Settings settings = null;

            try
            {
                FileInfo file = new FileInfo(filename);

                if (file.Exists)
                {
                    string text = File.ReadAllText(filename);
                    settings = JsonConvert.DeserializeObject<Settings>(text);

                    if (settings == null || settings.Streams == null || settings.Streams.Count == 0)
                        throw new Exception($"Invalid {filename}");

                    settings.AssignPorts();

                    settings.JSONFile = file;
                }
                else
                    Console.Error.WriteLine($"Nonexistent {filename}");
            }
            catch (Exception ex)
            {
                settings = null;
                Console.Error.WriteLine(ex.Message);
            }

            return settings;
        }
    }
}
