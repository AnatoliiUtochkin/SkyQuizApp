using System.Windows;
using System.Windows.Input;
using SkyQuizApp.ViewModels.Teacher;

namespace SkyQuizApp.Views.Teacher
{
    public partial class TeacherMainView : Window
    {
        private readonly TeacherMainViewModel _viewModel;

        public TeacherMainView(TeacherMainViewModel viewModel)
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
            this.Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.HandleMinimize(this);
        }
    }
}