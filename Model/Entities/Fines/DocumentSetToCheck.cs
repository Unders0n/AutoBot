using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities.Fines
{
    [Serializable]
    public class DocumentSetToCheck
    {
        [Key]
        public int Id { get; set; }

        //name of a set for example "mom's car"
        public string Name { get; set; }

        public bool ScheduleCheck { get; set; }

        public DateTime? ScheduleLastTimeOfCheck { get; set; }
        //certificate of car registration
        public string Sts { get; set; }
        //driver licence
        public string Vu { get; set; }

        public User User { get; set; }

        public override string ToString()
        {
            var txtVu = Vu != "" ? $", Водительское: {Vu}" : "";
            var txt =  $"Подписка на набор документов: Свидетельство: {Sts} {txtVu}";
            return txt;
        }
    }
}
