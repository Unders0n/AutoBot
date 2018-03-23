using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    [Serializable]
    class CarInGarage
    {
        public CarInGarage Car { get; set; }
        public int MileageLastEntered { get; set; }
        public string PlateNumber { get; set; }
        public string Vin { get; set; }
    }
}
