using System.Data.SqlClient;

namespace ogübüs
{
    public class AddStudent
    {
        private readonly DatabaseHelper dbHelper;

        public AddStudent()
        {
            dbHelper = new DatabaseHelper();
        }

     
        public void Add(string firstName, string lastName)
        {
            using (SqlConnection connection = dbHelper.GetConnection())
            {
                string query = "INSERT INTO öğrenciler (ad, soyad) VALUES (@firstName, @lastName)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
