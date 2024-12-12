using System;
using System.Data.SqlClient;

namespace ogübüs
{
    public class StudentRegistration
    {
        private readonly DatabaseHelper dbHelper;

        public StudentRegistration()
        {
            dbHelper = new DatabaseHelper();
        }

        public void RegisterStudentForCourse(string studentName, string studentSurname, string courseName)
        {
            using (SqlConnection connection = dbHelper.GetConnection())
            {
                string checkQuery = "SELECT kontenjan FROM Dersler WHERE ders = @courseName";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@courseName", courseName);

                connection.Open();

                var result = checkCommand.ExecuteScalar();
                if (result == DBNull.Value)
                {
                    throw new InvalidOperationException("Dersin kontenjanı bilgisi bulunamadı.");
                }

                int availableSlots = Convert.ToInt32(result);

                if (availableSlots <= 0)
                {
                    throw new InvalidOperationException("Dersin kontenjanı dolmuş.");
                }

                string registerQuery = "INSERT INTO ÖğrencilerDersler (student_name, student_surname, course_name) " +
                                       "VALUES (@studentName, @studentSurname, @courseName)";
                SqlCommand registerCommand = new SqlCommand(registerQuery, connection);
                registerCommand.Parameters.AddWithValue("@studentName", studentName);
                registerCommand.Parameters.AddWithValue("@studentSurname", studentSurname);
                registerCommand.Parameters.AddWithValue("@courseName", courseName);

                int rowsAffected = registerCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    string updateQuery = "UPDATE Dersler SET kontenjan = kontenjan - 1 WHERE ders = @courseName";
                    SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@courseName", courseName);
                    updateCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
