using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PraneshKing_Cabs.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public decimal CategoryCost { get; set; }
        public bool IsActive { get; set; }

    }
}