using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PraneshKing_Cabs.Models
{
    public class Login
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string Password { get; set; }
        public string SecurityCode { get; set; }

    }
}