using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace camsim
{
    /// <summary>
    /// In newer versions of .net than what we're using, there is a built-in
    /// JsonContent class, but we don't get that luxury. Here's our own.
    /// </summary>
    /// <typeparam name="T">The type of object to convert to a ByteArrayContent
    /// as JSON</typeparam>
    public class AsJsonContent<T>
    {
        private T _t;

        public AsJsonContent(T t)
        {
            _t = t;
        }

        public ByteArrayContent GetContent()
        {
            string content = JsonConvert.SerializeObject(_t);
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            ByteArrayContent byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return byteContent;
        }
    }
}
