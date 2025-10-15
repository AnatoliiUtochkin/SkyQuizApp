using System.Windows.Controls;
using SkyQuizApp.ViewModels.Admin.Tabs;

namespace SkyQuizApp.Views.Admin.Tabs
{
    public partial class AnswersTab : UserControl
    {
        public AnswersTab(AnswersTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}