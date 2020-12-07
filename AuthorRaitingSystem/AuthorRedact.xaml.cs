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
    /// Логика взаимодействия для AuthorRedact.xaml
    /// </summary>
    public partial class AuthorRedact : Window
    {
        MainWindow main_wnd;
        int auth_id;
        public AuthorRedact(MainWindow mw, Author auth)
        {
            InitializeComponent();
            main_wnd = mw;
            auth_id = auth.id;
            tb_family_name.Text = auth.family_name;
            tb_middle_name.Text = auth.middle_name;
            tb_name.Text = auth.name;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(tb_family_name.Text.Length == 0 || tb_middle_name.Text.Length == 0 || tb_name.Text.Length == 0)
            {
                MessageBox.Show("Поля Имя, Фамилия, Отчество не должны быть пустыми!");
                return;
            }
            MySQLClient client = new MySQLClient(main_wnd.connectionString);
            client.UpdateAuthor(MySQLClient.SpecialChars(tb_family_name.Text), MySQLClient.SpecialChars(tb_name.Text), MySQLClient.SpecialChars(tb_middle_name.Text), auth_id);
            Close();
        }
    }
}
