using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PraneshKing_Cabs.Models
{
    public class Cab
    {
        public int CabID { get; set; }
        public string VehicleNumber { get; set; }
        public string DriverName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }
}
