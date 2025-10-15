using System.Windows;
using System.Windows.Controls;
using SkyQuizApp.Models;
using SkyQuizApp.ViewModels.Admin.Tabs;

namespace SkyQuizApp.Views.Admin.Tabs
{
    public partial class TestsTab : UserControl
    {
        private TestsTabViewModel ViewModel => (TestsTabViewModel)DataContext;

        public TestsTab(TestsTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void AddTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newTest = ViewModel.AddTest();
                TestsDataGrid.Items.Refresh();

                TestsDataGrid.ScrollIntoView(newTest);
                TestsDataGrid.SelectedItem = newTest;
                TestsDataGrid.CurrentCell = new DataGridCellInfo(newTest, TestsDataGrid.Columns[1]);
                TestsDataGrid.BeginEdit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося створити новий тест:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTestButton_Click(object sender, RoutedEventArgs e)
        {
            var test = (sender as FrameworkElement)?.DataContext as Test;
            if (test == null) return;

            if (MessageBox.Show("Ви дійсно хочете видалити цей тест?", "Підтвердження",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    ViewModel.DeleteTest(test);
                    TestsDataGrid.Items.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося видалити тест:\n{ex.Message}",
                        "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ConfirmTestButton_Click(object sender, RoutedEventArgs e)
        {
            var test = (sender as FrameworkElement)?.DataContext as Test;
            if (test == null) return;

            if (string.IsNullOrWhiteSpace(test.Title))
            {
                MessageBox.Show("Назва тесту не може бути порожньою.", "Помилка валідації",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (test.TimeLimitMinutes <= 0 || test.TimeLimitMinutes > 300)
            {
                MessageBox.Show("Ліміт часу має бути в межах від 1 до 300 хвилин.", "Помилка валідації",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (test.UserID <= 0)
            {
                MessageBox.Show("ID користувача має бути більше нуля.", "Помилка валідації",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ViewModel.UpdateTest(test);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні тесту:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}