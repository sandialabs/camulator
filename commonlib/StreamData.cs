using System;

namespace commonlib
{
    public class StreamData
    {
        public string Filename { get; set; }
        public long Size { get; set; }
        public long DurationMS { get; set; }
        public int? FramesPerSecond { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Codec { get; set; }

        public override string ToString()
        {
            return $"StreamData: Filename '{Filename}', Size {Size}, Duration {DurationMS}, FPS ({FramesPerSecond}), WidthxHeight {Width}x{Height}, Codec '{Codec}'";
        }
    }
}
