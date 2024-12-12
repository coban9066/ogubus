using System;
using System.Data.SqlClient;

namespace ogübüs
{
    public class AddCourse
    {
        private readonly DatabaseHelper dbHelper;

        public AddCourse()
        {
            dbHelper = new DatabaseHelper();
        }

        public void Add(string courseName, string credit, string kontenjan)
        {
            if (int.TryParse(credit, out int creditValue) && int.TryParse(kontenjan, out int kontenjanValue))
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string query = "INSERT INTO Dersler (ders, kredi, kontenjan) VALUES (@courseName, @credit, @kontenjan)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@courseName", courseName);
                    command.Parameters.AddWithValue("@credit", creditValue);  
                    command.Parameters.AddWithValue("@kontenjan", kontenjanValue);  

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                throw new ArgumentException("Geçersiz kredi veya kontenjan değeri. Lütfen geçerli bir sayısal değer girin.");
            }
        }
    }
}
