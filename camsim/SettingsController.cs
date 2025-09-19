using commonlib;
using Newtonsoft.Json;
using System;
using System.Web.Http;

namespace camsim
{
    public class SettingsController : ApiController
    {
        // GET api/settings
        public Settings Get()
        {
            string json = JsonConvert.SerializeObject(GlobalVariables.Manager.TheSettings);
            Console.WriteLine($"SettingsController.Get {json}");

            return GlobalVariables.Manager.TheSettings;
        }

        public void Post([FromBody] Settings settings)
        {
            Console.WriteLine($"SettingsController.Post {settings}");

            GlobalVariables.Manager.ChangeSettings(settings);
        }
    }
}
