using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities.Fines
{
    //some summaries to log events
    public class FinesLog
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
