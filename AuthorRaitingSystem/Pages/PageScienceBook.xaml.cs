using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;

namespace AuthorRaitingSystem
{
    /// <summary>
    /// Interaction logic for PageScienceBook.xaml
    /// </summary>
    public partial class PageScienceBook : Page
    {
        PageScienceBookDetails scienceBookDetails;
        public ObservableCollection<SciencePublication> science_publications = new ObservableCollection<SciencePublication>();
        MainWindow main_wnd;
        public PageScienceBook(MainWindow mw)
        {
            main_wnd = mw;
            InitializeComponent();
            dataGrid.ItemsSource = science_publications;
            frameSearch.Navigate(main_wnd.pageScienceSearchLong);
        }

        //Обработчик нажатия клавиши "Подробнее"
        //Создаем страницу подробной информации, переходим в нее через навигацию
        private void infoClick(object sender, RoutedEventArgs e)
        {
            //проверяем выбран ли элемент в dataGrid
            if (dataGrid.SelectedIndex != -1)
            {
                scienceBookDetails = new PageScienceBookDetails(main_wnd, ((SciencePublication)dataGrid.SelectedItem).id, dataGrid.SelectedIndex);
                main_wnd.mainFrame.Navigate(scienceBookDetails);
            }
            else
            {
                popupNotSelected.IsOpen = true;
            }
        }

        private void dataGrid_mouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IInputElement element = e.MouseDevice.DirectlyOver;
            if (element is TextBlock || element is System.Windows.Controls.DataGridCell || element is Border)
            {
                infoClick(dataGrid, new RoutedEventArgs());
            }
        }

        private void b_edit_Click(object sender, RoutedEventArgs e)
        {
            //проверяем выбран ли элемент в dataGrid
            if (dataGrid.SelectedIndex != -1)
            {
                scienceBookDetails = new PageScienceBookDetails(main_wnd, ((SciencePublication)dataGrid.SelectedItem).id, dataGrid.SelectedIndex);
                scienceBookDetails.b_edit_Click(this, new RoutedEventArgs());
                main_wnd.mainFrame.Navigate(scienceBookDetails);
            }
            else
            {
                popupNotSelected.IsOpen = true;
            }
        }

        //Вывод данных в excel
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (science_publications.Count < 1)
            {
                popupDataGridEmpty.IsOpen = true;
                return;
            }
            try
            {
                Excel.Application ex = new Microsoft.Office.Interop.Excel.Application();
                //Отобразить Excel
                ex.Visible = false;
                //Количество листов в рабочей книге
                ex.SheetsInNewWorkbook = 1;
                //Добавить рабочую книгу
                Excel.Workbook workBook = ex.Workbooks.Add(Type.Missing);
                //Отключить отображение окон с сообщениями
                ex.DisplayAlerts = false;
                //Получаем первый лист документа (счет начинается с 1)
                Excel.Worksheet sheet = (Excel.Worksheet)ex.Worksheets.get_Item(1);
                //Название листа (вкладки снизу)
                sheet.Name = "Отчет по результатам поиска";

                sheet.Cells[2, 2] = String.Format("Вид");
                sheet.Cells[2, 3] = String.Format("Форма");
                sheet.Cells[2, 4] = String.Format("Гриф");
                sheet.Cells[2, 5] = String.Format("Название");
                sheet.Cells[2, 6] = String.Format("Номер части издания");
                sheet.Cells[2, 7] = String.Format("Авторы");
                sheet.Cells[2, 8] = String.Format("Дата выхода издания в свет");
                sheet.Cells[2, 9] = String.Format("Структурное подразделение");

                int i = 2;
                foreach (SciencePublication sp in science_publications)
                {
                    i++;
                    sheet.Cells[i, 2] = sp.type;
                    sheet.Cells[i, 3] = sp.form;
                    sheet.Cells[i, 4] = sp.classification;
                    sheet.Cells[i, 5] = sp.title;
                    sheet.Cells[i, 6] = sp.part_number;
                    sheet.Cells[i, 7] = sp.s_authors;
                    sheet.Cells[i, 8] = sp.publication_date;
                    sheet.Cells[i, 9] = sp.structure_unit;
                }
                sheet.Columns.AutoFit();

                //Захватываем диапазон ячеек
                Excel.Range c1 = sheet.Cells[3, 2];
                Excel.Range c2 = sheet.Cells[i--, 9];
                Excel.Range range1 = sheet.get_Range(c1, c2);
                Excel.Range c3 = sheet.Cells[2, 2];
                Excel.Range c4 = sheet.Cells[2, 9];
                Excel.Range range2 = sheet.get_Range(c3, c4);

                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlEdgeBottom).Weight = 3d;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlEdgeTop).LineStyle = Excel.XlLineStyle.xlContinuous;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlEdgeTop).Weight = 3d;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlEdgeRight).LineStyle = Excel.XlLineStyle.xlContinuous;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlEdgeRight).Weight = 3d;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlEdgeLeft).LineStyle = Excel.XlLineStyle.xlContinuous;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlEdgeLeft).Weight = 3d;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlInsideVertical).LineStyle = Excel.XlLineStyle.xlContinuous;
                range2.Cells.Borders.get_Item(Excel.XlBordersIndex.xlInsideVertical).Weight = 3d;
                range2.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range2.Cells.Font.Bold = true;

                range1.Borders.get_Item(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous;
                range1.Borders.get_Item(Excel.XlBordersIndex.xlEdgeLeft).LineStyle = Excel.XlLineStyle.xlContinuous;
                range1.Borders.get_Item(Excel.XlBordersIndex.xlEdgeRight).LineStyle = Excel.XlLineStyle.xlContinuous;
                range1.Borders.get_Item(Excel.XlBordersIndex.xlInsideVertical).LineStyle = Excel.XlLineStyle.xlContinuous;
                //Шрифт для диапазона
                range1.Cells.Font.Name = "TimesNewRoman";
                range2.Cells.Font.Name = "TimesNewRoman";
                //Размер шрифта для диапазона
                range1.Cells.Font.Size = 10;
                range2.Cells.Font.Size = 10;

                //Создаем диалоговое окно выбора файла
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                // получаем выбранный файл
                string filename = saveFileDialog1.FileName;
                // читаем файл в строку

                ex.Application.ActiveWorkbook.SaveAs(filename, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlShared,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                ex.Quit();
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
        }

        private void deleteClick(object sender, RoutedEventArgs e)
        {
            MySQLClient client = new MySQLClient(main_wnd.connectionString);
            client.DeleteSciencePublicationById(((SciencePublication)dataGrid.SelectedItem).id);
            science_publications.Remove((SciencePublication)dataGrid.SelectedItem);
        }

        private void b_add_Click(object sender, RoutedEventArgs e)
        {
            PageScienceBookDetails psbd = new PageScienceBookDetails(main_wnd);
            psbd.b_edit_Click(this, new RoutedEventArgs());
            psbd.b_delete.Visibility = Visibility.Collapsed;
            main_wnd.mainFrame.Navigate(psbd);
        }
    }
}
