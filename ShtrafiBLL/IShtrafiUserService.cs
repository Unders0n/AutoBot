﻿using Model.Entities;
using Model.Entities.Fines;

namespace ShtrafiBLL
{
    public interface IShtrafiUserService
    {
        User GetUserByMessengerId(string UserIdTelegramm);
        User RegisterUserAfterFirstPay(string UserIdTelegramm, string payName, string paySurname, string sts, string vu = "");

        DocumentSetToCheck RegisterDocumentSetToCheck(User user, string sts, string vu, string name);

        User GetUserById(int id);
        // void CancelSubscription(User user, string stsToCancel);
    }
}