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
    public class CabController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // GET: Cab
        public ActionResult CabList(string searchName, string categoryName, int page = 1, int pageSize = 5)
        {
            var categories = getcategory().ToList();
            var categorySelectList = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryID.ToString(),
                Text = c.CategoryName.ToString()
            }).ToList();
            ViewBag.CategorySelectList = categorySelectList;
            ViewBag.SearchName = searchName;
            ViewBag.SelectedCar = categoryName;
            List<Cab> cabs = new List<Cab>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Cab WHERE IsActive = 1", con);
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
                    string query = "SELECT c.CabID, c.VehicleNumber, c.DriverName, ca.CategoryName FROM Cab c inner join Category ca on c.CategoryID = ca.CategoryID WHERE c.IsActive = 1";
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        query += " AND DriverName LIKE '%' + @SearchName + '%'";
                    }
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        query += " AND c.CategoryID = @CategoryID";
                    }
                    query += " ORDER BY c.CabID DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

                    SqlCommand cmd = new SqlCommand(query, con);
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        cmd.Parameters.AddWithValue("@SearchName", searchName);
                    }
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryName);
                    }

                    cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        Cab cab = new Cab
                        {
                            CabID = Convert.ToInt32(rdr["CabID"]),
                            VehicleNumber = rdr["VehicleNumber"].ToString(),
                            DriverName = rdr["DriverName"].ToString(),
                            CategoryName = rdr["CategoryName"].ToString(),
                        };
                        cabs.Add(cab);
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
            return View(cabs);
        }

        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Cab SET IsActive = 0 WHERE CabID = @CabID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CabID", id);

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

        public ActionResult InsertCab(Cab cabs)
        {
            try
            {
                int noOfRowsAffected = 0;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Cab (VehicleNumber, DriverName, CategoryID) " +
                                   "VALUES(@VehicleNumber, @DriverName, @CategoryID)";

                    SqlCommand com = new SqlCommand(query, con);
                    com.Parameters.AddWithValue("@VehicleNumber", cabs.VehicleNumber);
                    com.Parameters.AddWithValue("@DriverName", cabs.DriverName);
                    com.Parameters.AddWithValue("@CategoryID", cabs.CategoryID);

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


        public ActionResult Add()
        {
            var categories = getcategory().ToList();
            var categorySelectList = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryID.ToString(),
                Text = c.CategoryName.ToString()
            }).ToList();
            ViewBag.CategorySelectList = categorySelectList;
            return View();
        }

        public List<Category> getcategory()
        {
            var categories = new List<Category>();

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT CategoryID, CategoryName from Category ORDER BY CategoryCost";
                var command = new SqlCommand(query, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var category = new Category
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString()
                        };
                        categories.Add(category);
                    }
                }
            }
            return categories;
        }
    }
}