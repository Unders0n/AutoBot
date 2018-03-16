using System.Collections.Generic;
using ShrafiBiz.Model;

namespace ShrafiBiz.Client
{
    public interface IShtrafBizClient
    {
        CheckPayResponse CheckPay(string sts, string vu);
        CreateZakazResponse CreateZkz(Dictionary<string, Pay> pays, string sts, string vu, string Surname, string Name);
    }
}