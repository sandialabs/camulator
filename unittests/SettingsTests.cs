using commonlib;
using System.IO;
using Xunit;

namespace unittests
{
    public class SettingsTests
    {
        [Fact]
        public void ShouldLoadSimpleFileProperly()
        {
            Settings settings = Settings.LoadFile("test-simple-settings.json");

            Assert.NotNull(settings);
            Assert.Equal(6000, settings.StartingPort);
            Assert.NotNull(settings.Streams);
            Assert.Equal(2, settings.Streams.Count);
            Assert.Equal("2023-Hoverfly_500-Night.mp4", settings.Streams[0].File);
            Assert.Null(settings.Streams[0].Port);
            Assert.Null(settings.Streams[0].Text);

            Assert.Equal("It's been a hard day's night", settings.Streams[1].Text);
            Assert.Null(settings.Streams[1].File);
            Assert.Null(settings.Streams[1].Port);
        }

        [Fact]
        public void ShouldLoadComplexFileProperly()
        {
            Settings settings = Settings.LoadFile("test-complex-settings.json");

            Assert.NotNull(settings);
            Assert.Equal(6000, settings.StartingPort);
            Assert.NotNull(settings.Streams);
            Assert.Equal(32, settings.Streams.Count);

            Assert.Equal("2023-Hoverfly_500-Night.mp4", settings.Streams[0].File);
            Assert.Null(settings.Streams[0].Port);
            Assert.Null(settings.Streams[0].Text);

            Assert.Equal("Opgal_Visible_250_transition-MinZoom.mp4", settings.Streams[31].File);
            Assert.Null(settings.Streams[31].Port);
            Assert.Null(settings.Streams[31].Text);

            Assert.Equal("Testing\n{{datetime}}", settings.Streams[1].Text);
            Assert.Equal(7000, settings.Streams[1].Port);
            Assert.Null(settings.Streams[1].File);

            RTSP rtsp = settings.Streams.Find(s => s.File == "OPGAL_IR_Transition.mp4");
            Assert.NotNull(rtsp);
            Assert.NotNull(rtsp.Port);
            Assert.Equal(223344, rtsp.Port);
        }

        [Fact]
        public void ShouldHandleBogusFileProperly()
        {
            Settings settings = Settings.LoadFile(string.Empty);

            Assert.Null(settings);

            settings = Settings.LoadFile("nonexistent.json");

            Assert.Null(settings);
        }
    }
}
