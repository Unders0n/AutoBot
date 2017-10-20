using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShrafiBiz.Model
{
    [Serializable]
    public class Exe
    {
        [JsonProperty("payer")]
        public string Payer { get; set; }
    }

    [Serializable]
    public class BANK
    {
        [JsonProperty("BILLDATE")]
        public string BILLDATE { get; set; }

        [JsonProperty("SOINAME")]
        public string SOINAME { get; set; }

        [JsonProperty("INN")]
        public string INN { get; set; }

        [JsonProperty("KPP")]
        public string KPP { get; set; }

        [JsonProperty("ACC")]
        public string ACC { get; set; }

        [JsonProperty("BANKNAME")]
        public string BANKNAME { get; set; }

        [JsonProperty("BIK")]
        public string BIK { get; set; }

        [JsonProperty("PURPOSE")]
        public string PURPOSE { get; set; }

        [JsonProperty("USERNAME")]
        public string USERNAME { get; set; }

        [JsonProperty("KBK")]
        public string KBK { get; set; }

        [JsonProperty("OKTMO")]
        public string OKTMO { get; set; }

        [JsonProperty("SIGN")]
        public string SIGN { get; set; }
    }

   
    [Serializable]
    public class Pay
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sum")]
        public string Sum { get; set; }

        [JsonProperty("feesrv")]
        public int Feesrv { get; set; }

        [JsonProperty("exe")]
        public Exe Exe { get; set; }

        [JsonProperty("BANK")]
        public BANK BANK { get; set; }

        [JsonProperty("PAYERIDENTIFIER")]
        public string PAYERIDENTIFIER { get; set; }

        [JsonProperty("TOTALAMOUNT")]
        public string TOTALAMOUNT { get; set; }

        [JsonProperty("ISPAID")]
        public string ISPAID { get; set; }

        [JsonProperty("DISCOUNTSIZE")]
        public string DISCOUNTSIZE { get; set; }

        [JsonProperty("DISCOUNTDATE")]
        public string DISCOUNTDATE { get; set; }

        [JsonProperty("ARTICLECODE")]
        public string ARTICLECODE { get; set; }

        [JsonProperty("LOCATION")]
        public string LOCATION { get; set; }

        [JsonProperty("dat")]
        public string Dat { get; set; }

        [JsonProperty("mkt")]
        public int Mkt { get; set; }

        [JsonProperty("mktdiscount")]
        public int Mktdiscount { get; set; }

        [JsonProperty("addinfo")]
        public string Addinfo { get; set; }

        public override string ToString()
        {
            return string.Format($"от **{this.Dat}** на **{Sum}р** (+{Feesrv}р комиссия): {this.Addinfo}");
        }
    }

    /* public class L
     {
         [JsonProperty("19001_18810116170930622624")]
         public List<Pay> Pay { get; set; }
     }
 */
   
    [Serializable]
    public class CheckPayResponse
    {
        
        [JsonProperty("l")]
        public Dictionary<string, Pay> L { get; set; }

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