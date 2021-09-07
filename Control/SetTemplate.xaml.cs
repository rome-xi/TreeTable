using GrapeCity.Forguncy.CellTypes;
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

namespace TreeTable.Control
{
    /// <summary>
    /// SetTemplate.xaml 的交互逻辑
    /// </summary>
    public partial class SetTemplate : UserControl
    {
#pragma warning disable IDE0052 // 删除未读的私有成员
        private readonly IBuilderContext builderContext;
#pragma warning restore IDE0052 // 删除未读的私有成员

        public SetTemplate(IBuilderContext builderContext)
        {
            InitializeComponent();
            this.builderContext = builderContext;
            DataContext = new SetTemplateViewModel(builderContext);
        }

        public SetTemplateViewModel ViewModel => DataContext as SetTemplateViewModel;


    }

    public class SetTemplateViewModel
    {
        public IBuilderContext BuilderContext { get; set; }

        public SetTemplateViewModel(IBuilderContext context)
        {
            BuilderContext = context;
        }

        public HyperlinkTemplate Model
        {
            get => BackgroundColor != null || FrontColor != null
                    ? new HyperlinkTemplate(BackgroundColor.Value, FrontColor.Value, IsBold)
                    : new HyperlinkTemplate(null, null, false);
            set
            {
                if (value == null)
                {
                    BackgroundColor = new ColorModel() { Value = null };
                    FrontColor = new ColorModel() { Value = null };
                    IsBold = false;
                    return;
                }
                BackgroundColor = new ColorModel() { Value = value.BackgroundColor };
                FrontColor = new ColorModel() { Value = value.FrontColor };
                IsBold = value.IsBold;
            }
        }

        public ColorModel BackgroundColor
        {
            get;
            set;
        }
        public ColorModel FrontColor
        {
            get;
            set;
        }
        public bool IsBold
        {
            get;
            set;
        }
    }

    public class ColorModel
    {
        public object Value { get; set; }
    }
}
