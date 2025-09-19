using System.CommandLine;
using System.Net;
using commonlib;
using System;
using camsim;
using loglib;
using Microsoft.Owin.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {
        Logger.DefaultType = Logger.Types.Console;
        Logger.DefaultLevel = Logger.Levels.Info;
        int? apiListenPort = null;

        IPAddress localIP = IPAddressExtensions.GetLocalIPAddress();

        if (localIP == null)
        {
            Console.WriteLine("Unable to determine IP address");
            return;
        }

        RootCommand rootCommand = new RootCommand();

        Option<string> settingsFile = new Option<string>("--settings", () => ".\\settings.json",
            "The settings file to use. If not specified, settings.json in the local folder is used.");
        rootCommand.Add(settingsFile);

        Option<bool> verboseOption = new Option<bool>("--verbose",
            "To output more detail");
        rootCommand.Add(verboseOption);

        Option<bool> apiPort = new Option<bool>(name: "--apiport",
            description: "Enable the API, listening on port 8080");
        rootCommand.Add(apiPort);

        rootCommand.SetHandler((settingsFilename, port, verbose) =>
        {
            if (string.IsNullOrEmpty(settingsFilename) == false)
                GlobalVariables.Manager.LoadFile(settingsFilename);

            if(port)
                apiListenPort = 8080;

            if (verbose)
                Logger.Default().Level = Logger.Levels.Debug;
        },
        settingsFile,
        apiPort,
        verboseOption);

        rootCommand.Invoke(args);

        // Start the API listening, if that's enabled
        IDisposable app = null;
        if(apiListenPort != null && apiListenPort.Value > 0)
        {
            string addr = $"http://+:{apiListenPort.Value}/";

            Logger.Default().Info($"Enabling API: {addr}").Flush();

            app = WebApp.Start<Startup>(addr);
        }

        StreamingManager streaming = new StreamingManager(GlobalVariables.Manager, localIP);

        if (streaming.Start())
        {
            Console.WriteLine($"Press any key to stop");
            Console.ReadKey();

            Logger.Default().Info("Shutting down").Flush(true);

            streaming.Stop();

            Logger.Default().Flush(true);
        }

        app?.Dispose();
        Logger.Default().Dispose();
    }
}
