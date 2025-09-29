![Camulator](camsimweb/public/camulatortitle.png)

---
In support of the Nuclear Smuggling Detection and Deterrence (NSDD) Software and Data Systems (SDS) projects, a camera simulation tool was developed to allow testing of systems that receive numerous RTSP streams.

## Main pieces

The actual Camulator app is a command-line app built in C#. It reads a JSON file specifying which video streams to send out via RTSP over which port. It can either stream a video from a file, or stream user-defined text, such as a clock.

> It uses the VLCSharp library to do the streaming. Recent versions of VLCSharp, such as the .NET Core versions, remove the ability to do RTSP streaming, so the tool is built using a version that works with .NET Framework 4.8.

The second part is the configuration app, which is an Angular-based web app. Install the web app in IIS, and you can then change the Camulator configuration with a GUI.

### Camulator

The command-line options for the Camulator app are:

**`--settings <settings file>`**

Specify the JSON settings file to use. If `--settings` is not used, Camulator will look for a `settings.json` file in the same directory as the executable.

**`--verbose`**

Provide more detail in the console output. Typically used for debugging purposes.

**`--apiport`**

Enable the API that is used to change the Camulator configuration. The API will always open port 8080. Don't provide the `--apiport` option unless you want to use the configuration tool.

### CamSimWeb

The Angular-based UI for changing the settings. It connects to the specified IP address and port, and allow the operator to add & remove streams.

Update the `assets/config.json` file to specify the IP address where the Camulator app is running. The port is an option in the `config.json` file, but the Camulator app always listens on port 8080. The Camulator app must be running with the `--apiport` option for the UI to work.

### settings.json

This is the JSON file that Camulator reads to determine what gets streamed over what port. Here's a simple example:

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
    ]
}
```

In the UI, when a new camera is added, it will automatically assign a port based on the value specified in `StartingPort`. If you want to edit the file manually, you don't need to use that.

The `Streams` section indicates what is streamed over which port. It is an array of objects that indicate what gets streamed:
- `File` specifies which video file will be repeatedly streamed over the specified `Port`. Set it to `null` when using the `Text` option.
- `Text` indicates what text should be streamed over the specified `Port`. Set it to null when using the `File` option.

  - These options will be expanded as the individual frame is generated:
    - `{{datetime}}` is replaced with the current date/time
    - `{{date}}` is replaced with just the date
    - `{{time}}` is replaced with just the time

- `Port` indicates what port the video will stream from. It must be specified.
- `FPS` is how many frames-per-second should be generated when the `Text` option is being used. It is not used when a `File` is being streamed.

### Installer

An installer has been developed using Advanced Installer. It will install both the Camulator app and the CamSimWeb app.

### Releases

| Release | Date | Comment |
| --- | --- | --- |
| [1.0.101](https://github.com/sandialabs/camulator/releases/tag/v.1.0.101) | 2025/09/19 | Initial release |

