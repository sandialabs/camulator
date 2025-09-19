using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace loglib
{
    public abstract class Logger : IDisposable
    {
        public enum Types
        {
            Trace,
            Console,
            File,
        }

        public enum Levels
        {
            None,
            Error,
            Info,
            Debug,

            NUM_LEVELS,
        }

        private List<string> _entries = new List<string>();

        private static Types _defaultType;
        private static Levels _defaultLevel;
        private static Logger _instance;

        private static string[] _levelStrings = new string[] { "NONE", "ERROR", "INFO", "DEBUG" };

        public static Logger Default()
        {
            if (_instance == null)
            {
                switch (DefaultType)
                {
                    case Types.Trace:
                    default:
                        _instance = new TraceLogger(DefaultLevel);
                        break;

                    case Types.Console:
                        _instance = new ConsoleLogger(DefaultLevel);
                        break;

                    case Types.File:
                        _instance = new FileLogger("camsim.log", DefaultLevel);
                        break;
                }
            }

            return _instance;
        }

        public static Types DefaultType
        {
            get { return _defaultType; }
            set { _defaultType = value; _instance?.Dispose(); _instance = null; }
        }

        public static Levels DefaultLevel
        {
            get { return _defaultLevel; }
            set { _defaultLevel = value; _instance?.Dispose(); _instance = null; }
        }

        static Logger()
        {
            DefaultType = Types.Trace;
            DefaultLevel = Levels.Info;
        }

        public Levels Level { get; set; }

        public Logger(Levels level)
        {
            Level = level;
        }

        public Logger Info(string message)
        {
            if (Level >= Levels.Info)
            {
                string entry = Format(message, Levels.Info);
                lock (_entries)
                    _entries.Add(entry);
                Flush(false);
            }
            return this;
        }

        public Logger Debug(string message)
        {
            if (Level >= Levels.Debug)
            {
                string entry = Format(message, Levels.Debug);
                lock (_entries)
                    _entries.Add(entry);
                Flush(false);
            }
            return this;
        }

        public Logger Error(string message)
        {
            if (Level >= Levels.Error)
            {
                string entry = Format(message, Levels.Error);
                lock (_entries)
                    _entries.Add(entry);
                Flush(false);
            }
            return this;
        }

        public Logger Error(Exception ex)
        {
            if (Level >= Levels.Error)
            {
                // Make sure all the entries for this exception are together in the log
                lock (_entries)
                {
                    string indent = "";
                    while (ex != null)
                    {
                        string entry = indent + Format(ex.Message, Levels.Error);
                        _entries.Add(entry);
                        ex = ex.InnerException;
                        indent += "    ";
                    }
                }
                Flush(false);
            }
            return this;
        }

        public void Dispose()
        {
            Flush(true);
        }

        public abstract Logger Flush(bool force = true);

        protected List<string> GetEntries(bool force)
        {
            List<string> entries = new List<string>();

            lock (_entries)
            {
                if (force || _entries.Count >= 10)
                {
                    entries.AddRange(_entries);
                    _entries.Clear();
                }
            }
            return entries;
        }

        private string Format(string message, Levels level)
        {
            string levelString = (level >= Levels.None && level < Levels.NUM_LEVELS) ? _levelStrings[(int)level] : string.Empty;

            return $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} {levelString}: {message}";
        }
    }

    public class TraceLogger : Logger
    {
        public TraceLogger(Levels level) : base(level)
        {
        }

        public override Logger Flush(bool force = true)
        {
            List<string> entries = GetEntries(force);
            entries.ForEach(e => Trace.WriteLine(e));
            return this;
        }
    }

    public class ConsoleLogger : Logger
    {
        public ConsoleLogger(Levels level) : base(level)
        {
        }

        public override Logger Flush(bool force = true)
        {
            List<string> entries = GetEntries(force);
            entries.ForEach(e => Console.WriteLine(e));
            return this;
        }
    }

    public class FileLogger : Logger
    {
        private static object _lock = new object();

        private string _filename;

        public FileLogger(string filename, Levels level) : base(level)
        {
            _filename = filename;
        }

        public override Logger Flush(bool force = true)
        {
            try
            {
                List<string> entries = GetEntries(force);
                if (entries.Count > 0)
                {
                    string filename = GetFilename();
                    lock (_lock)
                        File.AppendAllLines(filename, entries);
                }
            }
            catch (Exception)
            {
            }
            return this;
        }

        private string GetFilename()
        {
            string rawFilename = Path.GetFileNameWithoutExtension(_filename);
            string suffix = DateTime.Now.ToString("yyyy-MM-dd");
            string extension = Path.GetExtension(_filename);
            string filename = $"{rawFilename}-{suffix}{extension}";

            Trace.WriteLine($"Writing to logfile {filename}");

            return filename;
        }
    }
}
