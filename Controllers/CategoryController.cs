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
    public class CategoryController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // GET: Category
        public ActionResult CategoryList(string searchName, int page = 1, int pageSize = 4)
        {
            ViewBag.SearchName = searchName;
            List<Category> categories = new List<Category>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Category WHERE IsActive = 1", con);
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
                    string query = "SELECT CategoryID, CategoryName, CategoryCost FROM Category WHERE IsActive = 1";
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        query += " AND CategoryName LIKE '%' + @SearchName + '%'";
                    }
                    query += " ORDER BY CategoryCost OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;;";
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
                        Category category = new Category
                        {
                            CategoryID = Convert.ToInt32(rdr["CategoryID"]),
                            CategoryName = rdr["CategoryName"].ToString(),
                            CategoryCost = Convert.ToDecimal(rdr["CategoryCost"])
                        };
                        categories.Add(category);
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
            return View(categories);
        }


        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Category SET IsActive = 0 WHERE CategoryID = @CategoryID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CategoryID", id);

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


        public ActionResult InsertCategory(Category cat)
        {
            try
            {
                int noOfRowsAffected = 0;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Category (CategoryName, CategoryCost) " +
                                   "VALUES(@CategoryName, @CategoryCost)";

                    SqlCommand com = new SqlCommand(query, con);
                    com.Parameters.AddWithValue("@CategoryName", cat.CategoryName);
                    com.Parameters.AddWithValue("@CategoryCost", cat.CategoryCost);

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

        public ActionResult getCategoryById(Category cat)
        {
            Category category = new Category();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = $"SELECT CategoryID, CategoryName, CategoryCost FROM Category WHERE CategoryID = {cat.CategoryID}";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    category.CategoryID = Convert.ToInt32(rdr["CategoryID"]);
                    category.CategoryName = rdr["CategoryName"].ToString();
                    category.CategoryCost = Convert.ToDecimal(rdr["CategoryCost"]);
                }

                con.Close();
            }

            return Json(category);
        }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Update()
        {
            return View();
        }
        public ActionResult UpdateCategory(Category cat)
        {
            try
            {
                int noOfRowsAffected = 0;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Category SET CategoryName = @CategoryName, CategoryCost = @CategoryCost" +
                                    " WHERE CategoryID = @CategoryID";
                    SqlCommand com = new SqlCommand(query, con);

                    com.Parameters.AddWithValue("@CategoryID", cat.CategoryID);
                    com.Parameters.AddWithValue("@CategoryName", cat.CategoryName);
                    com.Parameters.AddWithValue("@CategoryCost", cat.CategoryCost);

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
