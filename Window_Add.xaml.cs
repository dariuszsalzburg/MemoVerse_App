using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MemoVerse_App
{
    /// <summary>
    /// Logika interakcji dla klasy Window_Add.xaml
    /// </summary>
    public partial class Window_Add : Window
    {
        string connectionString = "Data Source=DESKTOP-F4HE8K3;Initial Catalog=Diary;Integrated Security=True";
        public Window_Add()
        {
            InitializeComponent();
            t1.Focus();

        }

     

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd = new SqlCommand("INSERT INTO wpisyy (data,kategoria, wpis) VALUES (@data,@kategoria, @wpis)", con);
                    cmd.Parameters.AddWithValue("@wpis", t1.Text);
                    cmd.Parameters.AddWithValue("@kategoria", t2.Text);


                    string formattedDate = d1.SelectedDate?.ToString("yyyy-MM-dd");
                    cmd.Parameters.AddWithValue("@data", SqlDbType.Date).Value = formattedDate;

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Zapisano pomyślnie");
               
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd: " + ex.Message);
            }

        }

        private void t1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
