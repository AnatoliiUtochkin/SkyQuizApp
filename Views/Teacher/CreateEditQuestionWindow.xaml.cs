using System.Windows;
using SkyQuizApp.Models;
using SkyQuizApp.ViewModels.Teacher;

namespace SkyQuizApp.Views.Teacher
{
    public partial class CreateEditQuestionWindow : Window
    {
        public CreateEditQuestionWindow(Question? question = null)
        {
            InitializeComponent();
            var vm = new CreateEditQuestionViewModel(question);
            vm.WindowResources = this.Resources;
            DataContext = vm;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}