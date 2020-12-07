using AuthorRaitingSystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Controls
{

    public class SelectionChanged
    {
        public object Value
        {
            get;
            private set;
        }
        public SelectionChanged(object v)
        {
            Value = v;
        }
    }


    public partial class TextBoxDropDownHintControl : UserControl
    {
        private string connectionString;
        private Thread structure_unit_thread;
        public bool lbListSelected = false;
        private bool IsInit = false;
        private List<SimpleTableType> control_variants = new List<SimpleTableType>();

        public delegate void StructureUnit_Delegate(string name);
        private StructureUnit_Delegate threadMethod;
        public delegate void Update_Delegate(List<SimpleTableType> values);
        public Update_Delegate upd_del;

        public delegate void SelectedValueChanged(object sender, SelectionChanged e);
        public event SelectedValueChanged OnSelect;

        public delegate void _TextChanged(object sender, TextChangedEventArgs e);
        public event _TextChanged TextChanged;

        IEnumerable<object> itemsSource = null;
        ObservableCollection<object> ISCollection = new ObservableCollection<object>();
        bool firstChange = true;
        
        public TextBoxDropDownHintControl()
        {
            InitializeComponent();
            ItemsSource = control_variants;
            upd_del = new Update_Delegate(Update_delegateMethod);

            lbList.ItemsSource = ISCollection;

            lbList.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Back) e.Handled = true;
            };

            tbxInputData.TextChanged += delegate (object sender, TextChangedEventArgs e)
            {
                if (TextChanged != null)
                {
                    TextChanged(this, e);
                }
            };

            //Вызываем подгрузку из БД возможных вариантов структурных подразделений при вводе от 3х символов
            TextChanged += delegate (object sender, TextChangedEventArgs e)
            {
                if (this.Text.Length > 2 && IsInit && !firstChange)
                {
                    if (structure_unit_thread != null)
                    {
                        if (!structure_unit_thread.IsAlive)
                        {
                            string n = MySQLClient.SpecialChars(this.Text);
                            if (n.Length > 2)//Только если длина больше 2
                            {
                                //инициируем новый поток
                                structure_unit_thread = new Thread(() => threadMethod(n));
                                structure_unit_thread.Start();
                            }
                        }
                    }
                    else
                    {
                        string n = MySQLClient.SpecialChars(this.Text);
                        if (n.Length > 2)//Только если длина больше 2
                        {
                            //инициируем новый поток
                            structure_unit_thread = new Thread(() => threadMethod(n));
                            structure_unit_thread.Start();
                        }
                    }
                }
                else firstChange = false;
            };

            tbxInputData.PreviewKeyDown += delegate (object sender, KeyEventArgs e)
            {
                lbListSelected = false;
                if (e.Key == Key.Down && lbList.IsDropDownOpen == true)
                {
                    lbList.Focus();
                }
            };

            lbList.PreviewKeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                {
                    tbxInputData.Focus();
                    e.Handled = true;
                }
            };
        }

        //получаем строку соединения и производим привязку определенного метода к делегату threadMethod,
        //зависящую от того, к какому объекту относится данный контрол
        public void Initialize(string cs, int d = 0)
        {
            connectionString = cs;
            IsInit = true;
            switch (d)
            {
                case 1:     //поиск по title
                    threadMethod = new StructureUnit_Delegate(StudyPublication_Title_Method);
                    break;
                case 2:     //поиск по speciality
                    threadMethod = new StructureUnit_Delegate(Speciality_Method);
                    break;
                case 3:     //поиск по discipline
                    threadMethod = new StructureUnit_Delegate(Discipline_Method);
                    break;
                case 4:     //поиск по discipline_unit
                    threadMethod = new StructureUnit_Delegate(DisciplineUnit_Method);
                    break;
                case 5:     //поиск по фамилии
                    threadMethod = new StructureUnit_Delegate(FamilyName_Method);
                    break;
                case 6:     //поиск по title
                    threadMethod = new StructureUnit_Delegate(SciencePublication_Title_Method);
                    break;
                case 7:     //поиск по PublisherAddress
                    threadMethod = new StructureUnit_Delegate(PublisherAddress_Method);
                    break;
                case 8:     //поиск по PublisherName
                    threadMethod = new StructureUnit_Delegate(PublisherName_Method);
                    break;
                default:    //поиск по StructureUnit
                    threadMethod = new StructureUnit_Delegate(StructureUnit_Method);
                    break;
            }
        }

        private bool FilterItems()
        {
            if (itemsSource == null) return false;
            List<object> collection = (itemsSource).ToList();
            for (int i = 0; i < collection.Count; i++)
            {
                var propertyInfo = collection[i].GetType().GetProperty(lbList.DisplayMemberPath);
                string val = propertyInfo.GetValue(collection[i], null).ToString();
                if (val.ToLower().IndexOf(tbxInputData.Text.ToLower()) != -1)
                {
                    ISCollection.Add(collection[i]);
                }
            }
            if (ISCollection.Count > 0)
            {
                return true;
            }
            return false;
        }

        void LbList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnSelect != null)
            {
                OnSelect(this, new SelectionChanged(lbList.SelectedItem));
            }
            if (lbList.SelectedItem != null)
            {
                tbxInputData.Text = lbList.SelectedItem.ToString();
                tbxInputData.Focus();
                lbListSelected = true;
                lbList.IsDropDownOpen = false;
            }
        }

        public string DisplayMemberPath
        {
            get
            {
                return lbList.DisplayMemberPath;
            }
            set
            {
                lbList.DisplayMemberPath = value;
            }
        }

        public string SelectedValuePath
        {
            get
            {
                return lbList.SelectedValuePath;
            }
            set
            {
                lbList.SelectedValuePath = value;
            }
        }

        public Style TextFieldStyle
        {
            get
            {
                return tbxInputData.Style;
            }
            set
            {
                tbxInputData.Style = value;
            }
        }

        public Style ListStyle
        {
            get
            {
                return lbList.Style;
            }
            set
            {
                lbList.Style = value;
            }
        }

        public string Text
        {
            get
            {
                return tbxInputData.Text;
            }
            set
            {
                tbxInputData.Text = value;
                lbList.IsDropDownOpen = false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return tbxInputData.IsReadOnly;
            }
            set
            {
                tbxInputData.IsReadOnly = value;
            }
        }


        public new Brush Background
        {
            get
            {
                return tbxInputData.Background;
            }
            set
            {
                tbxInputData.Background = value;
            }
        }

        public void Clear()
        {
            lbList.SelectedIndex = -1;
            tbxInputData.Text = "";
        }

        public TextBox getTextBoxItem()
        {
            return tbxInputData;
        }


        public string CurrentText
        {
            get
            {
                return GetValue(CurrentTextProperty).ToString();
            }
            set
            {
                SetCurrentTextDependencyProperty(CurrentTextProperty, value);
                Text = value;
            }
        }

        public static readonly DependencyProperty CurrentTextProperty = DependencyProperty.Register("CurrentText", typeof(string),
                typeof(TextBoxDropDownHintControl), new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.None,
                new PropertyChangedCallback(OnCurrentTextChanged)));

        public event PropertyChangedEventHandler CurrentTextChanged;

        void SetCurrentTextDependencyProperty(DependencyProperty property, string value)
        {
            SetValue(property, value);
            if (CurrentTextChanged != null)
            {
                CurrentTextChanged(this, new PropertyChangedEventArgs(property.ToString()));
            }
        }

        private static void OnCurrentTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxDropDownHintControl control = d as TextBoxDropDownHintControl;
            if (control == null) return;
            control.CurrentText = e.NewValue.ToString();
        }


        public IEnumerable<object> ItemsSource
        {
            get
            {
                return (IEnumerable<object>)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetItemsSourceDependencyProperty(ItemsSourceProperty, value);
                itemsSource = value;
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<object>),
                typeof(TextBoxDropDownHintControl), new FrameworkPropertyMetadata(default(IEnumerable<object>),
                FrameworkPropertyMetadataOptions.None,
                new PropertyChangedCallback(OnItemsSourceChanged)));

        public event PropertyChangedEventHandler ItemsSourceChanged;

        void SetItemsSourceDependencyProperty(DependencyProperty property, IEnumerable<object> value)
        {
            SetValue(property, value);
            if (ItemsSourceChanged != null)
            {
                ItemsSourceChanged(this, new PropertyChangedEventArgs(property.ToString()));
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxDropDownHintControl control = d as TextBoxDropDownHintControl;
            if (control == null) return;
            control.ItemsSource = (IEnumerable<object>)e.NewValue;
        }

        //###################### Методы для работы с потоками для динамической подгрузки данных ########################//

        //Делегат обновления данных
        public void Update_delegateMethod(List<SimpleTableType> values)
        {
            control_variants.Clear();
            foreach (SimpleTableType stt in values)
            {
                control_variants.Add(stt);
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                this.ShowList();
            });
        }
        //вызывает обновление listbox'a и показ вариантов
        public void ShowList()
        {
            if(lbListSelected) { lbListSelected = false; return; }
            ISCollection.Clear();
            if (itemsSource == null) return;
            List<object> collection = (itemsSource).ToList();
            for (int i = 0; i < collection.Count; i++)
            {
                ISCollection.Add(collection[i]);
            }
            if (ISCollection.Count == 0) lbList.IsDropDownOpen = false;
            else lbList.IsDropDownOpen = true;
        }

        //Метод для подгрузки данных из таблицы structure_unit
        private void StructureUnit_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewStructureUnit(pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }
        private void Speciality_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewSpiciality(pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }
        private void Discipline_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewDiscipline(pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }
        private void DisciplineUnit_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewFromTable("study_publication", "discipline_unit", pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }

        //Метод для подгрузки данных из таблицы study_publication.title
        private void StudyPublication_Title_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewFromTable("study_publication", "title", pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }
        //Метод для подгрузки данных из таблицы science_publication.title
        private void SciencePublication_Title_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewFromTable("science_publication", "title", pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }
        private void FamilyName_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewFromTable("author", "family_name", pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }
        private void PublisherAddress_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewFromTable("publisher_address", "name", pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }
        private void PublisherName_Method(string pattern)
        {
            List<SimpleTableType> variants = new List<SimpleTableType>();
            MySQLClient mySqlClient = new MySQLClient(connectionString);
            variants = mySqlClient.GetPreviewFromTable("publisher_name", "name", pattern);
            //Вызов делегата
            upd_del.Invoke(variants);
        }
        //###################### Методы для работы с потоками для динамической подгрузки данных ########################//
    }
}