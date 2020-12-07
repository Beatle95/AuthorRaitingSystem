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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AuthorRaitingSystem.Pages
{
    /// <summary>
    /// Interaction logic for PageScienceSearchLong.xaml
    /// </summary>
    public partial class PageScienceSearchLong : Page
    {
        MainWindow main_wnd;
        string[] education_levels = { "", "Среднее профессиональное", "Высшее" };
        public PageScienceSearchLong(MainWindow mw)
        {
            main_wnd = mw;
            InitializeComponent();
            cb_type.ItemsSource = main_wnd.science_publication_types;
            cb_form.ItemsSource = main_wnd.publication_forms;
            cb_classification.ItemsSource = main_wnd.publication_classifications;
            tb_structure_unit.Initialize(mw.connectionString, 0);//структурное подразделение
            tb_title.Initialize(mw.connectionString, 6); //поиск по title = 6
            tb_author.Initialize(mw.connectionString, 5); //поиск по фамилии
        }

        private void ClearFilters(object sender, RoutedEventArgs e)
        {
            tb_title.Text = "";
            tb_author.Text = "";
            tb_publication_date.Text = "";
            tb_structure_unit.Text = "";
            cb_classification.SelectedIndex = 0;
            cb_form.SelectedIndex = 0;
            cb_type.SelectedIndex = 0;
        }

        private void Find(object sender, RoutedEventArgs e)
        {
            string title = MySQLClient.SpecialChars(tb_title.Text);
            string author = MySQLClient.SpecialChars(tb_author.Text);
            string author_name = MySQLClient.SpecialChars(tb_author_name.Text);
            string structure_unit = MySQLClient.SpecialChars(tb_structure_unit.Text);
            string publication_date = MySQLClient.SpecialChars(tb_publication_date.Text);

            if (title == "" && author == "" && structure_unit == "" && publication_date == "" && author_name == "" &&
                cb_type.SelectedIndex < 1 && cb_classification.SelectedIndex < 1 && cb_form.SelectedIndex < 1)
            {
                popupNoFilters.IsOpen = true;
                return;
            }

            MySQLClient mySqlClient;
            string WHERE_expr = "";
            bool not_first = false;
            mySqlClient = new MySQLClient(main_wnd.connectionString);
            if (title != "")
            {
                not_first = true;
                WHERE_expr += String.Format("science_publication.title like ('%{0}%')", title);
            }
            if (author != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                if (author_name == "")
                {
                    WHERE_expr += String.Format(@"science_publication.id in (select author_science_publication.publication_id 
from author_science_publication
join author on author.id = author_science_publication.author_id
where author.family_name like ('%{0}%'))", author);
                }
                else
                {
                    WHERE_expr += String.Format(@"science_publication.id in (select author_science_publication.publication_id 
from author_science_publication
join author on author.id = author_science_publication.author_id
where author.family_name like ('%{0}%') and author.name like ('%{1}%'))", author,
                    author_name);
                }
            }
            else if (author_name != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"science_publication.id in (select author_science_publication.publication_id 
from author_science_publication
join author on author.id = author_science_publication.author_id
where author.name like ('%{0}%'))", author_name);
            }
            if (cb_type.SelectedIndex > 0)
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format("science_publication.type in ({0})", Convert.ToString(((SimpleTableType)cb_type.SelectedItem).id));
            }
            if (structure_unit != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"science_publication.structure_unit in (select structure_unit.id from structure_unit 
where structure_unit.name like ('%{0}%'))", structure_unit);
            }
            if (cb_form.SelectedIndex > 0)
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format("science_publication.form in ({0})", Convert.ToString(((SimpleTableType)cb_form.SelectedItem).id));
            }
            if (cb_classification.SelectedIndex > 0)
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format("science_publication.classification in ({0})", Convert.ToString(((SimpleTableType)cb_classification.SelectedItem).id));
            }
            if (publication_date != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"science_publication.publication_date like ('%{0}%')", publication_date);
            }
            int result = mySqlClient.GetSciencePublications(WHERE_expr, main_wnd.ScienceBookPage.science_publications);
            if (result == 0)
            {
                popupNotFound.IsOpen = true;
            }
        }
    }
}
