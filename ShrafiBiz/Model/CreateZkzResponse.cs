using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrafiBiz.Model
{
    [Serializable]
    public class CreateZakazResponse
        {

            [JsonProperty("err")]
            public int Err { get; set; }

            [JsonProperty("msg")]
            public string Msg { get; set; }

            [JsonProperty("zn")]
            public string Zn { get; set; }

            [JsonProperty("mkt")]
            public int Mkt { get; set; }

            [JsonProperty("dat")]
            public string Dat { get; set; }

            [JsonProperty("pin")]
            public int Pin { get; set; }

            [JsonProperty("sumpay")]
            public int Sumpay { get; set; }

            [JsonProperty("urlpay")]
            public string Urlpay { get; set; }
        }
    
}
