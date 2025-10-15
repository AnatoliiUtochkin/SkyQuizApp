using System.Windows.Controls;
using SkyQuizApp.ViewModels.Student;

namespace SkyQuizApp.Views.Student.Tabs
{
    public partial class StudentSettingsTab : UserControl
    {
        public StudentSettingsTab(StudentSettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}