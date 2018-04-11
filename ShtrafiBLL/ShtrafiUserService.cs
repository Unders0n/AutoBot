using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Model;
using Model.Entities;
using Model.Entities.Fines;

namespace ShtrafiBLL
{
    public class ShtrafiUserService : IShtrafiUserService
    {
        //todo: abstract messenger
        private const string MESSENGER = "tlgrm";
        private const string MYDOMAIN = "shtraf.pro-log.ru";

        private readonly AutoBotContext _autoBotContext;

        public ShtrafiUserService(AutoBotContext _autoBotContext)
        {
            this._autoBotContext = _autoBotContext;
        }




        [Obsolete]
        public User RegisterUserAfterFirstPay(string UserIdTelegramm, string payName, string paySurname, string sts,
            string vu = "")
        {
            try
            {
                var usr = new User();
                //usr.IdWithDomain = UserIdTelegramm
                usr.UserIdTelegramm = UserIdTelegramm;
                usr.PayName = payName;
                usr.PaySurname = paySurname;
                usr.RegistrationDate = DateTime.Now.Date;

                usr.DocumentSetsTocheck = new List<DocumentSetToCheck>
                {
                    new DocumentSetToCheck
                    {
                        User = usr,
                        Name = "основной",
                        ScheduleCheck = true,
                        Sts = sts,
                        Vu = vu,
                        ScheduleLastTimeOfCheck = null
                    }
                };

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

        public User GetUserAndRegisterIfNeeded(string userIdTelegramm, string mainConversationReferenceSerialized)
        {
            try
            {
                var userInDb = GetUserByMessengerId(userIdTelegramm);
                if (userInDb != null) return userInDb;

                var usr = new User();
                //usr.IdWithDomain = UserIdTelegramm
                usr.UserIdTelegramm = userIdTelegramm;
                usr.RegistrationDate = DateTime.Now;
                usr.MainConversationReferenceSerialized = mainConversationReferenceSerialized;

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

        public DocumentSetToCheck RegisterDocumentSetToCheck(User user, string sts, string vu, string name)
        {
            DocumentSetToCheck doc;

            //find user in dbContext 
          
            var userInContext = GetUserById(user.Id);

            if (GetDocumentSetToCheck(userInContext, sts) == null)
            {
                doc = new DocumentSetToCheck
                {
                    User = userInContext,
                    Sts = sts,
                    Vu = vu,
                    Name = name,
                    ScheduleCheck = true
                };

                userInContext.DocumentSetsTocheck.Add(doc);

                _autoBotContext.SaveChanges();
                return doc;
            }

            return null;
            // new Exception("Подписка для этого пользователя с данным");
        }

        public void DisableScheduleForDocumentSet(User user, string sts)
        {
            using (_autoBotContext)
            {
                var set = GetDocumentSetToCheck(user, sts);
                set.ScheduleCheck = false;
                _autoBotContext.SaveChanges();
            }
        }

        public void EnableScheduleForDocumentSet(User user, string sts)
        {
            using (_autoBotContext)
            {
                var set = GetDocumentSetToCheck(user, sts);
                set.ScheduleCheck = true;
                _autoBotContext.SaveChanges();
            }
        }

        public DocumentSetToCheck GetDocumentSetToCheck(User user, string sts)
        {
            var userInContext = GetUserById(user.Id);

            return userInContext.DocumentSetsTocheck.FirstOrDefault(check => check.Sts == sts);
        }

        public User GetUserByMessengerId(string UserIdTelegramm)
        {
            return _autoBotContext.Users.Include(user => user.DocumentSetsTocheck)
                .FirstOrDefault(user => user.UserIdTelegramm == UserIdTelegramm);
        }

        public User GetUserById(int id)
        {
            return _autoBotContext.Users.Include(user => user.DocumentSetsTocheck)
                .FirstOrDefault(user => user.Id == id);
        }

        public void ToggleDocumentSetForSubscription(DocumentSetToCheck documentSetToCheck, bool SubscriptionToggle)
        {
            var docSetInDb = _autoBotContext.DocumentSetsTocheck.Find(documentSetToCheck.Id);
            if (docSetInDb != null) docSetInDb.ScheduleCheck = SubscriptionToggle;
            _autoBotContext.SaveChanges();
        }

      
    }
}