using System.Windows;
using System.Windows.Input;
using SkyQuizApp.ViewModels.Auth;

namespace SkyQuizApp.Views.Auth
{
    /// <summary>
    /// Interaction logic for ResetPassword.xaml
    /// </summary>
    public partial class ResetPasswordView : Window
    {
        private readonly ResetPasswordViewModel _viewModel;

        public ResetPasswordView(ResetPasswordViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.HandleDrag(this, e);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.HandleMinimize(this);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.HandleClose(this);
        }
    }
}