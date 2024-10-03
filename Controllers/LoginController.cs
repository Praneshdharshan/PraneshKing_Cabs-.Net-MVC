using Newtonsoft.Json;
using PraneshKing_Cabs.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace PraneshKing_Cabs.Controllers
{
    public class LoginController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // GET: Login
        public ActionResult LoginPage()
        {
            ViewBag.LogoutMessage = TempData["LogoutMessage"] as string;
            Session.Abandon();
            Session.Clear();
            return View();
        }

      
        [HttpPost]
        public ActionResult Login(Login user)
        {
            try
            {
                Login userLogin = new Login();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = $"SELECT d.UserName, d.[Password], r.RoleName FROM UserDetails d " +
                                   $"INNER JOIN LoginRole r ON d.RoleID = r.RoleID " +
                                   $"WHERE d.UserName=@UserName AND d.[Password]=@Password";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        userLogin.UserName = reader["UserName"].ToString();
                        userLogin.RoleName = reader["RoleName"].ToString();
                        Session["UserName"] = userLogin.UserName;
                        Session["RoleName"] = userLogin.RoleName;
                        return Json(new { success = true, RoleName = userLogin.RoleName });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid username or password." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult LogOut()
        {
            Session.Abandon();
            Session.Clear();
            TempData["LogoutMessage"] = "You have been logged out successfully.";
            return RedirectToAction("LoginPage");
        }
        public ActionResult Registeration()
        {
            return View();
        }

        public ActionResult InsertUserDetails(Login user, string securityCode)
        {
            try
            {
                int noOfRowsAffected = 0;

                if (user.RoleID == 1) // Check if the role is Admin
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();

                        // Query to check if the security code exists and is active
                        string securityCodeQuery = "SELECT COUNT(*) FROM SecurityCodes WHERE Code = @SecurityCode AND IsActive = 1";
                        SqlCommand securityCodeCmd = new SqlCommand(securityCodeQuery, con);
                        securityCodeCmd.Parameters.AddWithValue("@SecurityCode", securityCode);

                        int securityCodeCount = (int)securityCodeCmd.ExecuteScalar();

                        if (securityCodeCount == 0)
                        {
                            return Json(new { success = false, message = "Invalid security code." });
                        }
                    }
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = "INSERT INTO UserDetails(UserName, [Password], RoleID) VALUES(@UserName, @Password, @RoleID)";
                    SqlCommand com = new SqlCommand(query, con);
                    com.Parameters.AddWithValue("@UserName", user.UserName);
                    com.Parameters.AddWithValue("@Password", user.Password);
                    com.Parameters.AddWithValue("@RoleID", user.RoleID);

                    noOfRowsAffected = com.ExecuteNonQuery();
                    con.Close();
                }

                return Json(new { success = true, noOfRowsAffected });
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CheckUsernameExists(string UserName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM UserDetails WHERE UserName = @UserName";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@UserName", UserName);
                    int count = (int)cmd.ExecuteScalar();
                    return Json(new { exists = count > 0 });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

    }
}