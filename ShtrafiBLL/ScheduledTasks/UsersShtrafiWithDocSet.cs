using System.Collections.Generic;
using Model.Entities;
using Model.Entities.Fines;
using ShrafiBiz.Model;

public class UsersShtrafiWithDocSet
{
    public UsersShtrafiWithDocSet(User user, DocumentSetToCheck documentSetToCheck, Dictionary<string, Pay> shtrafs)
    {
        User = user;
        DocumentSetToCheck = documentSetToCheck;
        Shtrafs = shtrafs;
    }

    public User User { get; set; }
    public DocumentSetToCheck DocumentSetToCheck { get; set; }
    public Dictionary<string, Pay> Shtrafs { get; set; }
}