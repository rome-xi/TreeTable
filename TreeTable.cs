using CommonUtilities;
using GrapeCity.Forguncy.CellTypes;
using GrapeCity.Forguncy.Commands;
using GrapeCity.Forguncy.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    [TreeTableStyleTemplateSupport]
    public class TreeTable : CellType, IReferenceListView, IReferenceListViewColumn, IStyleTemplateSupport
    {
        [DisplayName("设置绑定表格参数")]
        public ListViewInfo SetBindingListViewParam { get; set; }

        [DisplayName("设置展开方式")]
        public UnfoldingMode SetUnfoldingMode { get; set; }

        [DisplayName("展开到层级：")]
        public int UnfoldingLevel
        {
            get; set;
        }

        [CategoryHeader("网格线设置")]
        [DisplayName("是否显示网格线")]
        public bool GridLineShow
        {
            get; set;
        }

        [DisplayName("网格线宽度：")]
        public int GridLineWidth
        {
            get; set;
        }

        [DisplayName("网格线颜色：")]
        [ColorProperty]
        public string GridLineColor
        {
            get; set;
        }

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

        public override bool GetDesignerPropertyVisible(string propertyName)
        {
            if (string.Equals(propertyName, nameof(GridLineWidth)) || string.Equals(propertyName, nameof(GridLineColor)))
            {
                return GridLineShow;
            }
            if (string.Equals(propertyName,nameof(UnfoldingLevel)))
            {
                return SetUnfoldingMode == UnfoldingMode.展开到指定层级;
            }
            return base.GetDesignerPropertyVisible(propertyName);
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
        private SetBindingListView control;
        private IEditorSettingsDataContext dataContext;

        public IBuilderContext BuilderContext { get; set; }

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
            StackPanel buttonControl = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 15, 10) };
            Button okButton = new Button() { Content = "确认", Width = 75 };
            okButton.Click += OkButton_Click;
            Button cancelButton = new Button() { Content = "取消", Width = 75, Margin = new Thickness(5, 0, 0, 0) };
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
                Width = 460d,
                Height = 546d,
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

        public HyperlinkTemplate HyperlinkTemplate { get; set; }

        public FieldType Type { get; set; }

        public MyFieldInfo(string field, string name, FieldType type, List<Command> commandList, HyperlinkTemplate hyperlinkTemplate)
        {
            Field = field;
            Name = name;
            Type = type;
            CommandList = commandList;
            HyperlinkTemplate = hyperlinkTemplate;
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

    public class HyperlinkTemplate
    {
        public object BackgroundColor
        {
            get;
            set;
        }

        public object FrontColor
        {
            get;
            set;
        }

        public bool IsBold
        {
            get;
            set;
        }

        public HyperlinkTemplate(object backgroundColor, object frontColor, bool isBold)
        {
            BackgroundColor = backgroundColor;
            FrontColor = frontColor;
            IsBold = isBold;
        }
    }

    public enum UnfoldingMode
    {
        全部收起,
        全部展开,
        展开到指定层级
    }

    public enum FieldType
    {
        Normal,
        Button,
        Hyperlink
    }


    public class TreeTableStyleTemplateSupportAttribute : CellTypeStyleTemplateSupportAttribute
    {
        protected const CellStates SupportStates = CellStates.Normal | CellStates.Selected;

        private SupportStyles DefaultSupportStyles =
                SupportStyles.BackgroundColor |
                SupportStyles.BackgroundGradient |
                SupportStyles.ForegroundColor |
                SupportStyles.Opacity;
        public TreeTableStyleTemplateSupportAttribute()
        {
            TemplateParts = new List<TemplatePart>()
            {
                new TemplatePart() { Name = "tableHead", SupportStates = CellStates.Normal, SupportStyles = DefaultSupportStyles },
                new TemplatePart() { Name = "tableBody", SupportStates = SupportStates, SupportStyles = DefaultSupportStyles }
            };
        }

        public override List<TemplatePart> TemplateParts { get; }


        List<CellTypeStyleTemplate> presetTemplates;
        public override List<CellTypeStyleTemplate> PresetTemplates
        {
            get
            {
                if (presetTemplates == null)
                {
                    presetTemplates = MakePresetStyleTemplates();
                }
                return presetTemplates;
            }
        }

        public override string DefaultTemplateKey => "Style1";

        protected string StyleFileName => "TreeTableStyle";

        private List<CellTypeStyleTemplate> MakePresetStyleTemplates()
        {
            var dllDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString());
            var jsPath = Path.Combine(dllDir, "Resources", "StyleTemplate", StyleFileName + ".js");
            var json = File.ReadAllText(Path.GetFullPath(jsPath));
            var index = json.LastIndexOf("];");
            if (index > 0)
            {
                json = json.Substring(0, index + 1) + json.Substring(index + 2, json.Length - index - 2);
            }
            return JsonUtilities.FromJsonString<List<CellTypeStyleTemplate>>(json);
        }
    }
}
