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
    /// Логика взаимодействия для AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        MainWindow main_wnd;
        public AddUserWindow(MainWindow mw)
        {
            InitializeComponent();
            main_wnd = mw;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(tb_login.Text == "")
            {
                MessageBox.Show("Введите логин!");
                return;
            }
            if(tb_login.Text.Length < 4)
            {
                MessageBox.Show("Логин слишком короткий!");
                return;
            }
            if (pb_pass.Password == "")
            {
                MessageBox.Show("Введите пароль!");
                return;
            }
            if (pb_confirm_pass.Password != pb_pass.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }
            MySQLClient client = new MySQLClient(main_wnd.connectionString);
            if (tb_login.IsHitTestVisible == true)
            {
                if (client.IsLoginFree(MySQLClient.SpecialChars(tb_login.Text)))
                {
                    //Если логин свободен, то сохраняем
                    client.SaveNewUser(MySQLClient.SpecialChars(tb_login.Text), MySQLClient.SpecialChars(pb_pass.Password));
                }
                else
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                }
                MessageBox.Show("Пользователь успешно добавлен!");
            }
            else
            {
                client.ChangePassword(MySQLClient.SpecialChars(tb_login.Text), MySQLClient.SpecialChars(pb_pass.Password));
                MessageBox.Show("Пароль изменен");
            }
            Close();
        }
    }
}
