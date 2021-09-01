using GrapeCity.Forguncy.CellTypes;
using GrapeCity.Forguncy.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace TreeTable.Control
{
    /// <summary>
    /// SetBindingListView.xaml 的交互逻辑
    /// </summary>
    public partial class SetBindingListView : UserControl
    {
        private readonly IBuilderContext builderContext;
        public SetBindingListView()
        {
            InitializeComponent();
        }

        public SetBindingListView(IBuilderContext builderContext)
        {
            InitializeComponent();
            this.builderContext = builderContext;
            DataContext = new SetBindingListViewViewModel(builderContext);
        }

        public SetBindingListViewViewModel ViewModel => DataContext as SetBindingListViewViewModel;

        public void NewButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedRow = new MyFieldInfoViewModel(ViewModel.ColumnsList);
            ViewModel.MyFieldInfosViewModel.Add(ViewModel.SelectedRow);
        }
        public void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            _ = ViewModel.MyFieldInfosViewModel.Remove(ViewModel.SelectedRow);
        }
        private void SetCommandList(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedRow = (sender as FrameworkElement).DataContext as MyFieldInfoViewModel;

            if (ViewModel.SelectedRow.FieldInfo.Type == FieldType.Custom)
            {

                ICommandWindow window = builderContext.GetCommandWindow(CommandScope.Cell, GetCommandParams().ToList());

                window.InitCommandEvent += () =>
                {
                    return ViewModel.SelectedRow.CommandList;
                };

                window.UpdateCommandEvent += (sender2, commandList) =>
                {
                    ViewModel.SelectedRow.CommandList = commandList;
                };

                window.Closed += (sender2, e2) =>
                {
                    builderContext.ShowParentDialog(this);
                };

                builderContext.HideParentDialog(this);
                _ = window.ShowDialog();
            }
        }

        private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedRow = (sender as FrameworkElement).DataContext as MyFieldInfoViewModel;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedRow = (sender as FrameworkElement).DataContext as MyFieldInfoViewModel;
        }

        public IEnumerable<GenerateParam> GetCommandParams()
        {
            return ViewModel.MyFieldInfosViewModel.Where( c => c.FieldInfo.Type == FieldType.Normal).Select( c => GetParam(c.FieldInfo.Field)).ToList();
        }

        GenerateParam GetParam(string paramName)
        {
            if (!string.IsNullOrEmpty(paramName) && !paramName.StartsWith("="))
            {
                return new GenerateNormalParam() { ParamName = paramName, ParamScope = CommandScope.All };
            }
            return null;
        }
    }

    public class SetBindingListViewViewModel : PropertyChangedObjectBase
    {
        public IBuilderContext BuilderContext { get; set; }

        public SetBindingListViewViewModel(IBuilderContext context)
        {
            BuilderContext = context;
        }

        public ListViewInfo Model
        {
            get => new ListViewInfo(ListViewName, ID, RelatedParentID, ConverMyFieldInfoViewModel(MyFieldInfosViewModel));
            set
            {
                if (value == null)
                {
                    return;
                }
                ListViewName = value.ListViewName;
                ID = value.ID;
                RelatedParentID = value.RelatedParentID;

                foreach (MyFieldInfo myFieldInfo in value.MyFieldInfos)
                {
                    MyFieldInfoViewModel myFieldInfoViewModel = new MyFieldInfoViewModel(ColumnsList)
                    {
                        FieldInfo = new FieldItem()
                        {
                            Field = myFieldInfo.Field,
                            Type = myFieldInfo.Type
                        },
                        Name = myFieldInfo.Name,
                        CommandList = myFieldInfo.CommandList
                    };
                    MyFieldInfosViewModel.Add(myFieldInfoViewModel);
                }
            }
        }

        private string listViewName;
        public string ListViewName
        {
            get => listViewName;
            set
            {
                if (listViewName != value)
                {
                    listViewName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ColumnsList));
                    MyFieldInfosViewModel.Clear();
                    OnPropertyChanged(nameof(MyFieldInfosViewModel));
                }
            }
        }


        public List<string> ListViewsList
        => BuilderContext?.EnumAllListViewInfos(BuilderContext.PageName).Select(t => { return t.ListViewName; }).ToList() ?? new List<string>();
        public List<string> ColumnsList => string.IsNullOrEmpty(ListViewName) ? new List<string>() : (BuilderContext?.EnumAllListViewInfos(BuilderContext.PageName).FirstOrDefault(t => t.ListViewName == ListViewName)?.GetAllColumnNames());

        private ObservableCollection<MyFieldInfoViewModel> myFieldInfos;
        public ObservableCollection<MyFieldInfoViewModel> MyFieldInfosViewModel
        {
            get
            {
                if (myFieldInfos == null)
                {
                    myFieldInfos = new ObservableCollection<MyFieldInfoViewModel>();
                }
                return myFieldInfos;
            }
            set => myFieldInfos = value;
        }

        private MyFieldInfoViewModel selectedRow;
        public MyFieldInfoViewModel SelectedRow
        {
            get => selectedRow;
            set
            {
                selectedRow = value;
                OnPropertyChanged();
            }
        }

        private string id;
        public string ID
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        private string parentId;
        public string RelatedParentID
        {
            get => parentId;
            set
            {
                parentId = value;
                OnPropertyChanged();
            }
        }

        private MyFieldInfo[] ConverMyFieldInfoViewModel(ObservableCollection<MyFieldInfoViewModel> myFieldInfosViewModel)
        {
            return myFieldInfosViewModel.Select(viewModel => new MyFieldInfo(viewModel.FieldInfo.Field, viewModel.Name, viewModel.FieldInfo.Type, viewModel.CommandList)).ToArray();
        }
    }

    public class MyFieldInfoViewModel : PropertyChangedObjectBase
    {
        public MyFieldInfoViewModel(List<string> columnList)
        {
            List<FieldItem> list = columnList.Select(c => new FieldItem()
            {
                Type = FieldType.Normal,
                Field = c
            }).ToList();

            FieldList = list.Union(new List<FieldItem>()
            {
                new FieldItem()
                {
                    Type = FieldType.Custom,
                    Field = "Button"
                },
                new FieldItem()
                {
                    Type = FieldType.Custom,
                    Field = "Hyperlink"
                }
            }).ToList();
        }

        private List<FieldItem> fieldList;
        public List<FieldItem> FieldList
        {
            get => fieldList;
            set
            {
                fieldList = value;
                OnPropertyChanged();
            }
        }

        private FieldItem field;
        public FieldItem FieldInfo
        {
            get => field;
            set
            {
                field = value;
                Name = field.Field;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Visible));
            }
        }

        private string name;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }


        private List<Command> commandList;
        public List<Command> CommandList
        {
            get => commandList;
            set
            {
                commandList = value;
                OnPropertyChanged();
            }
        }

        public Visibility Visible => FieldInfo?.Type == FieldType.Custom ? Visibility.Visible : Visibility.Hidden;
    }

    public class FieldItem
    {
        public string Field { get; set; }

        public FieldType Type { get; set; }

        public override bool Equals(object obj)
        {
            return obj is FieldItem item && item.Field == Field && item.Type == Type;
        }

        public override int GetHashCode()
        {
            return Field.GetHashCode() ^ Type.GetHashCode();
        }
    }

    public class PropertyChangedObjectBase : INotifyPropertyChanged
    {
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
