using System.Windows.Controls;
using SkyQuizApp.ViewModels.Admin.Tabs;

namespace SkyQuizApp.Views.Admin.Tabs
{
    public partial class ResultsTab : UserControl
    {
        public ResultsTab(ResultsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}