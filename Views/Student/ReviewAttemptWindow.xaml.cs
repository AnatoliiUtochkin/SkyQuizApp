using System.Windows;
using System.Windows.Input;
using SkyQuizApp.ViewModels.Student;

namespace SkyQuizApp.Views.Student
{
    public partial class ReviewAttemptWindow : Window
    {
        public ReviewAttemptWindow(int sessionId)
        {
            InitializeComponent();
            DataContext = new ReviewAttemptViewModel(sessionId);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}