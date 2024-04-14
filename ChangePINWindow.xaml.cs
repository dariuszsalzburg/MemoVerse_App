
using System;
using System.Data.SqlClient;
using System.Windows;

namespace Diary
{
    public partial class ChangePINWindow : Window
    {
        public event EventHandler PINChanged;

        private string connectionString = "Data Source=DESKTOP-F4HE8K3;Initial Catalog=Diary;Integrated Security=True";

        public ChangePINWindow()
        {
            InitializeComponent();
        }

        private void btn_Change_Click(object sender, RoutedEventArgs e)
        {
            string newPin = txt_NewPIN.Password;

            if (string.IsNullOrEmpty(newPin))
            {
                MessageBox.Show("Nowy kod PIN nie może być pusty. Aplikacja zostanie zamknięta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            else
            {
                ZmienPIN();

            }
   
        }

        private void ZmienPIN()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE PIN_table SET PIN=@NewPin";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@NewPin", txt_NewPIN.Password);
         


                connection.Open();
                command.ExecuteNonQuery();
                MessageBox.Show("Pomyślnie zmieniono PIN", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

   
    }
}
