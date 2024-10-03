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
    public class CustomerController : Controller
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        // GET: Customer
        public ActionResult CustomerList(string searchName, int page = 1, int pageSize = 5)
        {
            ViewBag.SearchName = searchName;

            List<Customers> customers = new List<Customers>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Customers WHERE IsActive = 1", con);
                    int totalRecords = (int)countCmd.ExecuteScalar();

                    int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                    if (page < 1)
                    {
                        page = 1;
                    }
                    else if (page > totalPages)
                    {
                        page = totalPages;
                    }
                    string query = "SELECT CustomerID,CustomerName,Gender,DOB,PhoneNumber,Address,City,State,Pincode from Customers WHERE IsActive = 1";
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        query += " AND CustomerName LIKE '%' + @SearchName + '%'";
                    }
                    query += " ORDER BY CustomerID DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
                    SqlCommand cmd = new SqlCommand(query, con);

                    if (!string.IsNullOrEmpty(searchName))
                    {
                        cmd.Parameters.AddWithValue("@SearchName", searchName);
                    }

                    cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        Customers customer = new Customers
                        {
                            CustomerID = Convert.ToInt32(rdr["CustomerID"]),
                            CustomerName = rdr["CustomerName"].ToString(),
                            Gender= rdr["Gender"].ToString(),
                            DOB = Convert.ToDateTime(rdr["DOB"]),
                            PhoneNumber = rdr["PhoneNumber"].ToString(),
                            Address = rdr["Address"].ToString(),
                            City = rdr["City"].ToString(),
                            State = rdr["State"].ToString(),
                            Pincode = rdr["Pincode"].ToString(),
                        };
                        customers.Add(customer);
                    }
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalPages = totalPages;

                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, such as logging or displaying an error message.
                ViewBag.ErrorMessage = "Error occurred while retrieving employee data: " + ex.Message;
            }
            return View(customers);
        }

        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Customers SET IsActive = 0 WHERE CustomerID = @CustomerID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CustomerID", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return Json(new { success = false, message = ex.Message });
            }
        }


        public ActionResult InsertCustomer(Customers cus)
        {
            try
            {
                int noOfRowsAffected = 0;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Customers (CustomerName, Gender, DOB, PhoneNumber, Address, City, State, Pincode) " +
                                   "VALUES(@CustomerName, @Gender, @DOB, @PhoneNumber, @Address, @City, @State, @Pincode)";

                    SqlCommand com = new SqlCommand(query, con);
                    com.Parameters.AddWithValue("@CustomerName", cus.CustomerName);
                    com.Parameters.AddWithValue("@Gender", cus.Gender);
                    com.Parameters.AddWithValue("@DOB", cus.DOB);
                    com.Parameters.AddWithValue("@PhoneNumber", cus.PhoneNumber);
                    com.Parameters.AddWithValue("@Address", cus.Address);
                    com.Parameters.AddWithValue("@City", cus.City);
                    com.Parameters.AddWithValue("@State", cus.State);
                    com.Parameters.AddWithValue("@Pincode", cus.Pincode);

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


        public ActionResult getCustomerById(Customers cust)
        {
            Customers customer = new Customers();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = $"SELECT CustomerID,CustomerName,Gender,DOB,PhoneNumber,Address,City,State,Pincode from Customers WHERE CustomerID = {cust.CustomerID}";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    customer.CustomerID = Convert.ToInt32(rdr["CustomerID"]);
                    customer.CustomerName = rdr["CustomerName"].ToString();
                    customer.Gender = rdr["Gender"].ToString();
                    customer.DOB = Convert.ToDateTime(rdr["DOB"]);
                    customer.PhoneNumber = rdr["PhoneNumber"].ToString();
                    customer.Address = rdr["Address"].ToString();
                    customer.City = rdr["City"].ToString();
                    customer.State = rdr["State"].ToString();
                    customer.Pincode = rdr["Pincode"].ToString();
                }

                con.Close();
            }

            return Json(customer);
        }


        public ActionResult Add()
        {
            return View();
        }

        public ActionResult update()
        {
            return View();
        }
        public ActionResult UpdateCustomer(Customers cust)
        {
            try
            {
                int noOfRowsAffected = 0;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Customers SET CustomerName = @CustomerName, Gender = @Gender, DOB = @DOB, PhoneNumber = @PhoneNumber," +
                        " Address = @Address, City =  @City, State = @State," +
                        " Pincode = @Pincode  " +
                                    "WHERE CustomerID = @CustomerID";
                    SqlCommand com = new SqlCommand(query, con);

                    com.Parameters.AddWithValue("@CustomerID", cust.CustomerID);
                    com.Parameters.AddWithValue("@CustomerName", cust.CustomerName);
                    com.Parameters.AddWithValue("@Gender", cust.Gender);
                    com.Parameters.AddWithValue("@DOB", cust.DOB);
                    com.Parameters.AddWithValue("@PhoneNumber", cust.PhoneNumber);
                    com.Parameters.AddWithValue("@Address", cust.Address);
                    com.Parameters.AddWithValue("@City", cust.City);
                    com.Parameters.AddWithValue("@State", cust.State);
                    com.Parameters.AddWithValue("@Pincode", cust.Pincode);

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

    }
}