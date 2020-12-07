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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        MainWindow main_wnd;
        public LoginWindow(MainWindow mw)
        {
            InitializeComponent();
            main_wnd = mw;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MySQLClient client = new MySQLClient(main_wnd.connectionString);
            if (client.Authorize(MySQLClient.SpecialChars(tb_login.Text), MySQLClient.SpecialChars(pb_password.Password)))
            {
                main_wnd.IsAdmin = true;
                main_wnd.SetPreviliges();
                main_wnd.adminMenuItem.Header = "Выйти из учетной записи";
                main_wnd.CallEnterAdminNotify();
                main_wnd.User = tb_login.Text;
                Close();
            }
            else
            {
                MessageBox.Show("Неправильные логин и/или пароль!");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
