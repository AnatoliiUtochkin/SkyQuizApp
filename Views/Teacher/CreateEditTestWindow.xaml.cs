using System.Windows;
using SkyQuizApp.ViewModels.Teacher;

namespace SkyQuizApp.Views.Teacher
{
    public partial class CreateEditTestWindow : Window
    {
        public CreateEditTestWindow(int? testId = null)
        {
            InitializeComponent();
            DataContext = new CreateEditTestViewModel(testId);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}