using System.Data.SqlClient;

namespace ogübüs
{
    public class DeleteStudent
    {
        private readonly DatabaseHelper dbHelper;

        public DeleteStudent()
        {
            dbHelper = new DatabaseHelper();
        }

        public void DeleteById(int id)
        {
            using (SqlConnection connection = dbHelper.GetConnection())
            {
                // Öğrenciyi ID'ye göre silme sorgusu
                string query = "DELETE FROM öğrenciler WHERE id = @id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
