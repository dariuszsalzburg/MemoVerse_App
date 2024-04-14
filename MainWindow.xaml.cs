using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Spreadsheet = DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using System.Windows.Data;

using System.IO;

using Border = DocumentFormat.OpenXml.Spreadsheet.Border;
using MemoVerse_App;
using iTextSharp.text.pdf;
using iTextSharp.text;

using System.Text;

namespace Diary
{
    public partial class MainWindow : Window
    {
        string connectionString = "Data Source=DESKTOP-F4HE8K3;Initial Catalog=Diary;Integrated Security=True";

        public MainWindow()
        {
            InitializeComponent();
            WypełnijDanymi();
          
        }

      


        private void DodajDoUlubionych()
        {
            try
            {
                if (g1.SelectedItems.Count > 0)
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        foreach (DataRowView rowView in g1.SelectedItems)
                        {
                            DataRow row = rowView.Row;
                            string data = row["Data"].ToString();
                            string kategoria = row["Kategoria"].ToString();
                            string wpis = row["Wpis"].ToString();

                         
                            SqlCommand cmd = new SqlCommand("INSERT INTO ulub_wpisy (Data, Kategoria, Wpis) VALUES (@Data, @Kategoria, @Wpis)", con);
                            cmd.Parameters.AddWithValue("@Data", data);
                            cmd.Parameters.AddWithValue("@Kategoria", kategoria);
                            cmd.Parameters.AddWithValue("@Wpis", wpis);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Wybrane wpisy zostały dodane do ulubionych.");
                }
                else
                {
                    MessageBox.Show("Wybierz wpisy do dodania do ulubionych.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas dodawania wpisów do ulubionych: " + ex.Message);
            }
        }



        private void Export()
        {
            try
            {
                // Pobranie danych z DataGrid
                DataView dataView = (DataView)g1.ItemsSource;
                System.Data.DataTable dataTable = dataView.ToTable();

                // Usunięcie istniejących kolumn z DataGrid
                g1.Columns.Clear();

                // Zapisanie pliku Excel
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel files (*.xls)|*.xls|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Tworzenie pliku Excel
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(saveFileDialog.FileName, SpreadsheetDocumentType.Workbook))
                    {
                        // Dodanie arkusza do pliku Excel
                        WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                        workbookPart.Workbook = new Spreadsheet.Workbook();

                        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                        worksheetPart.Worksheet = new Spreadsheet.Worksheet(new Spreadsheet.SheetData());

                        Spreadsheet.Sheets sheets = new Spreadsheet.Sheets();
                        Spreadsheet.Sheet sheet = new Spreadsheet.Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
                        sheets.Append(sheet);

                        workbookPart.Workbook.AppendChild(sheets);

                        

                        // Dodanie wiersza nagłówka z nazwami kolumn
                        Spreadsheet.SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<Spreadsheet.SheetData>();
                        Spreadsheet.Row headerRow = new Spreadsheet.Row();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Spreadsheet.Cell cell = new Spreadsheet.Cell();
                            cell.DataType = Spreadsheet.CellValues.String;
                            cell.CellValue = new Spreadsheet.CellValue(column.ColumnName);
                            headerRow.Append(cell);

                            // Dodanie nowych kolumn do DataGrid
                            g1.Columns.Add(new DataGridTextColumn()
                            {
                                Header = column.ColumnName,
                                Binding = new Binding(string.Format("[{0}]", column.ColumnName))
                            });
                        }
                        sheetData.Append(headerRow);

                        // Dodanie danych
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Spreadsheet.Row newRow = new Spreadsheet.Row();
                            foreach (var cellValue in row.ItemArray)
                            {
                                Spreadsheet.Cell cell = new Spreadsheet.Cell();
                                cell.DataType = Spreadsheet.CellValues.String;
                                cell.CellValue = new Spreadsheet.CellValue(cellValue.ToString());
                                newRow.Append(cell);
                            }
                            sheetData.Append(newRow);
                        }
                    }

                    MessageBox.Show("Dane zostały pomyślnie wyeksportowane do pliku Excel.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas eksportowania danych do pliku Excel: " + ex.Message);
            }
        }



        private void Import()
        {
            try
            {
                // Wybierz plik Excel
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    // Otwórz plik Excel
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(openFileDialog.FileName, false))
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
                        if (worksheetPart == null)
                        {
                            MessageBox.Show("Brak arkusza w wybranym pliku Excel.");
                            return;
                        }

                        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
                        if (sheetData == null)
                        {
                            MessageBox.Show("Brak danych w arkuszu w wybranym pliku Excel.");
                            return;
                        }

                        // Konwertuj dane z pliku Excel na DataTable
                        System.Data.DataTable dataTable = new System.Data.DataTable();

                        // Uzyskaj pierwszy wiersz
                        Row firstRow = sheetData.Elements<Row>().FirstOrDefault();
                        if (firstRow != null)
                        {
                            foreach (Cell cell in firstRow.Elements<Cell>())
                            {
                                string columnName = GetCellValue(workbookPart, cell);
                                dataTable.Columns.Add(columnName);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Brak nagłówków kolumn w wybranym pliku Excel.");
                            return;
                        }

                        // Odczytaj pozostałe wiersze jako dane
                        foreach (Row row in sheetData.Elements<Row>().Skip(1))
                        {
                            DataRow dataRow = dataTable.NewRow();
                            int columnIndex = 0;
                            foreach (Cell cell in row.Elements<Cell>())
                            {
                                dataRow[columnIndex] = GetCellValue(workbookPart, cell);
                                columnIndex++;
                            }
                            dataTable.Rows.Add(dataRow);
                        }


                    

                        // Wstaw nowe dane do bazy danych
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                string insertQuery = "INSERT INTO wpisyy (Data,Kategoria,Wpis) VALUES (@Data,@Kategoria,@Wpis)"; 
                                SqlCommand command = new SqlCommand(insertQuery, connection);
                                
                               
                                command.Parameters.AddWithValue("@Data", row[0]);
                                command.Parameters.AddWithValue("@Kategoria", row[1]);
                                command.Parameters.AddWithValue("@Wpis", row[2]);
                            
                                command.ExecuteNonQuery();
                            }
                        }




                
                        g1.ItemsSource = dataTable.DefaultView;
                        g1.UpdateLayout();
                        g1.Items.Refresh();

                        MessageBox.Show("Dane zostały pomyślnie zaimportowane z pliku Excel i zapisane w bazie danych.");
                        MessageBox.Show("Liczba wczytanych wierszy: " + dataTable.Rows.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas importowania danych z pliku Excel: " + ex.Message);
            }
        }
        private string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            SharedStringTablePart stringTablePart = workbookPart.SharedStringTablePart;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ElementAt(int.Parse(cell.CellValue.Text)).InnerText;
            }
            else
            {
                return cell.CellValue?.Text ?? "";
            }
        }

       




        private void Wyszukaj(string searchText)
        {
            try
            {
                if (g1.ItemsSource is DataView dataView)
                {
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                       
                        dataView.RowFilter = "";
                    }
                    else
                    {
                        
                        dataView.RowFilter = $"Wpis LIKE '%{searchText}%'";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas wyszukiwania wpisów: " + ex.Message);
            }
        }

        private void Wyszukaj2()
        {
            try
            {
                if (g1.ItemsSource is DataView dataView)
                {
                    if (string.IsNullOrWhiteSpace(t2.Text))
                    {
                        dataView.RowFilter = ""; // Wyczyszczenie filtra, jeśli pole tekstowe jest puste
                    }
                    else
                    {
                        // Wyczyszczenie poprzedniego filtra, jeśli już istnieje
                        if (!string.IsNullOrWhiteSpace(dataView.RowFilter))
                        {
                            dataView.RowFilter = "";
                        }

                        dataView.RowFilter = $"Kategoria LIKE '{t2.Text}'"; // Ustawienie nowego filtra na wybraną kategorię
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas wyszukiwania wpisów: " + ex.Message);
            }
        }




        private void UsuńWpis()
        {
            try
            {
                if (g1.SelectedItems.Count > 0)
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        foreach (DataRowView rowView in g1.SelectedItems)
                        {
                            DataRow row = rowView.Row;
                            string wpis = row["Wpis"].ToString(); 
                            SqlCommand cmd = new SqlCommand("DELETE FROM wpisyy WHERE Wpis = @Wpis", con);
                            cmd.Parameters.AddWithValue("@Wpis", wpis);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Wybrane wpisy zostały pomyślnie usunięte.");
                    WypełnijDanymi(); 
                }
                else
                {
                    MessageBox.Show("Wybierz wpisy do usunięcia.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas usuwania wpisów: " + ex.Message);
            }
        }



        public void WypełnijDanymi()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string selectQuery = "SELECT * FROM wpisyy";
                    SqlCommand cmd = new SqlCommand(selectQuery, con);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    g1.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas pobierania danych: " + ex.Message);
            }
        }
        private void WypełnijDanymiUlub()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string selectQuery = "SELECT * FROM ulub_wpisy";
                    SqlCommand cmd = new SqlCommand(selectQuery, con);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    g1.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas pobierania danych: " + ex.Message);
            }
        }




        private void EdytujWpis()
        {
            try
            {
                if (g1.SelectedItems.Count == 1) 
                {
                    DataRowView selectedRowView = (DataRowView)g1.SelectedItem;
                    DataRow selectedRow = selectedRowView.Row;

                    
                    string data = selectedRow["Data"].ToString();
                    string kategoria = selectedRow["Kategoria"].ToString();
                    string wpis = selectedRow["Wpis"].ToString();

                    Window_Edit editWindow = new Window_Edit(data, kategoria, wpis);
                    if (editWindow.ShowDialog() == true)
                    {
                        
                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();
                            SqlCommand cmd = new SqlCommand("UPDATE wpisyy SET Data = @Data, Kategoria = @Kategoria, Wpis = @Wpis WHERE Data = @OldData AND Kategoria = @OldKategoria AND Wpis = @OldWpis", con);
                            cmd.Parameters.AddWithValue("@Data", editWindow.EditedData);
                            cmd.Parameters.AddWithValue("@Kategoria", editWindow.EditedKategoria);
                            cmd.Parameters.AddWithValue("@Wpis", editWindow.EditedWpis);
                            cmd.Parameters.AddWithValue("@OldData", data);
                            cmd.Parameters.AddWithValue("@OldKategoria", kategoria);
                            cmd.Parameters.AddWithValue("@OldWpis", wpis);

                            cmd.ExecuteNonQuery();
                        }
                      
                        WypełnijDanymi();
                        MessageBox.Show("Wpis został pomyślnie zaktualizowany.");
                    }
                }
                else
                {
                    MessageBox.Show("Wybierz dokładnie jeden wpis do edycji.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas edycji wpisu: " + ex.Message);
            }
        }


        private void searchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = searchTextBox.Text;
            Wyszukaj(searchText);
        }


        private void btn_Fav_Click(object sender, RoutedEventArgs e)
        {
            WypełnijDanymiUlub();
        }

        private void btn_All_Click(object sender, RoutedEventArgs e)
        {
            WypełnijDanymi();
        }



        private void ExportToPdf(DataGrid dataGrid)
        {
            try
            {
                // Wybór ścieżki do zapisu pliku
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // Tworzenie dokumentu PDF
                    Document document = new Document();
                    PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                    document.Open();

                    // Dodawanie zawartości z DataGrid do dokumentu PDF
                    PdfPTable pdfTable = new PdfPTable(dataGrid.Columns.Count);
                    pdfTable.DefaultCell.Padding = 3;
                    pdfTable.WidthPercentage = 100;
                    pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                    // Ustawienie czcionki z polskimi znakami
                    BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);

                    // Dodawanie nagłówków kolumn
                    foreach (DataGridColumn column in dataGrid.Columns)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(column.Header.ToString(), new iTextSharp.text.Font(bf)));
                        cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240); // Ustaw kolor tła dla nagłówków kolumn
                        pdfTable.AddCell(cell);
                    }

                    // Dodawanie danych
                    foreach (var item in dataGrid.Items)
                    {
                        if (item is DataRowView rowView)
                        {
                            DataRow row = rowView.Row;
                            // Mapowanie danych z DataRow do odpowiednich kolumn DataGrid
                            var data = row["Data"].ToString();
                            var kategoria = row["Kategoria"].ToString();
                            var wpis = row["Wpis"].ToString();

                            pdfTable.AddCell(new PdfPCell(new Phrase(data, new iTextSharp.text.Font(bf))));
                            pdfTable.AddCell(new PdfPCell(new Phrase(kategoria, new iTextSharp.text.Font(bf))));
                            pdfTable.AddCell(new PdfPCell(new Phrase(wpis, new iTextSharp.text.Font(bf))));
                        }
                    }

                    document.Add(pdfTable);
                    document.Close();

                    MessageBox.Show("Plik PDF został pomyślnie wyeksportowany.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas eksportowania danych do pliku PDF: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            Window_Add wa = new Window_Add();
            wa.Show();
        }
    




        
        private void ExportToCSV(DataGrid dataGrid)
        {
            try
            {
                // Wybór ścieżki do zapisu pliku
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // Otwarcie strumienia do zapisu do pliku
                    using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        // Zapisanie nagłówków kolumn
                        foreach (var column in dataGrid.Columns)
                        {
                            writer.Write('"' + column.Header.ToString() + '"' + ",");
                        }
                        writer.WriteLine(); // Nowy wiersz po nagłówkach

                        // Zapisanie danych
                        foreach (var item in dataGrid.Items)
                        {
                            var row = item as DataRowView;
                            if (row != null)
                            {
                                foreach (var cellValue in row.Row.ItemArray)
                                {
                                    writer.Write('"' + cellValue.ToString().Replace("\"", "\"\"") + '"' + ",");
                                }
                                writer.WriteLine(); // Nowy wiersz po danych
                            }
                        }
                    }

                    MessageBox.Show("Dane zostały pomyślnie wyeksportowane do pliku CSV.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas eksportowania danych do pliku CSV: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromCSV(DataGrid dataGrid)
        {
            try
            {
                // Wybór pliku do importu
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSV files (*.csv)|*.csv";
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    // Otwarcie strumienia do odczytu pliku
                    using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                    {
                        // Odczytanie nagłówków kolumn
                        string[] headers = reader.ReadLine().Split(',');

                        // Utworzenie tabeli do przechowywania danych
                        DataTable dataTable = new DataTable();
                        foreach (string header in headers)
                        {
                            dataTable.Columns.Add(header.Replace("\"", ""));
                        }

                        // Odczytanie danych
                        while (!reader.EndOfStream)
                        {
                            string[] rowValues = reader.ReadLine().Split(',');
                            DataRow row = dataTable.NewRow();
                            for (int i = 0; i < headers.Length; i++)
                            {
                                row[i] = rowValues[i].Replace("\"", "");
                            }
                            dataTable.Rows.Add(row);
                        }

                        // Ustawienie danych w datagridzie
                        dataGrid.ItemsSource = dataTable.DefaultView;

                        // Wstaw nowe dane do bazy danych
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                string insertQuery = "INSERT INTO wpisyy (Data,Kategoria,Wpis) VALUES (@Data,@Kategoria,@Wpis)";
                                SqlCommand command = new SqlCommand(insertQuery, connection);


                                command.Parameters.AddWithValue("@Data", row[0]);
                                command.Parameters.AddWithValue("@Kategoria", row[1]);
                                command.Parameters.AddWithValue("@Wpis", row[2]);

                                command.ExecuteNonQuery();
                            }
                            connection.Close();
                        }
                    }

                    MessageBox.Show("Dane zostały pomyślnie zaimportowane z pliku CSV.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas importowania danych z pliku CSV: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ImportCSV_Click(object sender, RoutedEventArgs e)
        {
            ImportFromCSV(g1);
        }

        private void Usun_Click(object sender, RoutedEventArgs e)
        {
            UsuńWpis();
        }

        private void Edytuj_Click(object sender, RoutedEventArgs e)
        {
            EdytujWpis();
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            Export();
        }

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            Import();
        }

        private void DodajUlub_Click(object sender, RoutedEventArgs e)
        {
            DodajDoUlubionych();
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            ExportToCSV(g1);
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            ExportToPdf(g1);
        }



        private void t2_LostFocus(object sender, RoutedEventArgs e)
        {
            Wyszukaj2();
        }
    }

}
