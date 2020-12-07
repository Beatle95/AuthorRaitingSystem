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
    /// Логика взаимодействия для AuthorWindow.xaml
    /// </summary>
    public partial class AuthorWindow : Window
    {
        MainWindow main_wnd;
        ObservableCollection<Author> authors = new ObservableCollection<Author>();
        public AuthorWindow(MainWindow mw)
        {
            InitializeComponent();
            main_wnd = mw;
            searchGrid.ItemsSource = authors;
        }

        //Обработчик нажатия кнопки "Найти"
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GetAuthors();
        }

        //Обработчик нажатия клавиши Enter в textBox'ах
        private void tb_keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetAuthors();
            }
        }

        private void GetAuthors()
        {
            //Проверяем есть ли данные для поиска
            if (MySQLClient.SpecialChars(tb_family_name.Text) == "" && MySQLClient.SpecialChars(tb_name.Text) == "" && MySQLClient.SpecialChars(tb_middle_name.Text) == "")
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
            if (ret_val == -2) popupNotFound.IsOpen = true;
            else if (ret_val != 1) MessageBox.Show("Произошла ошибка при обмене данными");
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

        private void b_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void b_delete_Click(object sender, RoutedEventArgs e)
        {
            if (main_wnd.User != "admin")
            {
                MessageBox.Show("Необходимо войти под учетной записью главного администратора! (admin)");
                return;
            }
            if (searchGrid.SelectedIndex != -1)
            {
                MySQLClient client = new MySQLClient(main_wnd.connectionString);
                client.DeleteAuthorById(((Author)searchGrid.SelectedItem).id);
                authors.Remove((Author)searchGrid.SelectedItem);
            }
        }

        private void b_redact_Click(object sender, RoutedEventArgs e)
        {
            if (searchGrid.SelectedIndex != -1)
            {
                AuthorRedact authorRedact = new AuthorRedact(main_wnd, (Author)searchGrid.SelectedItem);
                authorRedact.ShowDialog();
                authors.Clear();
            }
        }
    }
}
