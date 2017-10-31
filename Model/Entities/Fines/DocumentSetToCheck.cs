using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities.Fines
{
    public class DocumentSetToCheck
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public bool ScheduleCheck { get; set; }

        public DateTime ScheduleLastTimeOfCheck { get; set; }
        //certificate of car registration
        public string Sts { get; set; }
        //driver licence
        public string Vu { get; set; }

        public User User { get; set; }

        
    }
}
