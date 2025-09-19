using System;
using System.Text;

namespace videolib
{
    public static class Extensions
    {
        /// <summary>
        /// The FourCC and codec are unsigned ints, with ASCII characters
        /// in each byte, but 'backwards', which leads to something like "462h" which we would
        /// like to see as "h264".
        /// </summary>
        /// <param name="code">The code we'd like to extract a string from</param>
        /// <returns>A string with the decoded ASCII characters in the correct order</returns>
        public static string Decode(this uint code)
        {
            return Encoding.ASCII.GetString(BitConverter.GetBytes(code));
        }
    }
}
