using System.Windows;
using System.Windows.Input;

namespace SkyQuizApp.Views.Student
{
    public partial class StudentMainView : Window
    {
        public StudentMainView(StudentMainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        public void HideTemporarily()
        {
            this.Hide();
        }

        public void RestoreAfterTest()
        {
            if (DataContext is StudentMainViewModel vm)
                vm.ShowMyResultsViewCommand.Execute(null);

            this.Show();
            this.Activate();
        }
    }
}