using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for PageStudySearchLong.xaml
    /// </summary>
    public partial class PageStudySearchLong : Page
    {
        MainWindow main_wnd;
        string[] education_levels = { "", "Среднее профессиональное", "Высшее" };
        List<SimpleTableType> structureUnit_variants = new List<SimpleTableType>();
        public PageStudySearchLong(MainWindow mw)
        {
            main_wnd = mw;
            InitializeComponent();
            cb_type.ItemsSource = main_wnd.publication_types;
            cb_form.ItemsSource = main_wnd.publication_forms;
            cb_classification.ItemsSource = main_wnd.publication_classifications;
            cb_education_level.ItemsSource = education_levels;
            tb_structure_unit.Initialize(mw.connectionString, 0);//структурное подразделение
            tb_title.Initialize(mw.connectionString, 1); //поиск по title = 1
            tb_speciality.Initialize(mw.connectionString, 2); //speciality
            tb_discipline.Initialize(mw.connectionString, 3); //discipline
            tb_discipline_unit.Initialize(mw.connectionString, 4); //discipline_unit
            tb_author.Initialize(mw.connectionString, 5); //поиск по фамилии
        }

        private void ClearFilters(object sender, RoutedEventArgs e)
        {
            tb_title.Text = "";
            tb_author.Text = "";
            tb_author_name.Text = "";
            tb_discipline.Text = "";
            tb_discipline_unit.Text = "";
            tb_publication_date.Text = "";
            tb_speciality.Text = "";
            tb_structure_unit.Text = "";
            cb_classification.SelectedIndex = 0;
            cb_education_level.SelectedIndex = 0;
            cb_form.SelectedIndex = 0;
            cb_type.SelectedIndex = 0;
        }

        private void Find(object sender, RoutedEventArgs e)
        {
            string title = MySQLClient.SpecialChars(tb_title.Text);
            string author = MySQLClient.SpecialChars(tb_author.Text);
            string structure_unit = MySQLClient.SpecialChars(tb_structure_unit.Text);
            string author_name = MySQLClient.SpecialChars(tb_author_name.Text);
            string discipline = MySQLClient.SpecialChars(tb_discipline.Text);
            string discipline_unit = MySQLClient.SpecialChars(tb_discipline_unit.Text);
            string publication_date = MySQLClient.SpecialChars(tb_publication_date.Text);
            string speciality = MySQLClient.SpecialChars(tb_speciality.Text);

            if (title == "" && author == "" && structure_unit == "" && author_name == "" &&
                discipline == "" && discipline_unit == "" && publication_date == "" && speciality == "" &&
                cb_type.SelectedIndex < 1 && cb_classification.SelectedIndex < 1 && cb_education_level.SelectedIndex < 1 &&
                cb_form.SelectedIndex < 1)
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
                WHERE_expr += String.Format("study_publication.title like ('%{0}%')", title);
            }
            if (author != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                if (author_name == "")
                {
                    WHERE_expr += String.Format(@"study_publication.id in (select author_study_publication.publication_id 
from author_study_publication
join author on author.id = author_study_publication.author_id
where author.family_name like ('%{0}%'))", author);
                }
                else
                {
                    WHERE_expr += String.Format(@"study_publication.id in (select author_study_publication.publication_id 
from author_study_publication
join author on author.id = author_study_publication.author_id
where author.family_name like ('%{0}%') and author.name like ('%{1}%'))", author,
                    author_name);
                }
            }
            else if (author_name != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"study_publication.id in (select author_study_publication.publication_id 
from author_study_publication
join author on author.id = author_study_publication.author_id
where author.name like ('%{0}%'))", author_name);
            }
            if (cb_type.SelectedIndex > 0)
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format("study_publication.type in ({0})", Convert.ToString(((SimpleTableType)cb_type.SelectedItem).id));
            }
            if (structure_unit != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"study_publication.structure_unit in (select structure_unit.id from structure_unit 
where structure_unit.name like ('%{0}%'))", structure_unit);
            }
            if (speciality != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"study_publication.speciality in (select speciality.id from speciality 
where speciality.name like ('%{0}%'))", speciality);
            }
            if (discipline != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"study_publication.discipline in (select discipline.id from discipline 
where discipline.name like ('%{0}%'))", discipline);
            }
            if (discipline_unit != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"study_publication.discipline_unit like ('%{0}%')", discipline_unit);
            }
            if(cb_form.SelectedIndex > 0)
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format("study_publication.form in ({0})", Convert.ToString(((SimpleTableType)cb_form.SelectedItem).id));
            }
            if (cb_classification.SelectedIndex > 0)
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format("study_publication.classification in ({0})", Convert.ToString(((SimpleTableType)cb_classification.SelectedItem).id));
            }
            //2 = "Среднее профессиональное", 1 = "Высшее"
            if (cb_education_level.SelectedIndex > 0)
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                string id;
                if (Convert.ToString(cb_education_level.SelectedItem) == "Среднее профессиональное") id = "2";
                else id = "1";
                WHERE_expr += String.Format("study_publication.education_level in ({0})", id);
            }
            if (publication_date != "")
            {
                if (not_first) { WHERE_expr += " and "; }
                else not_first = true;
                WHERE_expr += String.Format(@"study_publication.publication_date like ('%{0}%')", publication_date);
            }
            int result = mySqlClient.GetStudyPublications(WHERE_expr, main_wnd.StudyBookPage.study_publications);
            if(result == 0)
            {
                popupNotFound.IsOpen = true;
            }
        }
    }
}
