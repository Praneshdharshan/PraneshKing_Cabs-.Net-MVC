using PraneshKing_Cabs.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using System.Web;
using System.Web.Mvc;
using System.Data;
using OfficeOpenXml;
using System.Globalization;

namespace PraneshKing_Cabs.Controllers
{
    public class TripController : Controller
    {
        // GET: Trip
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [Authorization]
        public ActionResult TripList(string searchName, string categoryName, string sortBy, int page = 1, int pageSize = 5)
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
            ViewBag.SortBy = sortBy;
            ViewBag.PageSize = pageSize; // Pass page size to the view
            List<Trip> trips = new List<Trip>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string countQuery = "SELECT COUNT(*) FROM Trip t " +
                            "INNER JOIN Customers cu ON t.CustomerID = cu.CustomerID " +
                            "INNER JOIN Cab c ON t.CabID = c.CabID " +
                            "INNER JOIN Category ca ON c.CategoryID = ca.CategoryID " +
                            "WHERE t.IsActive = 1";

                    if (!string.IsNullOrEmpty(searchName))
                    {
                        countQuery += " AND cu.CustomerName LIKE '%' + @SearchName + '%'";
                    }
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        countQuery += " AND c.CategoryID = @CategoryID";
                    }

                    SqlCommand countCmd = new SqlCommand(countQuery, con);

                    if (!string.IsNullOrEmpty(searchName))
                    {
                        countCmd.Parameters.AddWithValue("@SearchName", searchName);
                    }
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        countCmd.Parameters.AddWithValue("@CategoryID", categoryName);
                    }

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

                    string orderBy = "t.TripID DESC"; // Default sorting

                    if (sortBy == "CustomerName")
                    {
                        orderBy = "cu.CustomerName";
                    }
                    else if (sortBy == "TripStatus")
                    {
                        orderBy = "CASE WHEN t.Amount = 0 THEN 1 ELSE 0 END, t.TripID DESC"; // Custom sorting for status
                    }

                    string query = "SELECT t.TripID, cu.CustomerName, c.DriverName, c.VehicleNumber, ca.CategoryName, t.PickupDate, t.PickupLocation, t.DropLocation, t.Kilometer, t.Amount FROM Trip t " +
                                   "INNER JOIN Customers cu ON t.CustomerID = cu.CustomerID " +
                                   "INNER JOIN Cab c ON t.CabID = c.CabID " +
                                   "INNER JOIN Category ca ON c.CategoryID = ca.CategoryID " +
                                   "WHERE t.IsActive = 1";

                    if (!string.IsNullOrEmpty(searchName))
                    {
                        query += " AND cu.CustomerName LIKE '%' + @SearchName + '%'";
                    }
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        query += " AND c.CategoryID = @CategoryID";
                    }

                    query += $" ORDER BY {orderBy} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

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
                        Trip trip = new Trip
                        {
                            TripID = Convert.ToInt32(rdr["TripID"]),
                            CustomerName = rdr["CustomerName"].ToString(),
                            DriverName = rdr["DriverName"].ToString(),
                            VehicleNumber = rdr["VehicleNumber"].ToString(),
                            CategoryName = rdr["CategoryName"].ToString(),
                            PickupDate = Convert.ToDateTime(rdr["PickupDate"]),
                            PickupLocation = rdr["PickupLocation"].ToString(),
                            DropLocation = rdr["DropLocation"].ToString(),
                            Kilometer = Convert.ToDecimal(rdr["Kilometer"]),
                            Amount = Convert.ToDecimal(rdr["Amount"])
                        };
                        trips.Add(trip);
                    }
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalPages = totalPages;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, such as logging or displaying an error message.
                ViewBag.ErrorMessage = "Error occurred while retrieving trip data: " + ex.Message;
            }
            return View(trips);
        }

        [Authorization]
        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Trip SET IsActive = 0 WHERE TripID = @TripID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@TripID", id);

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

        public ActionResult InsertTrip(Trip trips)
        {
            try
            {
                int noOfRowsAffected = 0;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Trip (CustomerID, CabID, PickupDate, PickupLocation, DropLocation, Kilometer, Amount) " +
                                   "VALUES(@CustomerID, @CabID, @PickupDate, @PickupLocation , @DropLocation , @kilometer, @Amount )";

                    SqlCommand com = new SqlCommand(query, con);
                    com.Parameters.AddWithValue("@CustomerID", trips.CustomerID);
                    com.Parameters.AddWithValue("@CabID", trips.CabID);
                    com.Parameters.AddWithValue("@PickupDate", trips.PickupDate);
                    com.Parameters.AddWithValue("@PickupLocation", trips.PickupLocation);
                    com.Parameters.AddWithValue("@DropLocation", trips.DropLocation);
                    com.Parameters.AddWithValue("@kilometer", trips.Kilometer);
                    com.Parameters.AddWithValue("@Amount", trips.Amount);

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


        public ActionResult getTripById(Trip tri)
        {
            try
            {
                Trip trip = new Trip();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = $"SELECT t.TripID, cu.CustomerName, c.DriverName, c.VehicleNumber," +
                        $" t.PickupDate, t.PickupLocation, t.DropLocation, t.Kilometer,t.Amount,ca.CategoryCost FROM Trip t " +
                        $"inner join Customers cu on t.CustomerID = cu.CustomerID" +
                        $" inner join Cab c on t.CabID = c.CabID inner join Category ca on" +
                        $" c.CategoryID = ca.CategoryID Where t.TripID = {tri.TripID}";
                    SqlCommand cmd = new SqlCommand(query, con);

                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        tri.TripID = Convert.ToInt32(rdr["TripID"]);
                        tri.CustomerName = rdr["CustomerName"].ToString();
                        tri.DriverName = rdr["DriverName"].ToString();
                        tri.VehicleNumber = rdr["VehicleNumber"].ToString();
                        tri.PickupDate = Convert.ToDateTime(rdr["PickupDate"]);
                        tri.PickupLocation = rdr["PickupLocation"].ToString();
                        tri.DropLocation = rdr["DropLocation"].ToString();
                        tri.Kilometer = Convert.ToDecimal(rdr["Kilometer"]);
                        tri.Amount = Convert.ToDecimal(rdr["Amount"]);
                        tri.CategoryCost = Convert.ToDecimal(rdr["CategoryCost"]);
                    }

                    con.Close();
                }

                return Json(tri);
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return Json(new { success = false, message = ex.Message });
            }
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

        public ActionResult Add()
        {
            var customers = getcustomername().ToList();
            var customernameSelectList = customers.Select(c => new SelectListItem
            {
                Value = c.CustomerID.ToString(),
                Text = c.CustomerName.ToString()
            }).ToList();
            ViewBag.customernameSelectList = customernameSelectList;

            var drivers = getDriver().ToList();
            var driverSelectList = drivers.Select(d => new SelectListItem
            {
                Value = d.CabID.ToString(),
                Text = $"{d.DriverName} ({d.VehicleNumber})"
            }).ToList();
            ViewBag.DriverSelectList = driverSelectList;
            ViewBag.RoleName = Session["RoleName"];
            ViewBag.Greeting = Session["UserName"];

            return View();

        }


        public List<Customers> getcustomername()
        {
            var customers = new List<Customers>();

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT DISTINCT CustomerID, CustomerName FROM Customers";
                var command = new SqlCommand(query, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var customer = new Customers
                        {
                            CustomerID = Convert.ToInt32(reader["CustomerID"]),
                            CustomerName = reader["CustomerName"].ToString()
                        };
                        customers.Add(customer);
                    }
                }
            }
            return customers;
        }

        public List<Cab> getDriver()
        {
            var cabs = new List<Cab>();

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT CabID, DriverName, VehicleNumber FROM Cab;";
                var command = new SqlCommand(query, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cab = new Cab
                        {
                            CabID = Convert.ToInt32(reader["CabID"]),
                            DriverName = reader["DriverName"].ToString(),
                            VehicleNumber = reader["VehicleNumber"].ToString()
                        };
                        cabs.Add(cab);
                    }
                }
            }
            return cabs;
        }
        [Authorization]
        public ActionResult Update()
        {
            return View();
        }

        [Authorization]
        public ActionResult downloadPdf()
        {
            return View();
        }
        public ActionResult UpdateTrip(Trip trips)
        {
            try
            {
                int noOfRowsAffected = 0;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Trip SET Kilometer = @Kilometer, Amount = @Amount" +
                                    " WHERE TripID = @TripID";
                    SqlCommand com = new SqlCommand(query, con);

                    com.Parameters.AddWithValue("@TripID", trips.TripID);
                    com.Parameters.AddWithValue("@Kilometer", trips.Kilometer);
                    com.Parameters.AddWithValue("@Amount", trips.Amount);

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

        [Authorization]
        public ActionResult Download(int id)
        {
            Trip t = GetTripByIdInternal(id);
            if (t == null)
            {
                return HttpNotFound("Trip not found");
            }

            return new Rotativa.ViewAsPdf("downloadPdf", t)
            {
                FileName = $"TripDetails_{id}.pdf" // Optional: Set the file name for the PDF
            };
        }

        public JsonResult PreviewExcel(HttpPostedFileBase file)
        {
            var result = new
            {
                headers = new List<string>(),
                rows = new List<List<string>>(),
                errors = new List<string>()
            };

            string fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension == ".xls" || fileExtension == ".xlsx")
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var workSheet = package.Workbook.Worksheets.First();
                    var start = workSheet.Dimension.Start;
                    var end = workSheet.Dimension.End;

                    // Extract headers
                    for (int col = start.Column; col <= end.Column; col++)
                    {
                        result.headers.Add(workSheet.Cells[start.Row, col].Text);
                    }

                    // Extract rows and validate
                    for (int row = start.Row + 1; row <= end.Row; row++)
                    {
                        var rowData = new List<string>();
                        bool hasError = false;
                        string errorMessage = string.Empty;

                        for (int col = start.Column; col <= end.Column; col++)
                        {
                            string cellText = workSheet.Cells[row, col].Text;

                            // Perform validation based on the column index
                            if (col == 5) // Assuming 5th column is pickup date
                            {
                                if (!DateTime.TryParseExact(cellText, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime pickupDate))
                                {
                                    hasError = true;
                                    errorMessage = $"Invalid date format at row {row}: {cellText} format should be like dd-MM-yyyy";
                                }
                                else if (pickupDate < DateTime.Today || pickupDate.Year > DateTime.Now.Year)
                                {
                                    hasError = true;
                                    errorMessage = $"Invalid pickup date at row {row}: {cellText}. Date should be greater than or equal to today's date and within the current year.";
                                }
                            }

                            if (col == 8) // Assuming 8th column is kilometres
                            {
                                if (!long.TryParse(cellText, out long kilometres) || kilometres != 0)
                                {
                                    hasError = true;
                                    errorMessage = $"Invalid kilometres at row {row}: {cellText}. Only 0 is allowed.";
                                }
                            }

                            if (col == 9) // Assuming 9th column is amount
                            {
                                if (!long.TryParse(cellText, out long amount) || amount != 0)
                                {
                                    hasError = true;
                                    errorMessage = $"Invalid amount at row {row}: {cellText}. Only 0 is allowed.";
                                }
                            }

                            rowData.Add(cellText);
                        }

                        result.rows.Add(rowData);

                        if (hasError)
                        {
                            result.errors.Add(errorMessage);
                        }
                        else
                        {
                            result.errors.Add(string.Empty);
                        }
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
      
        [Authorization]
        public ActionResult ImportFromExcel(HttpPostedFileBase file)
        {
            string fileExtension = Path.GetExtension(file.FileName).ToLower();


            // Check if the file is an Excel file
            if (fileExtension == ".xls" || fileExtension == ".xlsx")
            {
                if (file != null && file.ContentLength > 0)
                {
                    try
                    {
                        var logs = new List<LogModel>();
                        int recordsInserted = 0;
                        int recordsNotInserted = 0;

                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var workSheet = package.Workbook.Worksheets.First();
                            var start = workSheet.Dimension.Start;
                            var end = workSheet.Dimension.End;

                            using (var connection = new SqlConnection(connectionString))
                            {
                                connection.Open();

                                for (int row = start.Row + 1; row <= end.Row; row++) // Skip header row
                                {
                                    var customerName = workSheet.Cells[row, 1].Text;
                                    var driverName = workSheet.Cells[row, 2].Text;
                                    var vehicleNumber = workSheet.Cells[row, 3].Text;
                                    var categoryName = workSheet.Cells[row, 4].Text;
                                    var pickupDateText = workSheet.Cells[row, 5].Text;
                                    var pickupLocation = workSheet.Cells[row, 6].Text;
                                    var dropLocation = workSheet.Cells[row, 7].Text;
                                    var kilometresText = workSheet.Cells[row, 8].Text;
                                    var amountText = workSheet.Cells[row, 9].Text;

                                    // Convert the pickup date
                                    if (!DateTime.TryParseExact(pickupDateText, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime pickupDate))
                                    {
                                        logs.Add(new LogModel
                                        {
                                            ErrorMessage = $"Invalid date format at row {row}: {pickupDateText} format should be like dd-MM-yyyy",
                                            ErrorDate = DateTime.Now
                                        });
                                        recordsNotInserted++;
                                        continue;
                                    }

                                    if (pickupDate < DateTime.Today || pickupDate.Year > DateTime.Now.Year)
                                    {
                                        logs.Add(new LogModel
                                        {
                                            ErrorMessage = $"Invalid pickup date at row {row}: {pickupDateText}. Date should be greater than or equal to today's date and within the current year.",
                                            ErrorDate = DateTime.Now
                                        });
                                        recordsNotInserted++;
                                        continue;
                                    }


                                    // Validate kilometres and amount
                                    if (!long.TryParse(kilometresText, out long kilometres) || kilometres != 0)
                                    {
                                        logs.Add(new LogModel
                                        {
                                            ErrorMessage = $"Invalid kilometres at row {row}: {kilometresText}. Only 0 is allowed.",
                                            ErrorDate = DateTime.Now
                                        });
                                        recordsNotInserted++;
                                        continue;
                                    }

                                    if (!long.TryParse(amountText, out long amount) || amount != 0)
                                    {
                                        logs.Add(new LogModel
                                        {
                                            ErrorMessage = $"Invalid amount at row {row}: {amountText}. Only 0 is allowed.",
                                            ErrorDate = DateTime.Now
                                        });
                                        recordsNotInserted++;
                                        continue;
                                    }

                                    // Find Customer ID
                                    var customerId = GetCustomerId(connection, customerName);
                                    if (customerId == 0)
                                    {
                                        logs.Add(new LogModel
                                        {
                                            ErrorMessage = $"Customer not found at row {row}: {customerName}",
                                            ErrorDate = DateTime.Now
                                        });
                                        recordsNotInserted++;
                                        continue;
                                    }

                                    // Find Cab ID
                                    var cabId = GetCabId(connection, vehicleNumber, driverName, categoryName);
                                    if (cabId == 0)
                                    {
                                        logs.Add(new LogModel
                                        {
                                            ErrorMessage = $"Cab not found at row {row}: {vehicleNumber}, {driverName}, {categoryName}",
                                            ErrorDate = DateTime.Now
                                        });
                                        recordsNotInserted++;
                                        continue;
                                    }

                                    // Validate Data
                                    var tripData = new Trip
                                    {
                                        CustomerID = customerId,
                                        CabID = cabId,
                                        PickupDate = pickupDate,
                                        PickupLocation = pickupLocation,
                                        DropLocation = dropLocation,
                                        Kilometer = kilometres,
                                        Amount = amount
                                    };

                                    if (!ValidateData(tripData))
                                    {
                                        logs.Add(new LogModel
                                        {
                                            ErrorMessage = $"Invalid data at row {row}: {customerName}, {driverName}, {vehicleNumber},{pickupDate}, {categoryName}, {pickupLocation}, {dropLocation}, {kilometres}, {amount}",
                                            ErrorDate = DateTime.Now
                                        });
                                        recordsNotInserted++;
                                        continue;
                                    }


                                    // Check if trip already exists
                                    if (!TripExists(connection, customerId, cabId, pickupDate))
                                    {
                                        // Insert into Trip table
                                        var query = "INSERT INTO Trip (CustomerID, CabID, PickupDate, PickupLocation, DropLocation, Kilometer, Amount)  " +
                                                    "VALUES (@CustomerId, @CabId, @PickupDatetime, @PickupLocation, @DropLocation, @Kilometres, @Amount)";

                                        using (var command = new SqlCommand(query, connection))
                                        {
                                            command.Parameters.AddWithValue("@CustomerId", customerId);
                                            command.Parameters.AddWithValue("@CabId", cabId);
                                            command.Parameters.AddWithValue("@PickupDatetime", pickupDate);
                                            command.Parameters.AddWithValue("@PickupLocation", pickupLocation);
                                            command.Parameters.AddWithValue("@DropLocation", dropLocation);
                                            command.Parameters.AddWithValue("@Kilometres", kilometres);
                                            command.Parameters.AddWithValue("@Amount", amount);

                                            try
                                            {
                                                var rowsAffected = command.ExecuteNonQuery();
                                                if (rowsAffected == 0)
                                                {
                                                    logs.Add(new LogModel
                                                    {
                                                        ErrorMessage = $"Failed to insert row {row}: {customerName}, {driverName}, {vehicleNumber},{pickupDate}, {categoryName},  {pickupLocation}, {dropLocation}, {kilometres}, {amount}",
                                                        ErrorDate = DateTime.Now
                                                    });
                                                    recordsNotInserted++;
                                                }
                                                else
                                                {
                                                    recordsInserted++;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                logs.Add(new LogModel
                                                {
                                                    ErrorMessage = $"SQL Exception at row {row}: {ex.Message}",
                                                    ErrorDate = DateTime.Now
                                                });
                                                recordsNotInserted++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        logs.Add(new LogModel
                                        {
                                            ErrorMessage = $"Duplicate trip found at row {row}: {customerName}, {driverName}, {vehicleNumber},{pickupDate}, {categoryName}, {pickupLocation}, {dropLocation}, {kilometres}, {amount}",
                                            ErrorDate = DateTime.Now
                                        });
                                        recordsNotInserted++;
                                    }
                                }
                            }

                            // Save all accumulated logs to a file after processing
                            SaveLogsToLocalFile(logs);

                            // Return JSON response indicating success
                            return Json(new
                            {
                                success = true,
                                message = $"Data imported successfully. Records inserted: {recordsInserted}, Records not inserted: {recordsNotInserted}.",
                                recordsInserted,
                                recordsNotInserted,
                                redirect = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Return JSON response indicating an error
                        return Json(new
                        {
                            success = false,
                            message = "An error occurred while processing the file: " + ex.Message,
                            recordsInserted = 0,
                            recordsNotInserted = 0,
                            redirect = false
                        });
                    }
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid file type. Please upload an Excel file.",
                    recordsInserted = 0,
                    recordsNotInserted = 0,
                    redirect = false
                });
            }

            // Return JSON response indicating no file uploaded
            return Json(new
            {
                success = false,
                message = "No file uploaded",
                recordsInserted = 0,
                recordsNotInserted = 0,
                redirect = false
            });
        }


        private bool TripExists(SqlConnection connection, int customerId, int cabId, DateTime pickupDate)
        {
            var query = "SELECT COUNT(1) FROM Trip WHERE CustomerID = @CustomerId AND CabID = @CabId AND PickupDate = @PickupDate";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@CabId", cabId);
                command.Parameters.AddWithValue("@PickupDate", pickupDate);

                var count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }



        private int GetCustomerId(SqlConnection connection, string customerName)
        {
            var query = "SELECT CustomerID From Customers Where CustomerName = @customerName";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@customerName", customerName);
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetCabId(SqlConnection connection, string vehicleNumber, string driverName, string categoryName)
        {
            var query = "SELECT C.CabID FROM Cab C join Category ca on c.CategoryID = ca.CategoryID " +
                        "WHERE C.VehicleNumber = @VehicleNumber AND C.DriverName = @DriverName AND ca.CategoryName = @CategoryName";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@VehicleNumber", vehicleNumber);
                command.Parameters.AddWithValue("@DriverName", driverName);
                command.Parameters.AddWithValue("@CategoryName", categoryName);
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private bool ValidateData(Trip data)
        {
            // Check for null or empty values for required string properties
            bool isValid = !string.IsNullOrEmpty(data.PickupLocation) &&
                           !string.IsNullOrEmpty(data.DropLocation);

            // Validate numeric and date properties
            isValid &= data.CustomerID > 0;
            isValid &= data.CabID > 0;
            isValid &= data.PickupDate != DateTime.MinValue; // Ensure valid date
            isValid &= data.Kilometer >= 0; // Ensure non-negative value
            isValid &= data.Amount >= 0;

            return isValid;
        }

        private void SaveLogsToLocalFile(List<LogModel> logs)
        {
            try
            {
                var logPath = Server.MapPath("~/App_Data/ErrorLogs");

                // Ensure the directory exists
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                // Create a unique log file name based on the current date and time
                var logFile = Path.Combine(logPath, $"Errorlog_{DateTime.Now:yyyy_MM_dd}.txt");

                // Write logs to the file
                using (var writer = new StreamWriter(logFile, append: true))
                {
                    foreach (var log in logs)
                    {
                        writer.WriteLine($"{log.ErrorDate:yyyy-MM-dd HH:mm:ss}: {log.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that may have occurred during file operations
                System.Diagnostics.Trace.WriteLine($"Failed to save logs: {ex.Message}");
            }
        }



        [Authorization]
        private Trip GetTripByIdInternal(int id)
        {
            Trip trip = new Trip();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT t.TripID, cu.CustomerName, c.DriverName, c.VehicleNumber," +
                                   " t.PickupDate, t.PickupLocation, t.DropLocation, t.Kilometer, t.Amount, ca.CategoryCost" +
                                   " FROM Trip t" +
                                   " INNER JOIN Customers cu ON t.CustomerID = cu.CustomerID" +
                                   " INNER JOIN Cab c ON t.CabID = c.CabID" +
                                   " INNER JOIN Category ca ON c.CategoryID = ca.CategoryID" +
                                   " WHERE t.TripID = @TripID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@TripID", id);

                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        trip.TripID = Convert.ToInt32(rdr["TripID"]);
                        trip.CustomerName = rdr["CustomerName"].ToString();
                        trip.DriverName = rdr["DriverName"].ToString();
                        trip.VehicleNumber = rdr["VehicleNumber"].ToString();
                        trip.PickupDate = Convert.ToDateTime(rdr["PickupDate"]);
                        trip.PickupLocation = rdr["PickupLocation"].ToString();
                        trip.DropLocation = rdr["DropLocation"].ToString();
                        trip.Kilometer = Convert.ToDecimal(rdr["Kilometer"]);
                        trip.Amount = Convert.ToDecimal(rdr["Amount"]);
                        trip.CategoryCost = Convert.ToDecimal(rdr["CategoryCost"]);
                    }

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the trip data.", ex);
            }

            return trip;
        }

        [Authorization]
        public ActionResult ExportToExcel(FormCollection formCollection, string searchName, string categoryName, string selectedTripIds)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT cu.CustomerName, c.DriverName, c.VehicleNumber, ca.CategoryName, t.PickupDate, t.PickupLocation, t.DropLocation, t.Kilometer, t.Amount " +
                        "FROM Trip t " +
                        "INNER JOIN Customers cu ON t.CustomerID = cu.CustomerID " +
                        "INNER JOIN Cab c ON t.CabID = c.CabID " +
                        "INNER JOIN Category ca ON c.CategoryID = ca.CategoryID " +
                        "WHERE t.IsActive = 1";
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        query += " AND cu.CustomerName LIKE '%' + @SearchName + '%'";
                    }
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        query += " AND c.CategoryID = @CategoryID";
                    }
                    if (!string.IsNullOrEmpty(selectedTripIds))
                    {
                        string[] tripIds = selectedTripIds.Split(',');
                        string tripIdFilter = string.Join(",", tripIds.Select(id => $"'{id}'"));
                        query += $" AND t.TripID IN ({tripIdFilter})";
                    }
                    SqlCommand com = new SqlCommand(query, con);
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        com.Parameters.AddWithValue("@SearchName", searchName);
                    }

                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        com.Parameters.AddWithValue("@CategoryID", categoryName);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(com);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    dt.TableName = "TripDetails";
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dt);

                        ws.Row(1).Style.Font.Bold = true;
                        ws.Row(1).Style.Font.FontSize = 14;
                        ws.Row(1).Style.Fill.BackgroundColor = XLColor.DarkBlue;
                        ws.Row(1).Style.Font.FontColor = XLColor.White;
                        ws.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Columns().AdjustToContents();

                        // Prepare the response
                        using (var stream = new MemoryStream())
                        {
                            wb.SaveAs(stream);
                            stream.Position = 0;
                            Response.Clear();
                            Response.Buffer = true;
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("content-disposition", "attachment;filename=TripDetails.xlsx");
                            stream.WriteTo(Response.OutputStream);
                            Response.End();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log it, show an error message, etc.)
                // For simplicity, we'll just rethrow here.
                throw ex;
            }

            return new EmptyResult(); // Return an empty result since the file is handled via response
        }

    }
}