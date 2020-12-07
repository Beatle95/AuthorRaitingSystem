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

namespace AuthorRaitingSystem
{
    /// <summary>
    /// Логика взаимодействия для DeleteUser.xaml
    /// </summary>
    public partial class DeleteUser : Window
    {
        MainWindow main_wnd;
        public DeleteUser(MainWindow mw)
        {
            InitializeComponent();
            main_wnd = mw;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MySQLClient.SpecialChars(tb_login.Text) == "admin")
            {
                MessageBox.Show("Невозможно удалить пользователя admin!");
                return;
            }
            MySQLClient client = new MySQLClient(main_wnd.connectionString);
            if (!client.DeleteUser(MySQLClient.SpecialChars(tb_login.Text)))
            {
                MessageBox.Show("Такой логин не найден в базе!");
                return;
            }
            MessageBox.Show("Пользователь " + tb_login.Text + " успешно удален!");
            tb_login.Text = "";
        }
    }
}
