using System.Windows.Controls;
using SkyQuizApp.ViewModels.Admin.Tabs;

namespace SkyQuizApp.Views.Admin.Tabs
{
    public partial class TwoFactorCodesTab : UserControl
    {
        public TwoFactorCodesTab(TwoFactorCodesTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}