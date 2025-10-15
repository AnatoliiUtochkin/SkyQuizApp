using System.Windows;
using System.Windows.Input;
using SkyQuizApp.ViewModels.Teacher;

namespace SkyQuizApp.Views.Teacher
{
    public partial class TestResultsWindow : Window
    {
        public TestResultsWindow(int testId)
        {
            InitializeComponent();
            DataContext = new TestResultsWindowViewModel(testId);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}