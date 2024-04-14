using System;
using System.Data.SqlClient;
using System.Windows;

namespace Diary
{
    public partial class PIN_window : Window
    {
        private int attempts = 0;
        private const int MaxAttempts = 5;

        private string connectionString = "Data Source=DESKTOP-F4HE8K3;Initial Catalog=Diary;Integrated Security=True";
        public PIN_window()
        {
            InitializeComponent();
            txt_PIN.Focus();
        }

        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            string pin = txt_PIN.Password;

            if (string.IsNullOrEmpty(pin))
            {
                MessageBox.Show("Proszę podać PIN.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SprawdzPIN(pin))
            {
                MessageBox.Show("Zalogowano pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow mw = new MainWindow();
                mw.Show();
                this.Close();
            }
            else
            {
                attempts++;
                if (attempts >= MaxAttempts)
                {
                    OpenPUKWindow();
                }
                else
                {
                    MessageBox.Show("Nieprawidłowy PIN. Pozostało prób: " + (MaxAttempts - attempts), "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenPUKWindow()
        {
            PUKWindow pukWindow = new PUKWindow();
            if (pukWindow.ShowDialog() == true) // Sprawdzenie wartości DialogResult
            {
                OpenChangePINWindow();
            }
            else
            {
                MessageBox.Show("Nieprawidłowy PUK lub operacja anulowana. Aplikacja zostanie zamknięta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }


        private void OpenChangePINWindow()
        {
            ChangePINWindow changePINWindow = new ChangePINWindow();
            changePINWindow.PINChanged += ChangePINWindow_PINChanged;
            changePINWindow.ShowDialog();
        }

        private void ChangePINWindow_PINChanged(object sender, EventArgs e)
        {
            PIN_window pinWindow = new PIN_window();
            pinWindow.Show();
            this.Close();
        }

        private bool SprawdzPIN(string pin)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM PIN_table WHERE PIN = @PinCode";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PinCode", pin);

                connection.Open();
                int pinCount = (int)command.ExecuteScalar();

                return pinCount > 0;
            }
        }
    }
}