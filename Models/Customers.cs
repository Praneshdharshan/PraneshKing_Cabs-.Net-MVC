using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PraneshKing_Cabs.Models
{
    public class Customers
    {
        public int CustomerID { get; set; }

        public string CustomerName { get; set; }

        public string Gender { get; set; }

        public DateTime DOB { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Pincode { get; set; }

        public bool IsActive { get; set; }

    }
}