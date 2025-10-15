using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SkyQuizApp.ViewModels.Auth;

namespace SkyQuizApp.Views.Auth
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUser.Focus();
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

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}