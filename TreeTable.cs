using GrapeCity.Forguncy.CellTypes;
using GrapeCity.Forguncy.Commands;
using GrapeCity.Forguncy.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TreeTable.Control;

namespace TreeTable
{
    [Designer("TreeTable.TreeTableDesigner, TreeTable")]
    [Icon("pack://application:,,,/TreeTable;component/Resources/TreeTableIcon.png")]
    public class TreeTable : CellType, IReferenceListView, IReferenceListViewColumn
    {
        [DisplayName("设置绑定表格参数")]
        public ListViewInfo SetBindingListViewParam { get; set; }

        [DisplayName("设置展开方式")]
        public UnfoldingMode SetUnfoldingMode { get; set; }

        [DefaultValue(null)]
        [Browsable(false)]
        public string TemplateKey { get; set; }

        public IEnumerable<string> GetListViewNames()
        {
            yield return SetBindingListViewParam.ListViewName;
        }

        public void RenameListviewColumnName(string listviewName, string oldName, string newName)
        {
            if (string.Equals(SetBindingListViewParam.ListViewName, listviewName))
            {
                if (string.Equals(SetBindingListViewParam.ID, oldName))
                {
                    SetBindingListViewParam.ID = newName;
                }
                if (string.Equals(SetBindingListViewParam.RelatedParentID, oldName))
                {
                    SetBindingListViewParam.RelatedParentID = newName;
                }
                foreach (MyFieldInfo myFieldInfo in SetBindingListViewParam.MyFieldInfos)
                {
                    if (string.Equals(myFieldInfo.Field, oldName))
                    {
                        myFieldInfo.Field = newName;
                    }
                }
            }
        }

        public void RenameListviewName(string oldName, string newName)
        {
            if (string.Equals(SetBindingListViewParam.ListViewName, oldName))
            {
                SetBindingListViewParam.ListViewName = newName;
            }
        }

        public override string ToString()
        {
            return "树型表格";
        }
    }

    public class TreeTableDesigner : CellTypeDesigner<TreeTable>
    {
        public override EditorSetting GetEditorSetting(PropertyDescriptor property, IBuilderContext builderContext)
        {
            return property.Name == nameof(TreeTable.SetBindingListViewParam)
                ? new HyperlinkEditorSetting(new SetBindingListViewParamCommand(builderContext))
                : base.GetEditorSetting(property, builderContext);
        }

        public override FrameworkElement GetDrawingControl(ICellInfo cellInfo, IDrawingHelper drawingHelper)
        {
            Grid grid = new Grid();
            Image image = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/TreeTable;component/Resources/TreeTableLogocopy.png", UriKind.RelativeOrAbsolute)),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            _ = grid.Children.Add(image);

            return grid;
        }
    }

    public class SetBindingListViewParamCommand : ICommand
    {

        private Window window;
        private IEditorSettingsDataContext dataContext;
        private IBuilderContext BuilderContext { get; set; }

        private SetBindingListView control;

        public SetBindingListViewParamCommand(IBuilderContext builderContext)
        {
            BuilderContext = builderContext;
        }
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            dataContext = parameter as IEditorSettingsDataContext;

            control = new SetBindingListView(BuilderContext);
            control.ViewModel.Model = dataContext?.Value as ListViewInfo;
            StackPanel buttonControl = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 5, 10) };
            Button okButton = new Button() { Content = "确认", Width = 80 };
            okButton.Click += OkButton_Click;
            Button cancelButton = new Button() { Content = "取消", Width = 80, Margin = new Thickness(8, 0, 0, 0) };
            cancelButton.Click += CancelButton_Click;
            buttonControl.Children.Add(okButton);
            buttonControl.Children.Add(cancelButton);

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            grid.Children.Add(control);
            grid.Children.Add(buttonControl);
            Grid.SetRow(control, 0);
            Grid.SetRow(buttonControl, 1);

            window = new Window
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "设置绑定表格参数",
                Width = 410d,
                Height = 630d,
                Content = grid
            };
            _ = window.ShowDialog();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            window.Close();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            dataContext.Value = control.ViewModel.Model;
            window.Close();
        }
    }

    public class MyFieldInfo
    {
        public string Field { get; set; }

        public string Name { get; set; }

        public List<Command> CommandList { get; set; }

        public FieldType Type { get; set; }

        public MyFieldInfo(string field, string name, FieldType type, List<Command> commandList)
        {
            Field = field;
            Name = name;
            Type = type;
            CommandList = commandList;
        }
        public MyFieldInfo Clone()
        {
            return new MyFieldInfo(Field, Name, Type, CommandList);
        }
    }

    public class ListViewInfo
    {
        public string ListViewName
        {
            get; set;
        }
        public string ID
        {
            get; set;
        }
        public string RelatedParentID
        {
            get; set;
        }
        public MyFieldInfo[] MyFieldInfos
        {
            get; set;
        }
        public ListViewInfo(string listViewName, string id, string relatedParentID, MyFieldInfo[] myFieldInfos)
        {
            ListViewName = listViewName;
            ID = id;
            RelatedParentID = relatedParentID;
            MyFieldInfos = myFieldInfos;
        }
    }

    public enum UnfoldingMode
    {
        默认收起,
        默认展开
    }

    public enum FieldType
    {
        Normal,
        Custom
    }
}
