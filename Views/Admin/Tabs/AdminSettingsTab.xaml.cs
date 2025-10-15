using System.Windows.Controls;
using SkyQuizApp.ViewModels.Admin.Tabs;

namespace SkyQuizApp.Views.Admin.Tabs
{
    /// <summary>
    /// Interaction logic for AdminSettingsTab.xaml
    /// </summary>
    public partial class AdminSettingsTab : UserControl
    {
        public AdminSettingsTab(AdminSettingsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}