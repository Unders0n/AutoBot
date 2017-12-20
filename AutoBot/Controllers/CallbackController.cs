using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using NLog;
using StepApp.CommonExtensions.Logger;

namespace AutoBot.Controllers
{
    public class CallbackController : ApiController
    {
        private LoggerService<ILogger> _loggerService;

        public CallbackController()
        {
            _loggerService = new LoggerService<ILogger>();
        }

        // GET api/<controller>
        public async Task<IEnumerable<string>> Get()
        {
            return new[] {"value1", "value2"};
        }


        public async Task<IHttpActionResult> Post(HttpRequestMessage request)
        {
            Stream strea = new MemoryStream();
            var str = await request.Content.ReadAsStringAsync();
            _loggerService.Info($"NEW CALLBACK CAME: {str} \r\n ------- ");
            await request.Content.CopyToAsync(strea);
            StreamReader reader = new StreamReader(strea);
            String res = reader.ReadToEnd();
            return Ok();
        }
    }
}
