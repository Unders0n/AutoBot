using Model.Entities;
using Model.Entities.Fines;

namespace ShtrafiBLL
{
    public interface IShtrafiUserService
    {
        User GetUserByMessengerId(string UserIdTelegramm);
        User RegisterUserAfterFirstPay(string UserIdTelegramm, string payName, string paySurname, string sts, string vu = "");

        User GetUserAndRegisterIfNeeded(string userIdTelegramm, string mainConversationReferenceSerialized);

        DocumentSetToCheck RegisterDocumentSetToCheck(User user, string sts, string vu, string name);

        void DisableScheduleForDocumentSet(User user, string sts);
        void EnableScheduleForDocumentSet(User user, string sts); 

        DocumentSetToCheck GetDocumentSetToCheck(User user, string sts);

       User GetUserById(int id);
        // void CancelSubscription(User user, string stsToCancel);
    }
}