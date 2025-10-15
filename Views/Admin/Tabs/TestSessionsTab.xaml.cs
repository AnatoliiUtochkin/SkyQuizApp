using System.Windows.Controls;
using SkyQuizApp.ViewModels.Admin.Tabs;

namespace SkyQuizApp.Views.Admin.Tabs
{
    public partial class TestSessionsTab : UserControl
    {
        public TestSessionsTab(TestSessionsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}