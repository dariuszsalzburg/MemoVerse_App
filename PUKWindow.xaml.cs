using System;
using System.Data.SqlClient;
using System.Windows;

namespace Diary
{
    public partial class PUKWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-F4HE8K3;Initial Catalog=Diary;Integrated Security=True";
        private int incorrectAttempts = 0;

        public PUKWindow()
        {
            InitializeComponent();
            txt_PUK.Focus();
        }

        public bool SprawdzPUK(string puk)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM PIN_table WHERE PUK = @PukCode";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PukCode", puk);

                connection.Open();
                int pukCount = (int)command.ExecuteScalar();

                return pukCount > 0;
            }
        }

        private void btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            string puk = txt_PUK.Password;

            if (string.IsNullOrEmpty(puk))
            {
                MessageBox.Show("Proszę podać PUK.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SprawdzPUK(puk))
            {
                DialogResult = true;
            }
            else
            {
                incorrectAttempts++;
                if (incorrectAttempts >= 3)
                {
                    MessageBox.Show("Przekroczono limit nieprawidłowych prób. Skontaktuj się z administratorem systemu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown(); // Zamknięcie aplikacji
                }
                else
                {
                    MessageBox.Show("Nieprawidłowy PUK. Pozostało prób: " + (3 - incorrectAttempts), "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
