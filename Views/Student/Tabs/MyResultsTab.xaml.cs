using System.Windows.Controls;
using SkyQuizApp.ViewModels.Student.Tabs;

namespace SkyQuizApp.Views.Student.Tabs
{
    public partial class MyResultsTab : UserControl
    {
        public MyResultsTab(MyResultsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}