using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    [Serializable]
    class User
    {
        public string IdInChannel { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
