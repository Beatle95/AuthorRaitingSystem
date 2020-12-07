﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for PageScienceBookDetails.xaml
    /// </summary>
    public partial class PageScienceBookDetails : Page
    {
        MainWindow main_wnd;
        int index = 0;
        SciencePublication currentPublication;
        public ObservableCollection<Author> authors = new ObservableCollection<Author>();
        bool IsAuthorsEdited = false;
        public PageScienceBookDetails(MainWindow mw, int id, int indx)
        {
            MySQLClient client = new MySQLClient(mw.connectionString);

            InitializeComponent();
            main_wnd = mw;
            index = indx;

            main_wnd.EnterAdminNotify += EnterAdminEventHandler;
            main_wnd.LeaveAdminNotify += LeaveAdminEventHandler;

            if (main_wnd.IsAdmin)
            {
                b_edit.IsEnabled = true;
                b_delete.IsEnabled = true;
            }
            //Получаем подробные данные о выбранном объекте
            currentPublication = client.GetSciencePublicationDetails(id);
            authors = currentPublication.authors;

            dataGrid.ItemsSource = authors;
            //Данные в комбобоксах
            cb_type.ItemsSource = main_wnd.science_publication_types;
            cb_form.ItemsSource = main_wnd.publication_forms;
            cb_classification.ItemsSource = main_wnd.publication_classifications;
            cb_structure_unit.ItemsSource = main_wnd.structure_units;
            //Значения в контролах
            cb_type.Text = currentPublication.type;
            cb_form.Text = currentPublication.form;
            tb_title.Text = currentPublication.title;
            tb_part_number.Text = currentPublication.part_number;
            cb_classification.Text = currentPublication.classification;

            cb_structure_unit.Text = currentPublication.structure_unit;

            if (cb_form.SelectedItem.ToString() == "Печатное")
            {
                dp_signing_date.Text = currentPublication.signing_date;            //TODO: убрать время
                tb_paper_format.Text = currentPublication.paper_format;
                tb_presswork_count.Text = currentPublication.presswork_count;
                tb_formal_presswork_count.Text = currentPublication.formal_presswork_count;
                tb_publication_account_count.Text = currentPublication.publication_account_count;
            }
            else
            {
                tb_mb_count.Text = currentPublication.mb_count;
            }
            tb_publication_date.Text = currentPublication.publication_date;
            tb_publication_count.Text = currentPublication.publication_count;
            tb_publication_number.Text = currentPublication.publication_number;
            tb_order_number.Text = currentPublication.order_number;
            tb_review_number.Text = currentPublication.review_number;
            tb_publication_author_count.Text = currentPublication.publication_author_count;
            tb_publisher_name.Text = currentPublication.publisher_name;
            tb_publisher_address.Text = currentPublication.publisher_address;

            tb_udk.Text = currentPublication.udk;
            tb_bbk.Text = currentPublication.bbk;
            tb_issn.Text = currentPublication.issn;
            tb_isbn.Text = currentPublication.isbn;
            tb_asset_number.Text = currentPublication.asset_number;
        }

        public PageScienceBookDetails(MainWindow mw)
        {
            InitializeComponent();
            main_wnd = mw;
            tb_publisher_name.Initialize(main_wnd.connectionString, 8);
            tb_publisher_address.Initialize(main_wnd.connectionString, 7);
            main_wnd.EnterAdminNotify += EnterAdminEventHandler;
            main_wnd.LeaveAdminNotify += LeaveAdminEventHandler;

            if (main_wnd.IsAdmin)
            {
                b_edit.IsEnabled = true;
                b_delete.IsEnabled = true;
            }
            //Данные в комбобоксах
            cb_type.ItemsSource = main_wnd.science_publication_types;
            cb_form.ItemsSource = main_wnd.publication_forms;
            cb_classification.ItemsSource = main_wnd.publication_classifications;
            cb_structure_unit.ItemsSource = main_wnd.structure_units;

            dataGrid.ItemsSource = authors;

            //заполняем дефолтными значениями адрес и название издателя
            MySQLClient client = new MySQLClient(main_wnd.connectionString);
            string name, addr;
            client.GetDefaultPublisherData(out name, out addr);
            tb_publisher_name.Text = name;
            tb_publisher_address.Text = addr;
        }

        private void b_close_Click(object sender, RoutedEventArgs e)
        {
            main_wnd.mainFrame.Navigate(main_wnd.ScienceBookPage);
        }

        public void b_edit_Click(object sender, RoutedEventArgs e)
        {
            if (main_wnd.IsAdmin)
            {
                cb_type.IsHitTestVisible = true;
                cb_form.IsHitTestVisible = true;
                tb_title.IsReadOnly = false;
                tb_part_number.IsReadOnly = false;
                cb_classification.IsHitTestVisible = true;

                cb_structure_unit.IsHitTestVisible = true;

                dp_signing_date.IsHitTestVisible = true;
                tb_publication_date.IsReadOnly = false;
                tb_publication_count.IsReadOnly = false;
                tb_paper_format.IsReadOnly = false;
                tb_publication_number.IsReadOnly = false;
                tb_order_number.IsReadOnly = false;
                tb_review_number.IsReadOnly = false;
                tb_presswork_count.IsReadOnly = false;
                tb_formal_presswork_count.IsReadOnly = false;
                tb_publication_account_count.IsReadOnly = false;
                tb_publication_author_count.IsReadOnly = false;
                tb_mb_count.IsReadOnly = false;
                tb_publisher_name.IsReadOnly = false;
                tb_publisher_address.IsReadOnly = false;

                tb_udk.IsReadOnly = false;
                tb_bbk.IsReadOnly = false;
                tb_issn.IsReadOnly = false;
                tb_isbn.IsReadOnly = false;
                tb_asset_number.IsReadOnly = false;

                b_save.Visibility = Visibility.Visible;
                b_edit.Visibility = Visibility.Collapsed;
                b_Add.Visibility = Visibility.Visible;
                b_Delete.Visibility = Visibility.Visible;
                Background = new SolidColorBrush(Color.FromRgb(0xD6, 0xD6, 0xD6));
            }
            else
            {
                MessageBox.Show("Вам необходмо быть администратором, чтобы редактировать записи.");
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void b_save_Click(object sender, RoutedEventArgs e)
        {
            if (main_wnd.IsAdmin)
            {
                if (b_delete.Visibility == Visibility.Collapsed) //Если кнопка удалить скрыта, значит мы в режиме добавления нового элемента
                {
                    MySQLClient client = new MySQLClient(main_wnd.connectionString);
                    int ret = CheckAndSave(client, true);
                    if (ret > 0)
                    {
                        SciencePublication added = client.GetSciencePublicationDetails(ret);
                        if (added != null)
                        {
                            added.GenerateAuthors();
                            main_wnd.ScienceBookPage.science_publications.Add(added);
                        }
                        main_wnd.mainFrame.Navigate(main_wnd.ScienceBookPage);
                    }
                }
                else
                {
                    MySQLClient client = new MySQLClient(main_wnd.connectionString);
                    int ret = CheckAndSave(client, false);
                    if (ret > 0)
                    {
                        foreach (SciencePublication sp in main_wnd.ScienceBookPage.science_publications)
                        {
                            if (sp.id == currentPublication.id) { main_wnd.ScienceBookPage.science_publications.Remove(sp); break; }
                        }
                        SciencePublication edited = client.GetSciencePublicationDetails(ret);
                        if (edited != null)
                        {
                            edited.GenerateAuthors();
                            main_wnd.ScienceBookPage.science_publications.Add(edited);
                        }
                        main_wnd.mainFrame.Navigate(main_wnd.ScienceBookPage);
                    }
                }
            }
            else
            {
                MessageBox.Show("Вы должны быть администратором, чтобы сохранить изменения.");
            }
        }

        //Необходимо передать инициализированный client, в ответ получаем id добавленной записи или 0, в случае ошибки
        //второй параметр - bool, если истина, то создание нового элемента, иначе - редактирование
        private int CheckAndSave(MySQLClient client, bool create_new_element)
        {
            int type, form, classification, structure_unit = 0;
            string title = "", part_number = "", publication_date = "", publication_count = "", publication_number = "", order_number = "",
                review_number = "", publication_author_count = "", publisher_name = "", publisher_address = "", udk = "", bbk = "", issn = "", isbn = "", asset_number,
                signing_date = "", paper_format = "", presswork_count = "", formal_presswork_count = "", publication_account_count = "", mb_count = "";
            if (cb_type.SelectedIndex > 0) type = ((SimpleTableType)cb_type.SelectedValue).id;
            else { MessageBox.Show("Укажите вид издания!"); return 0; }
            if (cb_form.SelectedIndex > 0) form = ((SimpleTableType)cb_form.SelectedValue).id;
            else { MessageBox.Show("Укажите форму издания!"); return 0; }
            if (MySQLClient.SpecialChars(tb_title.Text) == "") { MessageBox.Show("Укажите название издания!"); return 0; }
            else title = MySQLClient.SpecialChars(tb_title.Text);
            if (MySQLClient.SpecialChars(tb_part_number.Text) == "") { MessageBox.Show("Укажите номер части!"); return 0; }
            else part_number = MySQLClient.SpecialChars(tb_part_number.Text);
            if (cb_classification.SelectedIndex > 0) classification = ((SimpleTableType)cb_classification.SelectedValue).id;
            else { MessageBox.Show("Укажите гриф издания!"); return 0; }
            if (type == 1) //Если выбрана монография
            {
                if (cb_structure_unit.SelectedIndex > 0) structure_unit = ((SimpleTableType)cb_structure_unit.SelectedValue).id;
                else { MessageBox.Show("Укажите структурное подразделение!"); return 0; }
            }
            //Выпускные данные
            publication_date = MySQLClient.SpecialChars(tb_publication_date.Text);
            publication_count = MySQLClient.SpecialChars(tb_publication_count.Text);
            if (publication_count == "") { publication_count = "0"; }
            publication_number = MySQLClient.SpecialChars(tb_publication_number.Text);
            order_number = MySQLClient.SpecialChars(tb_order_number.Text);
            review_number = MySQLClient.SpecialChars(tb_review_number.Text);
            publication_author_count = MySQLClient.SpecialChars(tb_publication_author_count.Text);
            if (MySQLClient.SpecialChars(tb_publisher_name.Text) == "") { MessageBox.Show("Укажите юридическое имя издателя!"); return 0; }
            else publisher_name = MySQLClient.SpecialChars(tb_publisher_name.Text);
            if (MySQLClient.SpecialChars(tb_publisher_address.Text) == "") { MessageBox.Show("Укажите юридический адрес издателя!"); return 0; }
            else publisher_address = MySQLClient.SpecialChars(tb_publisher_address.Text);
            //Индексы и номера
            udk = MySQLClient.SpecialChars(tb_udk.Text);
            bbk = MySQLClient.SpecialChars(tb_bbk.Text);
            issn = MySQLClient.SpecialChars(tb_issn.Text);
            isbn = MySQLClient.SpecialChars(tb_isbn.Text);
            asset_number = MySQLClient.SpecialChars(tb_asset_number.Text);
            //Добавляемые по условиям
            if (form == 1)
            {
                if (MySQLClient.SpecialChars(dp_signing_date.Text) != "")
                {
                    signing_date = MySQLClient.SpecialChars(dp_signing_date.Text);
                    string[] splited = signing_date.Split('.');
                    signing_date = splited[2] + "-" + splited[1] + "-" + splited[0];
                }
                paper_format = MySQLClient.SpecialChars(tb_paper_format.Text);
                presswork_count = MySQLClient.SpecialChars(tb_presswork_count.Text);
                formal_presswork_count = MySQLClient.SpecialChars(tb_formal_presswork_count.Text);
                publication_account_count = MySQLClient.SpecialChars(tb_publication_account_count.Text);
            }
            else
            {
                mb_count = MySQLClient.SpecialChars(tb_mb_count.Text);
            }
            if (signing_date == "") signing_date = "0001-01-01";
            if (create_new_element)
            {
                //Дальше передаем все данные для сохранения в MySqlClient
                int ret = client.InsertSciencePublication(type, form, classification, structure_unit, title, part_number, publication_date, publication_count, publication_number, order_number,
                    review_number, publication_author_count, publisher_name, publisher_address, udk, bbk, issn, isbn, asset_number,
                    signing_date, paper_format, presswork_count, formal_presswork_count, publication_account_count, mb_count, authors);
                return ret;
            }
            else
            {
                int ret = client.UpdateSciencePublication(currentPublication.id, type, form, classification, structure_unit, title, part_number, publication_date, publication_count, publication_number, order_number,
                    review_number, publication_author_count, publisher_name, publisher_address, udk, bbk, issn, isbn, asset_number,
                    signing_date, paper_format, presswork_count, formal_presswork_count, publication_account_count, mb_count, IsAuthorsEdited, authors);
                return ret;
            }
        }

            //Обработчики событий входа и выхода в/из режима админа
        public void EnterAdminEventHandler()
        {
            b_edit.IsEnabled = true;
            b_delete.IsEnabled = true;
        }
        public void LeaveAdminEventHandler()
        {
            b_edit.IsEnabled = false;
            b_delete.IsEnabled = false;
        }

        private void SetMonographyVisibility(Visibility visual)
        {
            lAuthors.Visibility = visual;
            dataGrid.Visibility = visual;
            sp_Purpose_label.Visibility = visual;
            sp_Purpose_textBox.Visibility = visual;
            lPurpose.Visibility = visual;
            lStructureUnit.Visibility = visual;
            cb_structure_unit.Visibility = visual;
            sp_Buttons.Visibility = visual;
        }

        private void SetControlsVisibility(Visibility visual)
        {
            dp_signing_date.Visibility = visual;
            l_signing_date.Visibility = visual;
            tb_paper_format.Visibility = visual;
            l_paper_format.Visibility = visual;
            tb_presswork_count.Visibility = visual;
            l_presswork_count.Visibility = visual;
            tb_formal_presswork_count.Visibility = visual;
            l_formal_presswork_count.Visibility = visual;
            tb_publication_account_count.Visibility = visual;
            l_publication_account_count.Visibility = visual;
            tb_mb_count.Visibility = visual;
            l_mb_count.Visibility = visual;
            if (visual == Visibility.Visible)
            {
                l_mb_count.Visibility = Visibility.Collapsed;
                tb_mb_count.Visibility = Visibility.Collapsed;
            }
            else
            {
                l_mb_count.Visibility = Visibility.Visible;
                tb_mb_count.Visibility = Visibility.Visible;
            }
        }

        private void cb_form_Update(object sender, SelectionChangedEventArgs e)
        {
            if (cb_form.SelectedItem.ToString() == "Печатное")
                SetControlsVisibility(Visibility.Visible);
            else
                SetControlsVisibility(Visibility.Collapsed);
        }
        
        
        private void cb_type_Update(object sender, SelectionChangedEventArgs e)
        {
            if (cb_type.SelectedItem.ToString() == "Монография")
                SetMonographyVisibility(Visibility.Visible);
            else
                SetMonographyVisibility(Visibility.Collapsed);
        }

        private void b_Delete_Pressed(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedIndex != -1)
            {
                IsAuthorsEdited = true;
                authors.Remove((Author)dataGrid.SelectedItem);
            }
        }

        private void _KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                main_wnd.mainFrame.Navigate(main_wnd.ScienceBookPage);
            }
        }

        //Удалить текущий элемент
        private void bDeleteClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить элемент?", "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MySQLClient client = new MySQLClient(main_wnd.connectionString);
                client.DeleteSciencePublicationById(currentPublication.id);
                main_wnd.ScienceBookPage.science_publications.RemoveAt(index);
                main_wnd.mainFrame.Navigate(main_wnd.ScienceBookPage);
            }
        }

        //Добавить авторов
        private void addAuthorClick(object sender, RoutedEventArgs e)
        {
            AddAuthors AddAuthorsWnd = new AddAuthors(main_wnd, this);
            AddAuthorsWnd.ShowDialog();
            if (AddAuthorsWnd.DialogResult == true)
            {
                IsAuthorsEdited = true;
                authors.Clear();
                foreach (Author a in AddAuthorsWnd.selected_authors)
                {
                    authors.Add(a);
                }
            }
        }
    }
}
