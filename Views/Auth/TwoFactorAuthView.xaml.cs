using System.Windows;
using System.Windows.Input;
using SkyQuizApp.ViewModels.Auth;

namespace SkyQuizApp.Views.Auth
{
    public partial class TwoFactorAuthView : Window
    {
        private readonly TwoFactorAuthViewModel _viewModel;

        public TwoFactorAuthView(TwoFactorAuthViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.HandleDrag(this, e);
        }
    }
}