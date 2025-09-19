using commonlib;
using Xunit;

namespace unittests
{
    public class RTSPTests
    {
        [Fact]
        public void DefaultRTSPShouldConstructOK()
        {
            RTSP rtsp = new RTSP();

            Assert.NotNull(rtsp);
            Assert.False(rtsp.IsValid());
        }

        [Fact]
        public void RTSPShouldConstructOK()
        {
            string filename = "test-simple-settings.json";
            RTSP rtsp = new RTSP(filename, true, 123, 4);

            Assert.NotNull(rtsp);
            Assert.True(rtsp.IsValid());
            Assert.Equal(filename, rtsp.File);
            Assert.Equal(123, rtsp.Port);
        }

        [Fact]
        public void RTSPShouldProperlyReportIsValid()
        {
            RTSP rtsp = new RTSP();

            Assert.NotNull(rtsp);
            Assert.False(rtsp.IsValid());

            string filename = "test-simple-settings.json";
            rtsp.File = filename;

            // Should still not be valid if we check the port
            Assert.False(rtsp.IsValid());

            // Now let's not check the port
            Assert.True(rtsp.IsValid(false));

            // Now set the port and make sure it's valid
            rtsp.Port = 123;
            Assert.True(rtsp.IsValid());

            // Make sure a negative port is invalid
            rtsp.Port = -234;
            Assert.False(rtsp.IsValid());

            rtsp.Port = 234;

            // Make sure the Clock field is null
            Assert.Null(rtsp.Text);

            // Now set the clock field. It should be false now
            // since we don't want both file and clock to be set
            rtsp.Text = "xyz";
            Assert.False(rtsp.IsValid(true));

            rtsp.File = null;
            Assert.True(rtsp.IsValid(true));
        }
    }
}
