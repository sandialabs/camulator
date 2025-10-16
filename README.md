![Camulator](camsimweb/public/camulatortitle.png)

---
In support of the Nuclear Smuggling Detection and Deterrence (NSDD) Software and Data Systems (SDS) projects, a camera simulation tool was developed to allow testing of systems that receive numerous RTSP streams.

## Main pieces

The actual Camulator app is a command-line app built in C#. It reads a JSON file specifying which video streams to send out via RTSP over which port. It can either stream a video from a file, or stream user-defined text, such as a clock.

> It uses the VLCSharp library to do the streaming. Recent versions of VLCSharp, such as the .NET Core versions, remove the ability to do RTSP streaming, so the tool is built using a version that works with .NET Framework 4.8.

The second part is the configuration app, which is an Angular-based web app. Install the web app in IIS, and you can then change the Camulator configuration with a GUI.

## Getting started

The simplest way is to run the latest installer. It will install the Camulator app as well as the Configuration web app.

The latest installer is: [1.0.101](https://github.com/sandialabs/camulator/releases/tag/v.1.0.101)

### Camulator

The command-line options for the Camulator app are:

**`--settings <settings file>`**

Specify the JSON settings file to use. If `--settings` is not used, Camulator will look for a `settings.json` file in the same directory as the executable.

**`--verbose`**

Provide more detail in the console output. Typically used for debugging purposes.

**`--apiport`**

Enable the API that is used to change the Camulator configuration. The API will always open port 8080. Don't provide the `--apiport` option unless you want to use the configuration tool. A specific command is done to reduce any attack surfaces.

### CamSimWeb

The Angular-based UI for changing the settings. It connects to the specified IP address and port, and allows the operator to add & remove streams.

Update the `assets/config.json` file to specify the IP address where the Camulator app is running--it defaults to `http://localhost`. The port is an option in the `config.json` file, but the Camulator app always listens on port 8080. The Camulator app must be running with the `--apiport` option for the UI to work.

### settings.json

The JSON file that Camulator reads to determine what gets streamed over what port. Here is a simple example:

```
{
    "StartingPort": 7000,
    "Streams": [
        {
            "File": "abc123.mp4",
            "Text": null,
            "Port": 7001,
            "FPS": null
        },
        {
            "File": null,
            "Text": "Lane 2\n{{datetime}}",
            "Port": 7002,
            "FPS": 10
        }
    ],
    "BaseDirectory": "c:\\camulator\\videos"
}
```

**`StartingPort`**

In the UI, when a new camera is added, it will automatically assign a port based on the value specified in `StartingPort`. If you want to edit the file manually and specify the port used for each stream, you don't need to use that.

**`Streams`**

The `Streams` section indicates what is streamed over which port. It is an array of objects that indicate what gets streamed:
- `File` specifies which video file will be repeatedly streamed over the specified `Port`. Set it to `null` when using the `Text` option.
- `Text` indicates what text should be streamed over the specified `Port`. Set it to null when using the `File` option.

  - These options will be expanded as the individual frame is generated:
    - `{{datetime}}` is replaced with the current date/time
      - ex: 2025/09/15 12:34:56.789
    - `{{date}}` is replaced with just the date
      - ex: 2025/09/15
    - `{{time}}` is replaced with just the time
      - ex: 12:34:56.789

- `Port` indicates what port the video will stream from. It must be specified.
- `FPS` is how many frames-per-second should be generated when the `Text` option is being used. It is not used when a `File` is being streamed.

**`BaseDirectory`**

The `BaseDirectory` setting specifies what directory contains the image files. It is checked only if the full path isn't specified in the `Streams.File` option.

### Development

The Camulator app was developed using Visual Studio 2022.

The CamSimWeb app is developed with Angular 19 and NPM. To build it, enter the CamSimWeb directory, then enter:

`npm install`

followed by

`npm run build` to build the app, or

`npm run serve` to have Angular run the CamSimWeb app within its own web server

Other scripts are listed in the `package.json` file under `scripts`.

### Installation

An installer has been developed using Advanced Installer 16. It will install both the Camulator app and the CamSimWeb app. You'll probably want to run the most recent installer executable; the releases are listed below.

### Releases

| Release | Date | Comment |
| --- | --- | --- |
| [1.0.101](https://github.com/sandialabs/camulator/releases/tag/v.1.0.101) | 2025/09/19 | Initial release |
