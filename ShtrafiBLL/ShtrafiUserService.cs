using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Model;
using Model.Entities;
using Model.Entities.Fines;

namespace ShtrafiBLL
{
    public class ShtrafiUserService : IShtrafiUserService
    {
        //todo: abstract messenger
        private const string MESSENGER = "tlgrm";
        private const string  MYDOMAIN = "shtraf.pro-log.ru";

        private readonly AutoBotContext _autoBotContext;

        public ShtrafiUserService(Model.AutoBotContext _autoBotContext)
        {
            this._autoBotContext = _autoBotContext;
        }

        public User RegisterUserAfterFirstPay(string UserIdTelegramm, string payName, string paySurname, string sts, string vu = "")
        {
            try
            {
                var usr = new User();
                //usr.IdWithDomain = UserIdTelegramm
                usr.UserIdTelegramm = UserIdTelegramm;
                usr.PayName = payName;
                usr.PaySurname = paySurname;
                usr.RegistrationDate = DateTime.Now.Date;

                  usr.DocumentSetsTocheck = new List<DocumentSetToCheck> { new DocumentSetToCheck(){User = usr , Name = "основной", ScheduleCheck = true, Sts = sts, Vu = vu, ScheduleLastTimeOfCheck = null} };

                _autoBotContext.Users.Add(usr);
                _autoBotContext.SaveChanges();
                return usr;
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                throw;
            }
            
        }

        public DocumentSetToCheck RegisterDocumentSetToCheck(User user, string sts,  string name, string vu = "")
        {
            DocumentSetToCheck doc;
            //find user in dbContext
            var userInContext = GetUserById(user.Id);

            if (userInContext.DocumentSetsTocheck.FirstOrDefault(check => check.Sts == sts) != null)
            {
                doc = new DocumentSetToCheck()
                {
                    User = user,
                    Sts = sts,
                    Vu = vu,
                    Name = name,
                    ScheduleCheck = true
                };

                userInContext.DocumentSetsTocheck.Add(doc);

                _autoBotContext.SaveChanges();
                return doc;
            }
            else
            {
                return null;
                // new Exception("Подписка для этого пользователя с данным");
            }
        }

        public User GetUserByMessengerId(string UserIdTelegramm)
        {
            return _autoBotContext.Users.Include(user => user.DocumentSetsTocheck).FirstOrDefault(user => user.UserIdTelegramm == UserIdTelegramm);
        }

        public User GetUserById(int id)
        {
            return _autoBotContext.Users.Include(user => user.DocumentSetsTocheck).FirstOrDefault(user => user.Id == id);
        }

        public void ToggleDocumentSetForSubscription(DocumentSetToCheck documentSetToCheck, bool SubscriptionToggle)
        {
            var docSetInDb = _autoBotContext.DocumentSetsTocheck.Find(documentSetToCheck.Id);
            if (docSetInDb != null) docSetInDb.ScheduleCheck = SubscriptionToggle;
            _autoBotContext.SaveChanges();
        }
    }
}
