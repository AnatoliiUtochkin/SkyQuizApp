using System.Windows;
using System.Windows.Input;
using SkyQuizApp.ViewModels.Admin;

namespace SkyQuizApp.Views.Admin
{
    /// <summary>
    /// Interaction logic for AdminMainView.xaml
    /// </summary>
    public partial class AdminMainView : Window
    {
        private readonly AdminMainViewModel _viewModel;

        public AdminMainView(AdminMainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.HandleDrag(this, e);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.HandleClose(this);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.HandleMinimize(this);
        }
    }
}