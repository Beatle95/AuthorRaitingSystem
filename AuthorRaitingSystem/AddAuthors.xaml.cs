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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AuthorRaitingSystem
{
    /// <summary>
    /// Логика взаимодействия для AddAuthors.xaml
    /// </summary>
    public partial class AddAuthors : Window
    {
        MainWindow main_wnd;

        public ObservableCollection<Author> authors = new ObservableCollection<Author>();
        public ObservableCollection<Author> selected_authors = new ObservableCollection<Author>();

        public List<int> authors_ids = new List<int>();
        public AddAuthors(MainWindow mw, object pd)
        {
            InitializeComponent();
            main_wnd = mw;
            if (pd.GetType() == typeof(PageStudyBookDetails))
            {
                selected_authors.Clear();
                foreach(Author a in ((PageStudyBookDetails)pd).authors)
                {
                    selected_authors.Add(a);
                }
            }
            else if(pd.GetType() == typeof(PageScienceBookDetails))
            {
                selected_authors.Clear();
                foreach (Author a in ((PageScienceBookDetails)pd).authors)
                {
                    selected_authors.Add(a);
                }
            }
            else
            {
                throw new Exception("Ошибка преобразования типов!");
            }
            searchGrid.ItemsSource = authors;
            acceptGrid.ItemsSource = selected_authors;
        }

        private void b_close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void b_accept_Click(object sender, RoutedEventArgs e)
        {
            //if(acceptGrid.Items.Count < 1)
            //{
            //    MessageBox.Show("Необходимо добавить хотя бы одного автора!");
            //    return;
            //}
            DialogResult = true;
            Close();
        }

        //Создание автора
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (main_wnd.IsAdmin)
            {
                if (tb_family_name.Text == "")
                {
                    MessageBox.Show("Не введена фамилия");
                    return;
                }
                if (tb_name.Text == "")
                {
                    MessageBox.Show("Не введено имя");
                    return;
                }
                if (tb_middle_name.Text == "")
                {
                    MessageBox.Show("Не введено отчество");
                    return;
                }
                MySQLClient client = new MySQLClient(main_wnd.connectionString);
                Author auth;
                bool result = client.CheckAuthor(MySQLClient.SpecialChars(tb_family_name.Text), MySQLClient.SpecialChars(tb_name.Text), MySQLClient.SpecialChars(tb_middle_name.Text), out auth);
                if (result)
                {
                    int res = client.InsertAuthor(MySQLClient.SpecialChars(tb_family_name.Text), MySQLClient.SpecialChars(tb_name.Text), MySQLClient.SpecialChars(tb_middle_name.Text));
                    if (res != 1) { MessageBox.Show("Произошла ошибка при обмене данными с сервером"); return; }
                    client.CheckAuthor(MySQLClient.SpecialChars(tb_family_name.Text), MySQLClient.SpecialChars(tb_name.Text), MySQLClient.SpecialChars(tb_middle_name.Text), out auth);
                    tb_family_name.Text = "";
                    tb_name.Text = "";
                    tb_middle_name.Text = "";
                    authors.Add(auth);
                }
                else
                {
                    if (System.Windows.Forms.MessageBox.Show("Создать дубликат?", "Автор с таким именем уже существует", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                    {
                        int res = client.InsertAuthor(MySQLClient.SpecialChars(tb_family_name.Text), MySQLClient.SpecialChars(tb_name.Text), MySQLClient.SpecialChars(tb_middle_name.Text));
                        if (res != 1) { MessageBox.Show("Произошла ошибка при обмене данными с сервером"); return; }
                        client.CheckAuthor(MySQLClient.SpecialChars(tb_family_name.Text), MySQLClient.SpecialChars(tb_name.Text), MySQLClient.SpecialChars(tb_middle_name.Text), out auth);
                        tb_family_name.Text = "";
                        tb_name.Text = "";
                        tb_middle_name.Text = "";
                        authors.Add(auth);
                    }
                }
            }
            else
            {
                MessageBox.Show("Необходимо быть администратором, чтобы сделать это!");
            }
        }

        private void GetAuthors()
        {
            //Проверяем есть ли данные для поиска
            if(MySQLClient.SpecialChars(tb_family_name.Text) == "" && MySQLClient.SpecialChars(tb_name.Text) == "" && MySQLClient.SpecialChars(tb_middle_name.Text) == "")
            {
                return;
            }
            MySQLClient client = new MySQLClient(main_wnd.connectionString);
            string where_expr = "";
            bool not_first = false;
            //Формируем запрос
            if (MySQLClient.SpecialChars(tb_family_name.Text) != "")
            {
                not_first = true;
                where_expr += String.Format(@"author.family_name like ('%{0}%')", MySQLClient.SpecialChars(tb_family_name.Text));
            }
            if (MySQLClient.SpecialChars(tb_name.Text) != "")
            {
                if (not_first) { where_expr += " and "; }
                else not_first = true;
                where_expr += String.Format(@"author.name like ('%{0}%')", MySQLClient.SpecialChars(tb_name.Text));
            }
            if (MySQLClient.SpecialChars(tb_middle_name.Text) != "")
            {
                if (not_first) { where_expr += " and "; }
                else not_first = true;
                where_expr += String.Format(@"author.middle_name like ('%{0}%')", MySQLClient.SpecialChars(tb_middle_name.Text));
            }
            int ret_val = client.GetAuthors(where_expr, authors);
            if(ret_val == -2) popupNotFound.IsOpen = true;
            else if (ret_val != 1) MessageBox.Show("Произошла ошибка при обмене данными");
        }

        //Обработчик нажатия кнопки "Найти"
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GetAuthors();
        }

        //Обработчик нажатия клавиши Enter в textBox'ах
        private void tb_keyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                GetAuthors();
            }
        }

        //Кнопка перекладывания в окно выбора
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if(searchGrid.SelectedIndex != -1)
            {
                bool check = true;
                foreach(Author a in selected_authors)
                {
                    if (a.id == ((Author)searchGrid.SelectedItem).id) { check = false; popupAlredyAdded.IsOpen = true; break; }
                }
                if (check) 
                {
                    selected_authors.Add((Author)searchGrid.SelectedItem);
                    authors.Remove((Author)searchGrid.SelectedItem);
                }
            }
        }

        //Кнопка перекладывания из окно выбора
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (acceptGrid.SelectedIndex != -1)
            {
                bool check = true;
                foreach (Author a in authors)
                {
                    if (a.id == ((Author)acceptGrid.SelectedItem).id) { check = false; popupAlredyAdded.IsOpen = true; break; }
                }
                if (check)
                {
                    authors.Add((Author)acceptGrid.SelectedItem);
                    selected_authors.Remove((Author)acceptGrid.SelectedItem);
                }
            }
        }

        private void searchGrid_mouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IInputElement element = e.MouseDevice.DirectlyOver;
            if (element is TextBlock || element is System.Windows.Controls.DataGridCell || element is Border)
            {
                Button_Click_2(this, new RoutedEventArgs());
            }
        }

        private void acceptGrid_mouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IInputElement element = e.MouseDevice.DirectlyOver;
            if (element is TextBlock || element is System.Windows.Controls.DataGridCell || element is Border)
            {
                Button_Click_3(this, new RoutedEventArgs());
            }
        }
    }
}
