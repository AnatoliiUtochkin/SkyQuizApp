using SkyQuizApp.ViewModels.Teacher.Tabs;

namespace SkyQuizApp.Views.Teacher.Tabs
{
    public partial class MyTestsTab : System.Windows.Controls.UserControl
    {
        public MyTestsTab(MyTestsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}