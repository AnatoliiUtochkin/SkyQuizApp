using System.Windows.Controls;
using SkyQuizApp.ViewModels.Student.Tabs;

namespace SkyQuizApp.Views.Student.Tabs
{
    public partial class JoinTestTab : UserControl
    {
        public JoinTestTab(JoinTestViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}