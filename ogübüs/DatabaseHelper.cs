using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ogübüs
{
    public class DatabaseHelper
    {
        private readonly string connectionString = "Data Source=DESKTOP-N5LCA8G;Initial Catalog=ogübüs;Integrated Security=True;Encrypt=False";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public List<string> GetCourses()
        {
            List<string> courses = new List<string>();
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT ders FROM Dersler"; 
                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string course = $"{reader["ders"]}";
                    courses.Add(course);
                }
            }
            return courses;
        }

        public bool IsCourseAlreadyTaken(string studentName, string studentSurname, string courseName)
        {
            bool isTaken = false;
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT COUNT(*) FROM Ders_Kayit " +
                               "JOIN Öğrenciler ON Öğrenciler.id = Ders_Kayit.ogrenci_id " +
                               "JOIN Dersler ON Dersler.id = Ders_Kayit.ders_id " +
                               "WHERE Öğrenciler.ad = @ad AND Öğrenciler.soyad = @soyad AND Dersler.ders = @courseName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ad", studentName);
                command.Parameters.AddWithValue("@soyad", studentSurname);
                command.Parameters.AddWithValue("@courseName", courseName);

                connection.Open();
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    isTaken = true;
                }
            }
            return isTaken;
        }

        public void DeleteCourse(string courseName)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "DELETE FROM Dersler WHERE ders = @courseName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseName", courseName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public int GetStudentTotalCredits(string studentName, string studentSurname)
        {
            int totalCredits = 0;
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT SUM(kredi) FROM Dersler " +
                               "JOIN Ders_Kayit ON Dersler.id = Ders_Kayit.ders_id " +
                               "JOIN Öğrenciler ON Öğrenciler.id = Ders_Kayit.ogrenci_id " +
                               "WHERE Öğrenciler.ad = @ad AND Öğrenciler.soyad = @soyad";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ad", studentName);
                command.Parameters.AddWithValue("@soyad", studentSurname);

                connection.Open();
                object result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    totalCredits = Convert.ToInt32(result);
                }
            }
            return totalCredits;
        }

        public int GetCourseSeats(string courseName)
        {
            int availableSeats = 0;
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT kontenjan FROM Dersler WHERE ders = @courseName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseName", courseName);

                connection.Open();
                object result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    availableSeats = Convert.ToInt32(result);
                }
            }
            return availableSeats;
        }

        public void UpdateCourseSeats(string courseName)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "UPDATE Dersler SET kontenjan = kontenjan - 1 WHERE ders = @courseName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseName", courseName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void AddStudentCourse(string studentName, string studentSurname, string courseName)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "INSERT INTO Ders_Kayit (ogrenci_id, ders_id) " +
                               "SELECT Öğrenciler.id, Dersler.id " +
                               "FROM Öğrenciler, Dersler " +
                               "WHERE Öğrenciler.ad = @ad AND Öğrenciler.soyad = @soyad AND Dersler.ders = @courseName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ad", studentName);
                command.Parameters.AddWithValue("@soyad", studentSurname);
                command.Parameters.AddWithValue("@courseName", courseName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool IsStudentRegisteredForCourse(string studentName, string studentSurname, string courseName)
        {
            bool isRegistered = false;
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT COUNT(*) FROM Ders_Kayit " +
                               "JOIN Öğrenciler ON Öğrenciler.id = Ders_Kayit.ogrenci_id " +
                               "JOIN Dersler ON Dersler.id = Ders_Kayit.ders_id " +
                               "WHERE Öğrenciler.ad = @ad AND Öğrenciler.soyad = @soyad AND Dersler.ders = @courseName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ad", studentName);
                command.Parameters.AddWithValue("@soyad", studentSurname);
                command.Parameters.AddWithValue("@courseName", courseName);

                connection.Open();
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    isRegistered = true;
                }
            }
            return isRegistered;
        }

        public bool CanRegisterForCourse(string studentName, string studentSurname, string courseName)
        {
            int totalCredits = GetStudentTotalCredits(studentName, studentSurname);

            if (totalCredits >= 10)
            {
                return false;
            }

            int availableSeats = GetCourseSeats(courseName);
            int registeredStudentsCount = GetRegisteredStudentsCount(courseName);

            if (registeredStudentsCount >= availableSeats)
            {
                return false;
            }

            return true;
        }

        public int GetRegisteredStudentsCount(string courseName)
        {
            int studentCount = 0;
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT COUNT(*) FROM Ders_Kayit " +
                               "JOIN Dersler ON Ders_Kayit.ders_id = Dersler.id " +
                               "WHERE Dersler.ders = @courseName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseName", courseName);

                connection.Open();
                studentCount = (int)command.ExecuteScalar();
            }
            return studentCount;
        }
    }
}
