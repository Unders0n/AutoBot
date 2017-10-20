using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrafiBiz.Model
{
    public class GenericCheckPayResponse
    {

        [JsonProperty("l")]
        public IList<object> L { get; set; }

        [JsonProperty("err")]
        public int Err { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("top")]
        public int Top { get; set; }
    }
}
