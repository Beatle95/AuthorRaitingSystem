using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;

namespace AuthorRaitingSystem
{
    //Используем этот класс для работы с mySQL
    //Конструктор устанавливает соединение с базой по переданой сроке соединения
    //Деструктор пытается закрыть соединение
    class MySQLClient
    {
        MySqlConnection UserSQLconnection;
        public MySQLClient(string connectionString)
        {
            UserSQLconnection = new MySqlConnection(connectionString);
            Thread.Sleep(100);
            try
            {
                UserSQLconnection.Open();
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        ~MySQLClient()
        {
            try
            {
                UserSQLconnection.Close();
            }
            catch (Exception) { }
        }

        //Сохранение нового элемента StudyPublication возвращает 0 - ошибка; или id нового элемента в случае успеха.
        public int InsertStudyPublication(int type, int form, int classification, int structure_unit, int speciality, int education_level,
                    string title, string discipline, string discipline_unit, string publication_date, string publication_count, string publication_number, string order_number,
                    string review_number, string publication_author_count, string publisher_name, string publisher_address, string udk, string bbk, string issn, string isbn, string asset_number,
                    string signing_date, string paper_format, string presswork_count, string formal_presswork_count, string publication_account_count, string mb_count, ObservableCollection<Author> authors)
        {
            int discipline_id = CheckValueExistence("discipline", "name", discipline);
            if(discipline_id == 0)
            {   //В случае если такой дисциплины нет в списке, добавляем новую и получаем ее ID
                try 
                {
                    string command = string.Format(@"insert into discipline (name) values ('{0}');", discipline);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    discipline_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            int publisher_name_id = CheckValueExistence("publisher_name", "name", publisher_name);
            if (publisher_name_id == 0)
            {   //Проверка для юридического имени автора, если нет в таблице - добавляем
                try
                {
                    string command = string.Format(@"insert into publisher_name (name) values ('{0}');", publisher_name);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    publisher_name_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            int publisher_address_id = CheckValueExistence("publisher_address", "name", publisher_address);
            if (publisher_address_id == 0)
            {   //Проверка для юридического адреса автора, если нет в таблице - добавляем
                try
                {
                    string command = string.Format(@"insert into publisher_address (name) values ('{0}');", publisher_address);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    publisher_address_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }

            string CommandText = string.Format(@"insert into study_publication (type, form, title, classification, structure_unit,
speciality, education_level, discipline, discipline_unit, signing_date, publication_date, publication_count, paper_format, publication_number,
order_number, review_number, presswork_count, formal_presswork_count, publication_account_count, publication_author_count, mb_count, publisher_name,
publisher_address, udk, bbk, issn, isbn, asset_number)
values ({0}, {1}, '{2}', {3}, {4}, {5}, {6}, {7}, '{8}', '{9}', '{10}', {11}, '{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}', 
'{20}', {21}, {22},'{23}','{24}','{25}','{26}','{27}');", type, form, title, classification, structure_unit, speciality, education_level,
discipline_id, discipline_unit, signing_date, publication_date, publication_count, paper_format, publication_number, order_number, review_number,
presswork_count, formal_presswork_count, publication_account_count, publication_author_count, mb_count, publisher_name_id, publisher_address_id,
udk, bbk, issn, isbn, asset_number);
            int inserted_id = 0;//Храним ID внесенной записи
            try
            {   //Вносим все данные в новую запись в БД
                MySqlCommand insert_publication = new MySqlCommand(CommandText, UserSQLconnection);
                var result = insert_publication.ExecuteNonQuery();
                inserted_id = Convert.ToInt32(insert_publication.LastInsertedId);
                if (result != 1)
                    return 0;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            //Создаем связи с авторами
            foreach(Author author in authors)
            {
                string cmd = string.Format(@"insert into author_study_publication (author_id, publication_id)
values ({0}, {1})", author.id, inserted_id);
                try
                {   //сохраняем связи с авторами
                    MySqlCommand ins = new MySqlCommand(cmd, UserSQLconnection);
                    var result = ins.ExecuteNonQuery();
                    if (result != 1)
                        return 0;
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            return inserted_id;
        }

        //Сохранение нового элемента StudyPublication возвращает 0 - ошибка; или id нового элемента в случае успеха.
        public int UpdateStudyPublication(int id, int type, int form, int classification, int structure_unit, int speciality, int education_level,
                    string title, string discipline, string discipline_unit, string publication_date, string publication_count, string publication_number, string order_number,
                    string review_number, string publication_author_count, string publisher_name, string publisher_address, string udk, string bbk, string issn, string isbn, string asset_number,
                    string signing_date, string paper_format, string presswork_count, string formal_presswork_count, string publication_account_count, string mb_count, 
                    bool IsAuthorsEdited, ObservableCollection<Author> authors)
        {
            int discipline_id = CheckValueExistence("discipline", "name", discipline);
            if (discipline_id == 0)
            {   //В случае если такой дисциплины нет в списке, добавляем новую и получаем ее ID
                try
                {
                    string command = string.Format(@"insert into discipline (name) values ('{0}');", discipline);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    discipline_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            int publisher_name_id = CheckValueExistence("publisher_name", "name", publisher_name);
            if (publisher_name_id == 0)
            {   //Проверка для юридического имени автора, если нет в таблице - добавляем
                try
                {
                    string command = string.Format(@"insert into publisher_name (name) values ('{0}');", publisher_name);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    publisher_name_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            int publisher_address_id = CheckValueExistence("publisher_address", "name", publisher_address);
            if (publisher_address_id == 0)
            {   //Проверка для юридического адреса автора, если нет в таблице - добавляем
                try
                {
                    string command = string.Format(@"insert into publisher_address (name) values ('{0}');", publisher_address);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    publisher_address_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }

            string CommandText = string.Format(@"update study_publication set type={0}, form={1}, title='{2}', classification={3}, structure_unit={4},
speciality={5}, education_level={6}, discipline={7}, discipline_unit='{8}', signing_date='{9}', publication_date='{10}', publication_count={11}, paper_format='{12}', 
publication_number='{13}', order_number='{14}', review_number='{15}', presswork_count='{16}', formal_presswork_count='{17}', publication_account_count='{18}', 
publication_author_count='{19}', mb_count='{20}', publisher_name={21}, publisher_address={22}, udk='{23}', bbk='{24}', issn='{25}', isbn='{26}', asset_number='{27}'
where id={28};", 
type, form, title, classification, structure_unit, speciality, education_level,
discipline_id, discipline_unit, signing_date, publication_date, publication_count, paper_format, publication_number, order_number, review_number,
presswork_count, formal_presswork_count, publication_account_count, publication_author_count, mb_count, publisher_name_id, publisher_address_id,
udk, bbk, issn, isbn, asset_number, id);
            try
            {   //Вносим все данные в новую запись в БД
                MySqlCommand insert_publication = new MySqlCommand(CommandText, UserSQLconnection);
                var result = insert_publication.ExecuteNonQuery();
                if (result != 1)
                    return 0;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            //Если авторы были изменены
            if (IsAuthorsEdited)
            {
                //Удаляем всех связанных авторов и прописываем новых
                CommandText = String.Format("delete from author_study_publication where publication_id={0};", id);
                try
                {
                    MySqlCommand delete = new MySqlCommand(CommandText, UserSQLconnection);
                    delete.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
                //Добавляем всех новых авторов
                foreach (Author author in authors)
                {
                    string cmd = string.Format(@"insert into author_study_publication (author_id, publication_id)
values ({0}, {1})", author.id, id);
                    try
                    {   //сохраняем связи с авторами
                        MySqlCommand ins = new MySqlCommand(cmd, UserSQLconnection);
                        var result = ins.ExecuteNonQuery();
                        if (result != 1)
                            return 0;
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
                }
            }
            return id;
        }

        //Сохранение нового элемента SciencePublication возвращает 0 - ошибка; или id нового элемента в случае успеха.
        public int InsertSciencePublication(int type, int form, int classification, int structure_unit,
                    string title, string part_number, string publication_date, string publication_count, string publication_number, string order_number,
                    string review_number, string publication_author_count, string publisher_name, string publisher_address, string udk, string bbk, string issn, string isbn, string asset_number,
                    string signing_date, string paper_format, string presswork_count, string formal_presswork_count, string publication_account_count, string mb_count, ObservableCollection<Author> authors)
        {
            int publisher_name_id = CheckValueExistence("publisher_name", "name", publisher_name);
            if (publisher_name_id == 0)
            {   //Проверка для юридического имени автора, если нет в таблице - добавляем
                try
                {
                    string command = string.Format(@"insert into publisher_name (name) values ('{0}');", publisher_name);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    publisher_name_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            int publisher_address_id = CheckValueExistence("publisher_address", "name", publisher_address);
            if (publisher_address_id == 0)
            {   //Проверка для юридического адреса автора, если нет в таблице - добавляем
                try
                {
                    string command = string.Format(@"insert into publisher_address (name) values ('{0}');", publisher_address);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    publisher_address_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            if (type != 1) structure_unit = 40;
            string CommandText = string.Format(@"insert into science_publication (type, form, title, part_number, classification, structure_unit,
signing_date, publication_date, publication_count, paper_format, publication_number, order_number, review_number, presswork_count, formal_presswork_count,
publication_account_count, publication_author_count, mb_count, publisher_name, publisher_address, udk, bbk, issn, isbn, asset_number)
values ({0},{1},'{2}','{3}',{4},{5},'{6}','{7}',{8},'{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}',{18},{19},
'{20}','{21}','{22}','{23}','{24}');", type, form, title, part_number, classification, structure_unit, 
signing_date, publication_date, publication_count, paper_format, publication_number, order_number, review_number,
presswork_count, formal_presswork_count, publication_account_count, publication_author_count, mb_count, publisher_name_id, publisher_address_id,
udk, bbk, issn, isbn, asset_number);
            int inserted_id = 0;//Храним ID внесенной записи
            try
            {   //Вносим все данные в новую запись в БД
                MySqlCommand insert_publication = new MySqlCommand(CommandText, UserSQLconnection);
                var result = insert_publication.ExecuteNonQuery();
                inserted_id = Convert.ToInt32(insert_publication.LastInsertedId);
                if (result != 1)
                    return 0;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            if (type == 1)
            {
                //Создаем связи с авторами
                foreach (Author author in authors)
                {
                    string cmd = string.Format(@"insert into author_science_publication (author_id, publication_id)
values ({0}, {1})", author.id, inserted_id);
                    try
                    {   //сохраняем связи с авторами
                        MySqlCommand ins = new MySqlCommand(cmd, UserSQLconnection);
                        var result = ins.ExecuteNonQuery();
                        if (result != 1)
                            return 0;
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
                }
            }
            return inserted_id;
        }

        //Сохранение нового элемента SciencePublication возвращает 0 - ошибка; или id нового элемента в случае успеха.
        public int UpdateSciencePublication(int id, int type, int form, int classification, int structure_unit,
                    string title, string part_number, string publication_date, string publication_count, string publication_number, string order_number,
                    string review_number, string publication_author_count, string publisher_name, string publisher_address, string udk, string bbk, string issn, string isbn, string asset_number,
                    string signing_date, string paper_format, string presswork_count, string formal_presswork_count, string publication_account_count, string mb_count, 
                    bool IsAuthorsEdited, ObservableCollection<Author> authors)
        {
            int publisher_name_id = CheckValueExistence("publisher_name", "name", publisher_name);
            if (publisher_name_id == 0)
            {   //Проверка для юридического имени автора, если нет в таблице - добавляем
                try
                {
                    string command = string.Format(@"insert into publisher_name (name) values ('{0}');", publisher_name);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    publisher_name_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            int publisher_address_id = CheckValueExistence("publisher_address", "name", publisher_address);
            if (publisher_address_id == 0)
            {   //Проверка для юридического адреса автора, если нет в таблице - добавляем
                try
                {
                    string command = string.Format(@"insert into publisher_address (name) values ('{0}');", publisher_address);
                    MySqlCommand insert = new MySqlCommand(command, UserSQLconnection);
                    int ret_val = insert.ExecuteNonQuery();
                    publisher_address_id = Convert.ToInt32(insert.LastInsertedId);
                    if (ret_val != 1)
                        return 0;

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            }
            if (type != 1) structure_unit = 40;
            string CommandText = string.Format(@"update science_publication set type={0}, form={1}, title='{2}', part_number='{3}', classification={4}, structure_unit={5},
signing_date='{6}', publication_date='{7}', publication_count={8}, paper_format='{9}', publication_number='{10}', order_number='{11}', review_number='{12}', presswork_count='{13}', 
formal_presswork_count='{14}', publication_account_count='{15}', publication_author_count='{16}', mb_count='{17}', publisher_name={18}, publisher_address={19}, udk='{20}', 
bbk='{21}', issn='{22}', isbn='{23}', asset_number='{24}' where id={25};", type, form, title, part_number, classification, structure_unit,
signing_date, publication_date, publication_count, paper_format, publication_number, order_number, review_number,
presswork_count, formal_presswork_count, publication_account_count, publication_author_count, mb_count, publisher_name_id, publisher_address_id,
udk, bbk, issn, isbn, asset_number, id);
            try
            {   //Вносим все данные в новую запись в БД
                MySqlCommand insert_publication = new MySqlCommand(CommandText, UserSQLconnection);
                var result = insert_publication.ExecuteNonQuery();
                if (result != 1)
                    return 0;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }

            if (type == 1 && IsAuthorsEdited)//Если это монография и авторы менялись
            {
                //Удаляем всех связанных авторов и прописываем новых
                CommandText = String.Format("delete from author_science_publication where publication_id={0};", id);
                try
                {
                    MySqlCommand delete = new MySqlCommand(CommandText, UserSQLconnection);
                    delete.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
                //Создаем связи с авторами
                foreach (Author author in authors)
                {
                    string cmd = string.Format(@"insert into author_science_publication (author_id, publication_id)
values ({0}, {1})", author.id, id);
                    try
                    {   //сохраняем связи с авторами
                        MySqlCommand ins = new MySqlCommand(cmd, UserSQLconnection);
                        var result = ins.ExecuteNonQuery();
                        if (result != 1)
                            return 0;
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
                }
            }
            return id;
        }

        public bool CheckAuthor(string family_name, string name, string middle_name, out Author ret_author)
        {
            ret_author = new Author();
            string CommandText = string.Format(@"select * from author where family_name='{0}' and name='{1}' and middle_name='{2}';", family_name, name, middle_name);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                if (reader.Read())
                {
                    ret_author.id = Convert.ToInt32(reader["id"]);
                    ret_author.family_name = Convert.ToString(reader["family_name"]);
                    ret_author.name = Convert.ToString(reader["name"]);
                    ret_author.middle_name = Convert.ToString(reader["middle_name"]);
                    return false;
                }
                reader.Close();
            }
            catch (Exception) { }
            return true;
        }

        public int InsertAuthor(string family_name, string name, string middle_name)
        {
            string CommandText = string.Format(@"insert into author (family_name, name, middle_name)
values ('{0}', '{1}', '{2}');", family_name, name, middle_name);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                var result = select.ExecuteNonQuery();
                if (result != 1)
                {
                    return 0;
                }
            }
            catch (Exception) { return 0; }
            return 1;
        }

        public int UpdateAuthor(string family_name, string name, string middle_name, long id)
        {
            string CommandText = string.Format(@"update author set family_name='{0}', name='{1}', middle_name='{2}' where id={3};", family_name, name, middle_name, id);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                var result = select.ExecuteNonQuery();
                if (result != 1)
                {
                    return 0;
                }
            }
            catch (Exception ex) { return 0; }
            return 1;
        }

        //Возвращает коллекцию авторов, соответствующую запросу 'SELECT * FROM authors WHERE {0};'
        //Где {0} - это WHERE_expr
        //return 1 - успешно; 0 - ошибка обмена данных; -1 - неинициализированная коллекция; -2 - поиск не дал результатов
        public int GetAuthors(string WHERE_expr, ObservableCollection<Author> authors_return)
        {
            string CommandText = string.Format(@"select * from author where {0};", WHERE_expr);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                //Если был получен результат заполняем поля объекта и возвращаем его
                if (authors_return == null) return -1;
                authors_return.Clear();
                while (reader.Read())
                {
                    authors_return.Add(new Author { 
                    id = Convert.ToInt32(reader["id"]),
                        family_name = Convert.ToString(reader["family_name"]),
                        name = Convert.ToString(reader["name"]),
                        middle_name = Convert.ToString(reader["middle_name"])
                    });
                }
                return 1;
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); return 0; }
        }

        //Возвращает подробную информацию по id издания
        public StudyPublication GetStudyPublicationDetails(int id)
        {
            StudyPublication sp = new StudyPublication();
            string CommandText = string.Format(@"select t1.id, t2.name as type, t3.name as form, t1.title, t4.name as classification,
t5.name as structure_unit, t5.number as number, t6.name as speciality, t1.education_level,
t7.name as discipline, t1.discipline_unit, t1.signing_date, t1.publication_date,
t1.publication_count, t1.paper_format, t1.publication_number, t1.order_number,
t1.review_number, t1.presswork_count, t1.formal_presswork_count, t1.publication_account_count,
t1.publication_author_count, t1.mb_count, t8.name as publisher_name, t9.name as publisher_address,
t1.udk, t1.bbk, t1.issn, t1.isbn, t1.asset_number
from study_publication              as t1
join study_publication_type         as t2 on t2.id = t1.type
join publication_form               as t3 on t3.id = t1.form
join publication_classification     as t4 on t4.id = t1.classification
join structure_unit                 as t5 on t5.id = t1.structure_unit
join speciality                     as t6 on t6.id = t1.speciality
join discipline                     as t7 on t7.id = t1.discipline
join publisher_name                 as t8 on t8.id = t1.publisher_name
join publisher_address              as t9 on t9.id = t1.publisher_address
where t1.id={0}
limit 100;", Convert.ToString(id));
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);                
                MySqlDataReader reader = select.ExecuteReader();
                //Если был получен результат заполняем поля объекта и возвращаем его
                if (reader.Read())
                {
                    string number = Convert.ToString(reader["number"]);
                    sp.id = Convert.ToInt32(reader["id"]);
                    sp.type = Convert.ToString(reader["type"]);
                    sp.form = Convert.ToString(reader["form"]);
                    sp.title = Convert.ToString(reader["title"]);
                    sp.classification = Convert.ToString(reader["classification"]);
                    sp.structure_unit = string.Format("({0}) {1}", number, Convert.ToString(reader["structure_unit"]));
                    sp.speciality = Convert.ToString(reader["speciality"]);
                    if (Convert.ToInt32(reader["education_level"]) == 1) { sp.education_level = "Среднее профессиональное"; }
                    else if (Convert.ToInt32(reader["education_level"]) == 2) { sp.education_level = "Высшее"; }
                    else { sp.education_level = "Не указано"; }
                    sp.discipline = Convert.ToString(reader["discipline"]);
                    sp.discipline_unit = Convert.ToString(reader["discipline_unit"]);
                    sp.signing_date = Convert.ToString(reader["signing_date"]);
                    sp.publication_date = Convert.ToString(reader["publication_date"]);
                    sp.publication_count = Convert.ToString(reader["publication_count"]);
                    sp.paper_format = Convert.ToString(reader["paper_format"]);
                    sp.publication_number = Convert.ToString(reader["publication_number"]);
                    sp.order_number = Convert.ToString(reader["order_number"]);
                    sp.review_number = Convert.ToString(reader["review_number"]);
                    sp.presswork_count = Convert.ToString(reader["presswork_count"]);
                    sp.formal_presswork_count = Convert.ToString(reader["formal_presswork_count"]);
                    sp.publication_account_count = Convert.ToString(reader["publication_account_count"]);
                    sp.publication_author_count = Convert.ToString(reader["publication_author_count"]);
                    sp.mb_count = Convert.ToString(reader["mb_count"]);
                    sp.publisher_name = Convert.ToString(reader["publisher_name"]);
                    sp.publisher_address = Convert.ToString(reader["publisher_address"]);
                    sp.udk = Convert.ToString(reader["udk"]);
                    sp.bbk = Convert.ToString(reader["bbk"]);
                    sp.issn = Convert.ToString(reader["issn"]);
                    sp.isbn = Convert.ToString(reader["isbn"]);
                    sp.asset_number = Convert.ToString(reader["asset_number"]);
                }
                else
                {
                    return null;
                }

                //Получаем авторов
                CommandText = string.Format(@"select asp.publication_id, author.id as author_id, author.family_name, author.name, author.middle_name
from author_study_publication as asp
join author on author.id=asp.author_id
where asp.publication_id in ({0});", Convert.ToInt32(id));
                reader.Close();
                MySqlCommand select_authors = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader_authors = select_authors.ExecuteReader();
                while (reader_authors.Read())
                {
                    sp.authors.Add(new Author
                    {
                        id = Convert.ToInt32(reader_authors["author_id"]),
                        family_name = Convert.ToString(reader_authors["family_name"]),
                        name = Convert.ToString(reader_authors["name"]),
                        middle_name = Convert.ToString(reader_authors["middle_name"])
                    });
                }
                reader_authors.Close();
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
            return sp;
        }

        //Возвращает подробную информацию по id издания
        public SciencePublication GetSciencePublicationDetails(int id)
        {
            SciencePublication sp = new SciencePublication();
            string CommandText = string.Format(@"select t1.id, t2.name as type, t3.name as form, t1.title, t4.name as classification,
t5.name as structure_unit, t5.number as number, t1.signing_date, t1.publication_date, t1.part_number,
t1.publication_count, t1.paper_format, t1.publication_number, t1.order_number,
t1.review_number, t1.presswork_count, t1.formal_presswork_count, t1.publication_account_count,
t1.publication_author_count, t1.mb_count, t8.name as publisher_name, t9.name as publisher_address,
t1.udk, t1.bbk, t1.issn, t1.isbn, t1.asset_number
from science_publication            as t1
join science_publication_type       as t2 on t2.id = t1.type
join publication_form               as t3 on t3.id = t1.form
join publication_classification     as t4 on t4.id = t1.classification
join structure_unit                 as t5 on t5.id = t1.structure_unit
join publisher_name                 as t8 on t8.id = t1.publisher_name
join publisher_address              as t9 on t9.id = t1.publisher_address
where t1.id={0}
limit 100;", Convert.ToString(id));
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                //Если был получен результат заполняем поля объекта и возвращаем его
                if (reader.Read())
                {
                    string number = Convert.ToString(reader["number"]);
                    sp.id = Convert.ToInt32(reader["id"]);
                    sp.type = Convert.ToString(reader["type"]);
                    sp.form = Convert.ToString(reader["form"]);
                    sp.title = Convert.ToString(reader["title"]);
                    sp.part_number = Convert.ToString(reader["part_number"]);
                    sp.classification = Convert.ToString(reader["classification"]);
                    sp.structure_unit = string.Format("({0}) {1}", number, Convert.ToString(reader["structure_unit"]));
                    sp.signing_date = Convert.ToString(reader["signing_date"]);
                    sp.publication_date = Convert.ToString(reader["publication_date"]);
                    sp.publication_count = Convert.ToString(reader["publication_count"]);
                    sp.paper_format = Convert.ToString(reader["paper_format"]);
                    sp.publication_number = Convert.ToString(reader["publication_number"]);
                    sp.order_number = Convert.ToString(reader["order_number"]);
                    sp.review_number = Convert.ToString(reader["review_number"]);
                    sp.presswork_count = Convert.ToString(reader["presswork_count"]);
                    sp.formal_presswork_count = Convert.ToString(reader["formal_presswork_count"]);
                    sp.publication_account_count = Convert.ToString(reader["publication_account_count"]);
                    sp.publication_author_count = Convert.ToString(reader["publication_author_count"]);
                    sp.mb_count = Convert.ToString(reader["mb_count"]);
                    sp.publisher_name = Convert.ToString(reader["publisher_name"]);
                    sp.publisher_address = Convert.ToString(reader["publisher_address"]);
                    sp.udk = Convert.ToString(reader["udk"]);
                    sp.bbk = Convert.ToString(reader["bbk"]);
                    sp.issn = Convert.ToString(reader["issn"]);
                    sp.isbn = Convert.ToString(reader["isbn"]);
                    sp.asset_number = Convert.ToString(reader["asset_number"]);
                }
                else
                {
                    return null;
                }
                reader.Close();
                if (sp.type == "Монография") {
                    //Получаем авторов
                    CommandText = string.Format(@"select asp.publication_id, author.id as author_id, author.family_name, author.name, author.middle_name
from author_science_publication as asp
join author on author.id=asp.author_id
where asp.publication_id in ({0});", Convert.ToInt32(id));
                    MySqlCommand select_authors = new MySqlCommand(CommandText, UserSQLconnection);
                    MySqlDataReader reader_authors = select_authors.ExecuteReader();
                    while (reader_authors.Read())
                    {
                        sp.authors.Add(new Author
                        {
                            id = Convert.ToInt32(reader_authors["author_id"]),
                            family_name = Convert.ToString(reader_authors["family_name"]),
                            name = Convert.ToString(reader_authors["name"]),
                            middle_name = Convert.ToString(reader_authors["middle_name"])
                        });
                    }
                    reader_authors.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return sp;
        }

        //Производим выборочную выборку требуемых полей, по условиям, определенным в WHERE_expr
        //Ответ получаем в коллекцию, переданную в параметрах result_collection
        //Третий параметр отображает нужно ли очистить коллекцию, перед добавлением данных, по умолчанию = true
        //return 1 - успешно; 0 - ошибка обмена данных; -1 - неинициализированная коллекция
        public int GetStudyPublications(string WHERE_expr, ObservableCollection<StudyPublication> result_collection, bool clear_collection = true)
        {
            if (result_collection == null) return -1;
            if (clear_collection) result_collection.Clear();
            string CommandText = string.Format(@"select study_publication.id, study_publication_type.name as type, publication_form.name as form, 
study_publication.title, publication_classification.name as classification,
structure_unit.name as structure_unit, structure_unit.number as number, speciality.name as speciality, study_publication.education_level,
discipline.name as discipline, study_publication.discipline_unit, study_publication.publication_date
from study_publication
join study_publication_type 		on study_publication_type.id = study_publication.type
join publication_form 				on publication_form.id = study_publication.form
join publication_classification 	on publication_classification.id = study_publication.classification
join structure_unit					on structure_unit.id = study_publication.structure_unit
join speciality						on speciality.id = study_publication.speciality
join discipline						on discipline.id = study_publication.discipline
where {0};", WHERE_expr);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                bool rows_returned = false;
                //Если был получен результат заполняем поля объекта и возвращаем его
                while (reader.Read())
                {
                    rows_returned = true;
                    string edu_lvl;
                    //Сначала получаем уровень образования и преобразуем в читаемый вид
                    if (Convert.ToInt32(reader["education_level"]) == 1) { edu_lvl = "Среднее образование"; }
                    else if (Convert.ToInt32(reader["education_level"]) == 2) { edu_lvl = "Высшее образование"; }
                    else { edu_lvl = "Не указано"; }

                    string number = Convert.ToString(reader["number"]); 
                    result_collection.Add(new StudyPublication { id = Convert.ToInt32(reader["id"]),
                        type = Convert.ToString(reader["type"]),
                        form = Convert.ToString(reader["form"]),
                        title = Convert.ToString(reader["title"]),
                        classification = Convert.ToString(reader["classification"]),
                        structure_unit = string.Format("({0}) {1}", number, Convert.ToString(reader["structure_unit"])),
                        speciality = Convert.ToString(reader["speciality"]),
                        education_level = edu_lvl,
                        discipline = Convert.ToString(reader["discipline"]),
                        discipline_unit = Convert.ToString(reader["discipline_unit"]),
                        publication_date = Convert.ToString(reader["publication_date"])
                    });
                }
                if (!rows_returned) return 0;    //Если не получили в выборке результатов, прекращаем выполнение метода
                //Получаем всех связанныех с выбранными изданиями авторов
                string publication_ids = "";
                for (int i = 0; i < result_collection.Count; i++)
                {
                    if (i != 0)
                    {
                        publication_ids += String.Format(", {0}", Convert.ToString(result_collection[i].id));
                    }
                    else
                    {
                        publication_ids += String.Format("{0}", Convert.ToString(result_collection[i].id));
                    }
                }
                CommandText = string.Format(@"select asp.publication_id, author.id as author_id, author.family_name, author.name, author.middle_name
                                            from author_study_publication as asp
                                            join author on author.id=asp.author_id
                                            where asp.publication_id in ({0});", publication_ids);
                reader.Close();
                MySqlCommand select_authors = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader_authors = select_authors.ExecuteReader();
                while (reader_authors.Read())
                {
                    int publication_id = Convert.ToInt32(reader_authors["publication_id"]);
                    foreach (StudyPublication val in result_collection)
                    {
                        if (val.id == publication_id)
                        {
                            val.authors.Add(new Author { id = Convert.ToInt32(reader_authors["author_id"]),
                                                        family_name = Convert.ToString(reader_authors["family_name"]),
                                                        name = Convert.ToString(reader_authors["name"]),
                                                        middle_name = Convert.ToString(reader_authors["middle_name"])
                            });
                            break;
                        }
                    }
                }
                reader_authors.Close();
                foreach (StudyPublication val in result_collection)
                {
                    val.GenerateAuthors();
                }

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            return 1;
        }

        //Производим выборочную выборку требуемых полей, по условиям, определенным в WHERE_expr
        //Ответ получаем в коллекцию, переданную в параметрах result_collection
        //Третий параметр отображает нужно ли очистить коллекцию, перед добавлением данных, по умолчанию = true
        //return 1 - успешно; 0 - ошибка обмена данных; -1 - неинициализированная коллекция
        public int GetSciencePublications(string WHERE_expr, ObservableCollection<SciencePublication> result_collection, bool clear_collection = true)
        {
            if (result_collection == null) return -1;
            if (clear_collection) result_collection.Clear();
            string CommandText = string.Format(@"select science_publication.id, science_publication_type.name as type, publication_form.name as form, 
science_publication.title, science_publication.part_number, publication_classification.name as classification,
structure_unit.name as structure_unit, structure_unit.number as number, science_publication.publication_date
from science_publication
join science_publication_type 		on science_publication_type.id = science_publication.type
join publication_form 				on publication_form.id = science_publication.form
join publication_classification 	on publication_classification.id = science_publication.classification
join structure_unit					on structure_unit.id = science_publication.structure_unit
where {0};", WHERE_expr);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                bool rows_returned = false;
                //Если был получен результат заполняем поля объекта и возвращаем его
                while (reader.Read())
                {
                    rows_returned = true;
                    string number = Convert.ToString(reader["number"]);
                    result_collection.Add(new SciencePublication
                    {
                        id = Convert.ToInt32(reader["id"]),
                        type = Convert.ToString(reader["type"]),
                        form = Convert.ToString(reader["form"]),
                        title = Convert.ToString(reader["title"]),
                        classification = Convert.ToString(reader["classification"]),
                        structure_unit = string.Format("({0}) {1}", number, Convert.ToString(reader["structure_unit"])),
                        part_number = Convert.ToString(reader["part_number"]),
                        publication_date = Convert.ToString(reader["publication_date"])
                    });
                }
                if (!rows_returned) return 0;    //Если не получили в выборке результатов, прекращаем выполнение метода
                //Получаем всех связанныех с выбранными изданиями авторов
                string publication_ids = "";
                for (int i = 0; i < result_collection.Count; i++)
                {
                    if (i != 0)
                    {
                        publication_ids += String.Format(", {0}", Convert.ToString(result_collection[i].id));
                    }
                    else
                    {
                        publication_ids += String.Format("{0}", Convert.ToString(result_collection[i].id));
                    }
                }
                CommandText = string.Format(@"select asp.publication_id, author.id as author_id, author.family_name, author.name, author.middle_name
                                            from author_science_publication as asp
                                            join author on author.id=asp.author_id
                                            where asp.publication_id in ({0});", publication_ids);
                reader.Close();
                MySqlCommand select_authors = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader_authors = select_authors.ExecuteReader();
                while (reader_authors.Read())
                {
                    int publication_id = Convert.ToInt32(reader_authors["publication_id"]);
                    foreach (SciencePublication val in result_collection)
                    {
                        if (val.id == publication_id)
                        {
                            val.authors.Add(new Author
                            {
                                id = Convert.ToInt32(reader_authors["author_id"]),
                                family_name = Convert.ToString(reader_authors["family_name"]),
                                name = Convert.ToString(reader_authors["name"]),
                                middle_name = Convert.ToString(reader_authors["middle_name"])
                            });
                            break;
                        }
                    }
                }
                reader_authors.Close();
                foreach (SciencePublication val in result_collection)
                {
                    val.GenerateAuthors();
                }

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
            return 1;
        }

        //Возвращает список типа SimpleTableType, представляющий собой набор id и названий специальностей
        public List<SimpleTableType> GetSpecialities()
        {
            return GetSimpleTableData("speciality");
        }

        //Возвращает список типа SimpleTableType, представляющий собой набор id и названий форм публикаций
        public List<SimpleTableType> GetPublicationForms()
        {
            return GetSimpleTableData("publication_form");
        }

        //Возвращает список типа SimpleTableType, представляющий собой набор id и названий грифов публикаций
        public List<SimpleTableType> GetPublicationClassifications()
        {
            return GetSimpleTableData("publication_classification");
        }

        //Возвращает список типа SimpleTableType, представляющий собой набор id и названий видов публикаций
        public List<SimpleTableType> GetStudyPublicationTypes()
        {
            return GetSimpleTableData("study_publication_type");
        }

        //Возвращает список типа SimpleTableType, представляющий собой набор id и названий видов публикаций
        public List<SimpleTableType> GetSciencePublicationTypes()
        {
            return GetSimpleTableData("science_publication_type");
        }

        //запрашивает из БД структурные подразделения с указанным name
        public List<SimpleTableType> GetPreviewStructureUnit(string find_name)
        {
            return GetSimpleTableData("structure_unit", find_name);
        }

        public List<SimpleTableType> GetPreviewSpiciality(string find_name)
        {
            return GetSimpleTableData("speciality", find_name);
        }

        public List<SimpleTableType> GetPreviewDiscipline(string find_name)
        {
            return GetSimpleTableData("discipline", find_name);
        }


        //Используется для компактности кода, возвращает список SimpleTableType,
        //в зависимости от переданной в параметрах таблицы
        //если параметр find_name задан, то произваодится поиск с указанным where, limit 6 и select ditinct
        private List<SimpleTableType> GetSimpleTableData(string table, string find_name = null)
        {
            List<SimpleTableType> ret_val = new List<SimpleTableType>();
            string CommandText;
            if(find_name != null) CommandText = String.Format("select distinct name from {0} where name like ('%{1}%') limit 6;", table, find_name);
            else CommandText = String.Format("select * from {0};", table);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                if(find_name == null) ret_val.Add(new SimpleTableType { id = 0, name = "" }); //Это добавляем только если не был задан limit6
                MySqlDataReader reader = select.ExecuteReader();
                //если find_name != null - значит у нас запрос preview для textBox'a
                if (find_name != null)
                    while (reader.Read())
                    {                    
                        ret_val.Add(new SimpleTableType
                        {
                            name = Convert.ToString(reader["name"])
                        });
                    }
                else
                    while (reader.Read())
                    {
                        ret_val.Add(new SimpleTableType
                        {
                            id = Convert.ToInt32(reader["id"]),
                            name = Convert.ToString(reader["name"])
                        });
                    }
                reader.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return ret_val;
        }

        //Возвращает список структурных подразделений и их номеров
        public List<SimpleTableType> GetStructureUnits()
        {
            List<SimpleTableType> ret_val = new List<SimpleTableType>();
            string CommandText = "select * from structure_unit;";
            string number;
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                ret_val.Add(new SimpleTableType { id = 0, name = ""});
                while (reader.Read())
                {
                    number = Convert.ToString(reader["number"]);
                    ret_val.Add(new SimpleTableType
                    {
                        id = Convert.ToInt32(reader["id"]),                        
                        name = string.Format("({0}) {1}", number, Convert.ToString(reader["name"]))
                    });
                }
                reader.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return ret_val;
        }

        public List<SimpleTableType> GetPreviewFromTable(string table, string column, string pattern)
        {
            List<SimpleTableType> ret_val = new List<SimpleTableType>();
            string CommandText;
            CommandText = String.Format("select distinct {1} from {0} where {1} like ('%{2}%') limit 6;", table, column, pattern);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                while (reader.Read())
                {
                    ret_val.Add(new SimpleTableType
                    {
                        name = Convert.ToString(reader[column])
                    });
                }
                reader.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return ret_val;
        }

        //Удаляет элемент из базы данных по ID (учебное)
        public bool DeleteStudyPublicationById(int id)
        {
            string CommandText = String.Format("delete from study_publication where id={0};", id);
            try
            {
                MySqlCommand delete = new MySqlCommand(CommandText, UserSQLconnection);
                var result = delete.ExecuteNonQuery();
                if(result != 1)
                {
                    return false;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return false; }
            return true;
        }

        //Удаляет автора из базы данных по ID
        public bool DeleteAuthorById(int id)
        {
            string CommandText = String.Format("delete from author where id={0};", id);
            try
            {
                MySqlCommand delete = new MySqlCommand(CommandText, UserSQLconnection);
                var result = delete.ExecuteNonQuery();
                if (result != 1)
                {
                    return false;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return false; }
            return true;
        }

        //Удаляет элемент из базы данных по ID (научное)
        public bool DeleteSciencePublicationById(int id)
        {
            string CommandText = String.Format("delete from science_publication where id={0};", id);
            try
            {
                MySqlCommand delete = new MySqlCommand(CommandText, UserSQLconnection);
                var result = delete.ExecuteNonQuery();
                if (result != 1)
                {
                    return false;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return false; }
            return true;
        }

        //Проверка существования элемента в таблице(если есть, возвращает id)
        public int CheckValueExistence(string table, string column, string pattern)
        {
            int id = 0;
            string CommandText = String.Format("select id from {0} where {1}='{2}' limit 1;", table, column, pattern);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                if (reader.Read())
                {
                    id = Convert.ToInt32(reader[id]);
                    reader.Close();
                }
                else
                {
                    reader.Close(); 
                    return 0; 
                }
                
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return id;
        }

        public void GetDefaultPublisherData(out string Name, out string Address)
        {
            Name = "";
            Address = "";
            string CommandText = "select name from publisher_name where id=1;";
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                if (reader.Read())
                {
                    Name = Convert.ToString(reader["name"]);
                }
                reader.Close();

                CommandText = "select name from publisher_address where id=1;";
                MySqlCommand select2 = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader2 = select2.ExecuteReader();
                if (reader2.Read())
                {
                    Address = Convert.ToString(reader2["name"]);
                }
                reader2.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        //Проверяет занят ли текущий логин
        public bool IsLoginFree(string login)
        {
            string CommandText = string.Format("select * from users where login='{0}';", login);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                if (reader.Read())
                {
                    return false;
                }
                reader.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return true;
        }

        public bool Authorize(string login, string pass)
        {
            string CommandText = string.Format(@"select * from users where login='{0}' and password='{1}';", login, GetHashString(pass));
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader =  select.ExecuteReader();
                if (reader.Read())
                    return true;
                else
                    return false;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return false; }
        }

        public void ChangePassword(string login, string new_pass)
        {
            string CommandText = string.Format(@"update users set password='{1}' where login='{0}';", login, GetHashString(new_pass));
            try
            {
                MySqlCommand insert = new MySqlCommand(CommandText, UserSQLconnection);
                insert.ExecuteNonQuery();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public bool DeleteUser(string login)
        {
            string CommandText = string.Format(@"select * from users where login='{0}';", login);
            try
            {
                string id = "";
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                if (reader.Read())
                    id = Convert.ToString(reader["id"]);
                else
                    return false;
                reader.Close();
                CommandText = string.Format(@"delete from users where id='{0}';", id);
                MySqlCommand delete = new MySqlCommand(CommandText, UserSQLconnection);
                delete.ExecuteNonQuery();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return false; }
            return true;
        }

        //Сохраняет нового пользователя
        public void SaveNewUser(string login, string pass)
        {
            string CommandText = string.Format(@"insert into users (login, password) values ('{0}', '{1}');", login, GetHashString(pass));
            try
            {
                MySqlCommand insert = new MySqlCommand(CommandText, UserSQLconnection);
                insert.ExecuteNonQuery();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        string GetHashString(string s)
        {
            //переводим строку в байт-массим  
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            //создаем объект для получения средст шифрования  
            MD5CryptoServiceProvider CSP =
                new MD5CryptoServiceProvider();
            //вычисляем хеш-представление в байтах  
            byte[] byteHash = CSP.ComputeHash(bytes);
            string hash = string.Empty;
            //формируем одну цельную строку из массива  
            foreach (byte b in byteHash)
                hash += string.Format("{0:x2}", b);
            return hash;
        }

        //Проверяет существует ли структура БД
        public bool IsDBExists(string db)
        {
            string CommandText = string.Format(@"select * from information_schema.tables where table_schema='{0}' limit 1", db);
            try
            {
                MySqlCommand select = new MySqlCommand(CommandText, UserSQLconnection);
                MySqlDataReader reader = select.ExecuteReader();
                if (reader.Read())
                {
                    return true;
                }
                reader.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return false;
        }

        public bool CreateDB(string db)
        {
            if (File.Exists("bd.create"))
            {
                StreamReader sr = new StreamReader("bd.create");
                string str = sr.ReadToEnd();
                str = str.Replace("author_raiting_system", db);
                string[] commands = str.Split(';');
                try
                {
                    foreach (string s in commands)
                    {
                        if (s != "\r\n")
                        {
                            MySqlCommand cmd = new MySqlCommand(s + ";", UserSQLconnection);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch(Exception ex) { MessageBox.Show(ex.Message); return false; }
                return true;
            }
            else
            {
                MessageBox.Show("ОШИБКА СОЗДАНИЯ БД!");
                return false;
            }
        }

        public static string SpecialChars(string target)
        {
            StringBuilder sb = new StringBuilder();
            foreach(char c in target)
            {
                if(c != '\'' && c != '\\' && c != '/' && c != '{' && c != '}' && c != '[' && c != ']' && c != '%')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
