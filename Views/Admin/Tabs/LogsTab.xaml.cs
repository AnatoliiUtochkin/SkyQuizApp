using System.Windows.Controls;
using SkyQuizApp.ViewModels.Admin.Tabs;

namespace SkyQuizApp.Views.Admin.Tabs
{
    public partial class LogsTab : UserControl
    {
        public LogsTab(LogsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}