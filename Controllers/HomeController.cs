using PraneshKing_Cabs.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PraneshKing_Cabs.Controllers
{
    [Authorization]
    public class HomeController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        
        public ActionResult Index()
        {
            var stats = GetTripStatus();
            var currentTime = DateTime.Now;
            var greeting = GetGreeting(currentTime);
            var currentDateTime = currentTime.ToString("MM/dd/yyyy hh:mm tt");

            ViewBag.Greeting = CapitalizeFirstLetter(Session["UserName"]?.ToString());
            ViewBag.CurrentDateTime = currentDateTime;
            ViewBag.TotalTrip = stats.TotalTrips;
            ViewBag.SuccessTrip = stats.SuccessTrip;
            ViewBag.UnSuccessTrip = stats.UnSuccessTrip;

            return View();
        }

        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
        private Trip GetTripStatus()
        {
            var stats = new Trip();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) as TotalTrips FROM Trip WHERE IsActive = 1; " +
                               "SELECT COUNT(*) as SuccessTrip FROM Trip WHERE Amount > 0 and IsActive = 1; " +
                               "SELECT COUNT(*) as UnSuccessTrip FROM Trip WHERE Amount = 0 and IsActive = 1;";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.TotalTrips = reader.GetInt32(0);
                        }
                        if (reader.NextResult() && reader.Read())
                        {
                            stats.SuccessTrip = reader.GetInt32(0);
                        }
                        if (reader.NextResult() && reader.Read())
                        {
                            stats.UnSuccessTrip = reader.GetInt32(0);
                        }
                    }
                }
            }

            return stats;
        }

        private string GetGreeting(DateTime currentTime)
        {
            if (currentTime.Hour < 12)
            {
                return "Good morning";
            }
            else if (currentTime.Hour < 16)
            {
                return "Good afternoon";
            }
            else if (currentTime.Hour < 19)
            {
                return "Good evening";
            }
            else
            {
                return "Good Night";
            }
        }


    }
}