using System.Windows;
using System.Windows.Input;
using SkyQuizApp.ViewModels.Student;

namespace SkyQuizApp.Views.Student
{
    public partial class TestAttemptsWindow : Window
    {
        public TestAttemptsWindow(int testId)
        {
            InitializeComponent();
            DataContext = new TestAttemptsViewModel(testId);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}