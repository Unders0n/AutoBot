using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Model;
using Model.Entities;
using Newtonsoft.Json;
using RestSharp;
using ShrafiBiz.Client;
using ShrafiBiz.Model;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var userTelegrammId = "TestUser";

            var clientId = "R413393879901";
            var key = "dfS3s4Gfadgf9";
            var operType = 1;
            var hash = "7f710ee37c3ff2e3587e1e1acff60ed5";



            Console.WriteLine("starting to connect...");
            var client = new RestSharp.RestClient(" https://www.elpas.ru/api.php");
            
            Console.WriteLine("Приветствуем вас в сервисе оплаты штрафов. У нас можно платить штрафы гибдд с низкой комиссией (10%, мин 30р). Оплата производится на надёжном сайте-партнере (moneta.ru), вы не вводите данные карт в чат.");
            Console.WriteLine("Пожалуйста введите номер свидетельства о регистрации ТС");
            var sts = Console.ReadLine();
            sts = "1621390860";

            //todo: add validation
            Console.WriteLine("Вы можете также ввести номер водительского удостоверения, это повысит вероятность поиска штрафов. Либо просто отправьте 0");
            var vu = Console.ReadLine();
            if (vu == "0") vu = null;

            var req = new RestSharp.RestRequest();
            req.AddParameter("top", "1");
            req.AddParameter("id", "R413393879901");
            req.AddParameter("hash", "7f710ee37c3ff2e3587e1e1acff60ed5");
            req.AddParameter("type", "10");
            req.AddParameter("sts", sts);
           if (vu != null) req.AddParameter("vu", vu);

            // req.Parameters.Add(new Parameter(){Name = });
            
           var resp = client.Post(req);
           var cont = resp.Content;

           var pays =  JsonConvert.DeserializeObject<CheckPayResponse>(cont);
            //todo: add check on -1 error and 500 and repeat call if needed

            if (pays?.Err == -4)
            {
                Console.WriteLine("Штрафы не найдены. Обратите внимание что это не гарантирует на 100% их отсутсвие. Рекомендуем повторить проверку через какое-то время");
                Console.ReadLine();
            }

            var payCount = pays?.L.Count;
            if (payCount != 0)
            {
                if (payCount == 1)
                {
                    Console.WriteLine("У вас найден штраф:");
                }
                else
                {
                    Console.WriteLine($"У вас найдено {payCount} штрафов:");
                }
                int i = 1;
                foreach (var pay in pays.L)
                {
                    Console.WriteLine(i + ": " + pay.Value);
                    i++;
                }
            }

            Console.WriteLine("Перечислите номера штрафов или введите \"все\" чтобы оплатить все штрафы.");
            if (Console.ReadLine().Contains("все"))
            {
                var zkzReq = new RestSharp.RestRequest();
                zkzReq.AddParameter("top", "2");
                zkzReq.AddParameter("id", "R413393879901");
                zkzReq.AddParameter("hash", "7116af7911c223750ce58d22948f7fd8");
                zkzReq.AddParameter("type", "10");
                zkzReq.AddParameter("sts", sts);
                zkzReq.AddParameter("vu", vu);

                //начисления
                var paysJson = JsonConvert.SerializeObject(pays.L);
                zkzReq.AddParameter("l", paysJson);

                zkzReq.AddParameter("name1", "Степанов");
                zkzReq.AddParameter("name2", "Андрей");
                zkzReq.AddParameter("email", "vzzlom@gmail.com");

                zkzReq.AddParameter("flmon", "1");
                zkzReq.AddParameter("flnonotice", "1");


                var zkzResp = client.Post(zkzReq);
                var zkzCont = zkzResp.Content;
                var zkz = JsonConvert.DeserializeObject<CreateZakazResponse>(zkzCont);

                if (zkzResp != null)
                {
                    Console.WriteLine($"Для оплаты перейдите по ссылке: {zkz.Urlpay}");
                }

                //register and check
                var bll = new ShtrafiBLL.ShtrafiUserService(new AutoBotContext());
                var usr = bll.GetUserByMessengerId(userTelegrammId);
                User registeredUser = new User();
                if (usr == null) registeredUser = bll.RegisterUserAfterFirstPay(userTelegrammId, "имя", "фамилия", sts, "");
                Console.WriteLine($"User registered, id is {registeredUser.Id}");

                Console.WriteLine("нажмите любую кнопку чтоб отменить подписку");

                bll.ToggleDocumentSetForSubscription(registeredUser.DocumentSetsTocheck.FirstOrDefault(), false);

                Console.ReadLine();

                /* var cont = resp.Content;
 
                 var pays = JsonConvert.DeserializeObject<CheckPayResponse>(cont);*/
            }

            Console.ReadLine();
        }
    }
}
