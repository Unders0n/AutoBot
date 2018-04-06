using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ShrafiBiz.Model;

namespace ShrafiBiz.Client
{
    public class ShtrafBizClient : IShtrafBizClient
    {
        private const string serviceUri = "https://www.elpas.ru/api.php";
        private const string id = "R413393879901";
        private const string hash1 = "7f710ee37c3ff2e3587e1e1acff60ed5";
        private const string hash2 = "7116af7911c223750ce58d22948f7fd8";


        public ShtrafBizClient()
        {
            
        }

        public CheckPayResponse CheckPay(string sts, string vu)
        {
            
                var client = new RestSharp.RestClient(serviceUri);


                var req = new RestSharp.RestRequest();
                req.AddParameter("top", "1");
                req.AddParameter("id", id);
                req.AddParameter("hash", hash1);
                req.AddParameter("type", "10");
                req.AddParameter("sts", sts);
               if (!string.IsNullOrEmpty(vu)) req.AddParameter("vu", vu);

                // req.Parameters.Add(new Parameter(){Name = });

                var resp = client.Post(req);
                var cont = resp.Content;
                //  JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };
                dynamic obj = JsonConvert.DeserializeObject(cont);
                
               //воркераунд потому что payload дегенератский
                //массива с платежами нет, сериализуем во враппер
                if (obj.l is JArray)
                {
                    var paysWrapper = JsonConvert.DeserializeObject<GenericCheckPayResponse>(cont);
                    return new CheckPayResponse(){Err = paysWrapper.Err, Hash = paysWrapper.Hash, L = null, Msg = paysWrapper.Msg, Top = paysWrapper.Top};
                }

             /*   if (!string.IsNullOrEmpty(obj.msg.Value))
                {
                    throw new Exception(obj.msg.Value);
                }
                
               /* var paysWrapper = JsonConvert.DeserializeObject<GenericCheckPayResponse>(cont);
                if (paysWrapper == null)
                {#1#*/
                else
                {
                    var pays = JsonConvert.DeserializeObject<CheckPayResponse>(cont);
                    return pays;
                }
                    
                /*}
                return null;*/


            
           
           
        }

        public CreateZakazResponse CreateZkz(Dictionary<string, Pay> pays, string sts, string vu, string Surname, string Name)
        {
            var client = new RestSharp.RestClient(serviceUri);


            var zkzReq = new RestSharp.RestRequest();
            zkzReq.AddParameter("top", "2");
            zkzReq.AddParameter("id", id);
            zkzReq.AddParameter("hash", hash2);
            zkzReq.AddParameter("type", "10");
            zkzReq.AddParameter("sts", sts);
            zkzReq.AddParameter("vu", vu);

            //начисления
            var paysJson = JsonConvert.SerializeObject(pays);
            zkzReq.AddParameter("l", paysJson);

            zkzReq.AddParameter("name1", Surname);
            zkzReq.AddParameter("name2", Name);
            zkzReq.AddParameter("email", "vzzlom@gmail.com");

            zkzReq.AddParameter("flmon", "1");
            zkzReq.AddParameter("flnonotice", "1");


            var zkzResp = client.Post(zkzReq);
            var zkzCont = zkzResp.Content;
            var zkz = JsonConvert.DeserializeObject<CreateZakazResponse>(zkzCont);
            return zkz;
        }
    }
}
