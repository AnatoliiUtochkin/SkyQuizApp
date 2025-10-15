using System.Windows.Controls;
using SkyQuizApp.ViewModels.Teacher.Tabs;

namespace SkyQuizApp.Views.Teacher.Tabs
{
    /// <summary>
    /// Interaction logic for SettingsTab.xaml
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        public SettingsTab(SettingsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}