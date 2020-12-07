using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AuthorRaitingSystem
{
    public class StudyPublication
    {
        public StudyPublication()
        {
            authors = new ObservableCollection<Author>();
        }
        public int id { get; set; }
        public string type { get; set; }
        public string form { get; set; }
        public string title { get; set; }
        public string classification { get; set; }
        public string structure_unit { get; set; }
        public string speciality { get; set; }
        public string education_level { get; set; }
        public string discipline { get; set; }
        public string discipline_unit { get; set; }
        public string signing_date { get; set; }
        public string publication_date { get; set; }
        public string publication_count { get; set; }
        public string paper_format { get; set; }
        public string publication_number { get; set; }
        public string order_number { get; set; }
        public string review_number { get; set; }
        public string presswork_count { get; set; }
        public string formal_presswork_count { get; set; }
        public string publication_account_count { get; set; }
        public string publication_author_count { get; set; }
        public string mb_count { get; set; }
        public string publisher_name { get; set; }
        public string publisher_address { get; set; }
        public string udk { get; set; }
        public string bbk { get; set; }
        public string issn { get; set; }
        public string isbn { get; set; }
        public string asset_number { get; set; }
        public string s_authors { get; set; }
        public ObservableCollection<Author> authors { get; set; }

        public void GenerateAuthors()
        {
            s_authors = "";
            bool result = false;
            foreach(Author a in authors)
            {
                result = true;
                s_authors += String.Format("{0} {1} {2}, ", a.family_name, a.name, a.middle_name);
            }
            if (result) s_authors = s_authors.Remove(s_authors.Length - 2, 2);
            else s_authors = "Не указан";
        }
    }
}
