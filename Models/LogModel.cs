using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PraneshKing_Cabs.Models
{
    public class LogModel
    {
            public string ErrorMessage { get; set; }
            public DateTime ErrorDate { get; set; }
        public string Message { get; internal set; }
    }
}