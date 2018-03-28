using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Entities.Fines;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entities
{
    [Serializable]
    public class User
    {
        [Key]
        public int Id { get; set; }

        //use like email for external services not to show real email. Usually using id.messenger@mydomain.com
      /* temporarily disable cos doesnt need callbacks
       * [Index(IsUnique = true)]
        public int IdWithDomain { get; set; }*/
        public DateTime RegistrationDate { get; set; }
        public string UserIdTelegramm { get; set; }
        public string UserIdSkype { get; set; }

        public string UserIdFacebook { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Surname { get; set; }

        public string PayName { get; set; }

        public string PaySurname { get; set; }

        public virtual List<DocumentSetToCheck> DocumentSetsTocheck { get; set; }

        /// <summary>
        /// Serialized Microsoft.Bot.Connector.ConversationReference for easy proactive messaging
        /// </summary>
        public string MainConversationReferenceSerialized { get; set; }
    }
}
