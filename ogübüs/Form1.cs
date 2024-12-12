using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ogübüs
{
    public partial class Form1 : Form
    {
        private readonly DatabaseHelper dbHelper;
        private readonly AddStudent addStudent;
        private readonly DeleteStudent deleteStudent;
        private readonly AddCourse addCourse;

        public Form1()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            addStudent = new AddStudent();
            deleteStudent = new DeleteStudent();
            addCourse = new AddCourse();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void LoadStudents()
        {
            listBox1.Items.Clear();
            using (SqlConnection connection = dbHelper.GetConnection())
            {
                string query = "SELECT id, ad, soyad FROM öğrenciler";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string studentInfo = $"{reader["id"]} - {reader["ad"]} {reader["soyad"]}";
                    listBox1.Items.Add(studentInfo);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            string surname = textBox2.Text;

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(surname))
            {
                addStudent.Add(name, surname);
                MessageBox.Show("Öğrenci eklendi.");
                LoadStudents();
            }
            else
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedItem = listBox1.SelectedItem.ToString();
                int id = int.Parse(selectedItem.Split('-')[0].Trim());

                deleteStudent.DeleteById(id);
                MessageBox.Show("Seçilen öğrenci silindi.");
                LoadStudents();
            }
            else
            {
                MessageBox.Show("Lütfen bir öğrenci seçin.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string courseName = textBox3.Text;
            string credit = listBox2.SelectedItem?.ToString();
            string kontenjan = textBox6.Text; 

            if (!string.IsNullOrWhiteSpace(courseName) && !string.IsNullOrWhiteSpace(credit) && !string.IsNullOrWhiteSpace(kontenjan))
            {
            
                int kontenjanValue;
                if (int.TryParse(kontenjan, out kontenjanValue))
                {
                    using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-N5LCA8G;Initial Catalog=ogübüs;Integrated Security=True;Encrypt=False"))
                    {
                        string query = "UPDATE Dersler SET kontenjan = @kontenjan WHERE ders = @courseName";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@kontenjan", kontenjanValue);
                        command.Parameters.AddWithValue("@courseName", courseName);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }

                    addCourse.Add(courseName, credit, kontenjan); 

                    MessageBox.Show("Ders eklendi ve kontenjan güncellendi.");
                }
                else
                {
                    MessageBox.Show("Lütfen geçerli bir kontenjan değeri girin.");
                }
            }
            else
            {
                MessageBox.Show("Lütfen tüm alanları doldurun ve bir kredi seçin.");
            }
        }



        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                listBox3.Items.Clear();
                List<string> courses = dbHelper.GetCourses();
                foreach (var course in courses)
                {
                    listBox3.Items.Add(course);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox3.SelectedItem != null)
                {
                    string selectedItem = listBox3.SelectedItem.ToString();
                    string courseName = selectedItem.Split('-')[0].Trim();
                    dbHelper.DeleteCourse(courseName);

                    MessageBox.Show("Seçilen ders silindi.");
                    button6_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("Lütfen bir ders seçin.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                listBox4.Items.Clear();
                List<string> courses = dbHelper.GetCourses();
                foreach (var course in courses)
                {
                    listBox4.Items.Add(course);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string selectedCourse = listBox4.SelectedItem?.ToString();  

            if (string.IsNullOrEmpty(selectedCourse))
            {
                MessageBox.Show("Lütfen bir ders seçin.");
                return;
            }

            string studentName = textBox5.Text; 
            string studentSurname = textBox4.Text;  

            if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentSurname))
            {
                MessageBox.Show("Lütfen öğrenci adı ve soyadını girin.");
                return;
            }

            if (dbHelper.IsCourseAlreadyTaken(studentName, studentSurname, selectedCourse))
            {
                MessageBox.Show("Bu dersi zaten almışsınız.");
                return;
            }

            if (!dbHelper.CanRegisterForCourse(studentName, studentSurname, selectedCourse))
            {
                MessageBox.Show("Ders kaydınız için yeterli kredi veya kontenjan yok.");
                return;
            }

            dbHelper.AddStudentCourse(studentName, studentSurname, selectedCourse);

            dbHelper.UpdateCourseSeats(selectedCourse);

            MessageBox.Show("Ders kaydınız başarıyla yapılmıştır.");
        }


        private void button5_Click(object sender, EventArgs e)
        {
            string studentName = textBox8.Text;  
            string studentSurname = textBox7.Text;  

            if (string.IsNullOrWhiteSpace(studentName) || string.IsNullOrWhiteSpace(studentSurname))
            {
                MessageBox.Show("Lütfen ad ve soyad girin.");
                return;
            }

            listBox5.Items.Clear(); 

            int studentId = GetStudentId(studentName, studentSurname);
            if (studentId == 0)
            {
                MessageBox.Show("Öğrenci bulunamadı.");
                return;
            }

            using (SqlConnection connection = dbHelper.GetConnection())
            {
                string query = @"
            SELECT d.ders
            FROM Dersler d
            WHERE d.id IN (
                SELECT dk.ders_id
                FROM Ders_kayit dk
                WHERE dk.ogrenci_id = @studentId
            )";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId); 

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string courseName = reader["ders"].ToString();
                        listBox5.Items.Add(courseName);
                    }

                    if (listBox5.Items.Count == 0)
                    {
                        MessageBox.Show("Bu öğrenciye ait ders bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Bir hata oluştu: {ex.Message}");
                }
            }
        }

        private int GetStudentId(string studentName, string studentSurname)
        {
            int studentId = 0;

            using (SqlConnection connection = dbHelper.GetConnection())
            {
                string query = "SELECT id FROM Öğrenciler WHERE ad = @name AND soyad = @surname";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", studentName);
                command.Parameters.AddWithValue("@surname", studentSurname);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        studentId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Öğrenci ID'si alınırken hata oluştu: {ex.Message}");
                }
            }

            return studentId;
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            string studentSurname = textBox7.Text;
            string studentName = textBox8.Text;

            string selectedCourse = listBox5.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentSurname))
            {
                MessageBox.Show("Lütfen öğrenci adı ve soyadını girin.");
                return;
            }

            if (string.IsNullOrEmpty(selectedCourse))
            {
                MessageBox.Show("Lütfen bir ders seçin.");
                return;
            }

            try
            {
                DatabaseHelper dbHelper = new DatabaseHelper();

                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    connection.Open();

                    string query = "DELETE FROM Ders_Kayit " +
                                   "WHERE ogrenci_id = (SELECT id FROM Öğrenciler WHERE ad = @ad AND soyad = @soyad) " +
                                   "AND ders_id = (SELECT id FROM Dersler WHERE ders = @ders)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ad", studentName);
                    command.Parameters.AddWithValue("@soyad", studentSurname);
                    command.Parameters.AddWithValue("@ders", selectedCourse);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Ders kaydı başarıyla silindi.");
                    }
                    else
                    {
                        MessageBox.Show("Belirtilen öğrenci için bu ders bulunamadı veya zaten kayıtlı değil.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}");
            }
        }
    }


}
