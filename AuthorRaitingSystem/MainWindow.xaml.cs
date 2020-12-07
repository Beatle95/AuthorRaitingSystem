using AuthorRaitingSystem.Pages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AuthorRaitingSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsAdmin = false;
        public string User = "";
        public AuthorWindow authorWindow;
        public PageScienceBook ScienceBookPage;
        public PageStudyBook StudyBookPage;
        public PageStudySearchLong studySearchLong;
        public PageScienceSearchLong pageScienceSearchLong;
        public string connectionString = "";
        public List<SimpleTableType> publication_types;
        public List<SimpleTableType> science_publication_types;
        public List<SimpleTableType> publication_forms;
        public List<SimpleTableType> publication_classifications;
        public List<SimpleTableType> structure_units;
        public List<SimpleTableType> specialities;

        public delegate void EnterAdminHandler();
        public event EnterAdminHandler EnterAdminNotify;

        public delegate void LeaveAdminHandler();
        public event LeaveAdminHandler LeaveAdminNotify;

        public MainWindow()
        {
            if (!ReadConnectionOptions())
            {
                Close();
                return;
            }
            MySQLClient client = new MySQLClient(connectionString);
            publication_types = client.GetStudyPublicationTypes();
            science_publication_types = client.GetSciencePublicationTypes();
            publication_forms = client.GetPublicationForms();
            publication_classifications = client.GetPublicationClassifications();
            structure_units = client.GetStructureUnits();
            specialities = client.GetSpecialities();

            studySearchLong = new PageStudySearchLong(this);
            pageScienceSearchLong = new PageScienceSearchLong(this);

            ScienceBookPage = new PageScienceBook(this);
            StudyBookPage = new PageStudyBook(this);

            InitializeComponent();
            SetPreviliges();

            mainFrame.Navigate(StudyBookPage);
        }

        private void adminClick(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin)
            {
                LoginWindow loginWnd = new LoginWindow(this);
                loginWnd.ShowDialog();
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Вы уверены, что хотите выйти?", 
                    "Подтверждение выхода", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    User = "";
                    IsAdmin = false;
                    SetPreviliges();
                    adminMenuItem.Header = "Администрирование";
                    LeaveAdminNotify?.Invoke();
                }
            }
        }

        //Устанавливаем доступность клавишь редактирования
        public void SetPreviliges()
        {
            if (IsAdmin)
            {
                usersMenuItem.Visibility = Visibility.Visible;
                StudyBookPage.b_add.IsEnabled = true;
                StudyBookPage.b_delete.IsEnabled = true;
                StudyBookPage.b_edit.IsEnabled = true;
                ScienceBookPage.b_add.IsEnabled = true;
                ScienceBookPage.b_delete.IsEnabled = true;
                ScienceBookPage.b_edit.IsEnabled = true;
                l_admin.Visibility = Visibility.Visible;
            }
            else
            {
                usersMenuItem.Visibility = Visibility.Collapsed;
                StudyBookPage.b_add.IsEnabled = false;
                StudyBookPage.b_delete.IsEnabled = false;
                StudyBookPage.b_edit.IsEnabled = false;
                ScienceBookPage.b_add.IsEnabled = false;
                ScienceBookPage.b_delete.IsEnabled = false;
                ScienceBookPage.b_edit.IsEnabled = false;
                l_admin.Visibility = Visibility.Hidden;
            }
        }

        //Чтение файла настроек и создание строки соединения с БД
        private bool ReadConnectionOptions()
        {
            if (File.Exists("connection.cfg"))
            {
                string[] connectionInfo = new string[5];
                StreamReader sr = new StreamReader("connection.cfg");
                string line; int i = 0;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    line = line.Trim(' ');
                    if (line[0] != '#' && i < 5)
                    {
                        connectionInfo[i] = line;
                        i++;
                    }
                }
                connectionString = string.Format(@"server={0};port={1};database={2};user={3};password={4};connection timeout=120;", connectionInfo);
                try
                {
                    MySQLClient client = new MySQLClient(string.Format(@"server={0};port={1};user={3};password={4};connection timeout=120;", connectionInfo));
                    if (!client.IsDBExists(connectionInfo[2]))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("Структура базы данных не обнаружена на этом сервере. Создать ее сейчас?", "Создание структуры БД", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            if (client.CreateDB(connectionInfo[2]))
                                MessageBox.Show("Структура БД создана УСПЕШНО!");
                            else
                            {
                                MessageBox.Show("Структура БД НЕ БЫЛА СОЗДАНА!");
                                Close();
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                catch (Exception) { };
            }
            else
            {
                MessageBox.Show("Не найден файл настроек. Программу нужно будет перезапустить!");
                IsEnabled = false;
            }
            return true;
        }

        public void CallEnterAdminNotify()
        {
            EnterAdminNotify?.Invoke();
        }

        //Навигация через меню (Учебные)
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(StudyBookPage);
        }
        //Навигация через меню (научные)
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(ScienceBookPage);
        }
        //Навигация через меню (авторы)
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            authorWindow = new AuthorWindow(this);
            authorWindow.ShowDialog();
        }

        private void MainWnd_KeyPress(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.F1)
            {
                MenuItem_Click(this, new RoutedEventArgs());
            }
            else if(e.Key == Key.F2)
            {
                MenuItem_Click_1(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.F3)
            {
                MenuItem_Click_2(this, new RoutedEventArgs());
            }
        }

        private void AddUserClick(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUser = new AddUserWindow(this);
            addUser.ShowDialog();
        }

        private void deleteUser(object sender, RoutedEventArgs e)
        {
            if (User == "admin")
            {
                DeleteUser deleteUser = new DeleteUser(this);
                deleteUser.ShowDialog();
            }
            else
            {
                MessageBox.Show("Это доступно только для пользователя admin!");
            }
        }

        private void ChangePassword(object sender, RoutedEventArgs e)
        {
            AddUserWindow adduser = new AddUserWindow(this);
            adduser.tb_login.Text = User;
            adduser.tb_login.IsHitTestVisible = false;
            adduser.Title = "Смена пароля";
            adduser.ShowDialog();
        }
    }
}

