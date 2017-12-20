using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShrafiBiz.Model
{
    public class PayNotificationCallbackResponse
    {
        [JsonProperty("l")]
        public Dictionary<string, Pay> L { get; set; }

        [JsonProperty("top")]
        public int Top { get; set; }

        [JsonProperty("err")]
        public int Err { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("name1")]
        public string Name1 { get; set; }

        [JsonProperty("name2")]
        public string Name2 { get; set; }

        [JsonProperty("name3")]
        public string Name3 { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}