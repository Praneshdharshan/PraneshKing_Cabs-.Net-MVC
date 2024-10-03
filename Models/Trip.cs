using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PraneshKing_Cabs.Models
{
    public class Trip
    {
        public int TripID { get; set; }

        public string roleName { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string DriverName { get; set; }
        public int CabID { get; set; }
        public string VehicleNumber { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public DateTime PickupDate { get; set; }
        public string PickupLocation {get; set; }
        public string DropLocation { get; set; }
        public Decimal Kilometer { get; set; }
        public Decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public Decimal CategoryCost { get; set; }

        public int TotalTrips { get; set; }
        public int SuccessTrip { get; set; }
        public int UnSuccessTrip { get; set; }

    }
}