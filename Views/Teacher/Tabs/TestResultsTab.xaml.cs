using System.Windows.Controls;
using SkyQuizApp.ViewModels.Teacher.Tabs;

namespace SkyQuizApp.Views.Teacher.Tabs
{
    public partial class TestResultsTab : UserControl
    {
        public TestResultsTab(TestResultsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}