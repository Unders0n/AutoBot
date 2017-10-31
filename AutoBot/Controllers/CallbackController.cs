using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AutoBot.Controllers
{
    public class CallbackController : ApiController
    {

        // GET api/<controller>
        public async Task<IEnumerable<string>> Get()
        {
           return new[] { "value1", "value2" };
        }
    }
}
