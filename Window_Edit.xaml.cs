using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
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
    /// Logika interakcji dla klasy Window_Edit.xaml
    /// </summary>
    public partial class Window_Edit : Window
    {
        public string EditedData { get; set; }
        public string EditedKategoria { get; set; }
        public string EditedWpis { get; set; }
        
        
        
        public Window_Edit(string data, string kategoria, string wpis)
        {
            InitializeComponent();
            d1.Text = data;
            t2.Text = kategoria;
            t1.Text = wpis;
            t1.Focus();
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            EditedData = d1.Text;
            EditedKategoria = t2.Text;
            EditedWpis = t1.Text;

          
            DialogResult = true;
        }

        private void t1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
