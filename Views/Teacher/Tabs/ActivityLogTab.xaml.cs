using System.Windows.Controls;
using SkyQuizApp.ViewModels.Teacher.Tabs;

namespace SkyQuizApp.Views.Teacher.Tabs
{
    /// <summary>
    /// Interaction logic for ActivityLogTab.xaml
    /// </summary>
    public partial class ActivityLogTab : UserControl
    {
        public ActivityLogTab(ActivityLogTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}