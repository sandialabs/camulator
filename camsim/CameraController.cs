using commonlib;
using System;
using System.Web.Http;

namespace camsim
{
    public class CameraController : ApiController
    {
        public void Post([FromBody] RTSP rtsp)
        {
            Console.WriteLine($"CameraController.Post {rtsp}");

            GlobalVariables.Manager.AddRTSP(rtsp);
        }
    }
}
