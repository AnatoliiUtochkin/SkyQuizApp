// ✅ UsersTab.xaml.cs (code-behind)
using System.Windows;
using System.Windows.Controls;
using SkyQuizApp.Models;
using SkyQuizApp.ViewModels.Admin.Tabs;

namespace SkyQuizApp.Views.Admin.Tabs
{
    public partial class UsersTab : UserControl
    {
        private UsersTabViewModel ViewModel => (UsersTabViewModel)DataContext;

        public UsersTab(UsersTabViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UsersDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                UsersDataGrid.CommitEdit();

                var newUser = ViewModel.AddUser();

                UsersDataGrid.Items.Refresh();
                UsersDataGrid.ScrollIntoView(newUser);
                UsersDataGrid.SelectedItem = newUser;
                UsersDataGrid.CurrentCell = new DataGridCellInfo(newUser, UsersDataGrid.Columns[1]);
                UsersDataGrid.BeginEdit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося створити нового користувача:\n{ex.Message}",
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement)?.DataContext as User;
            if (user == null) return;

            if (MessageBox.Show("Ви дійсно хочете видалити цього користувача?",
                                "Підтвердження",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    ViewModel.DeleteUser(user);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося видалити користувача:\n{ex.Message}",
                                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ConfirmUserButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not User user)
                return;

            var currentUser = ViewModel.CurrentUser;
            if (currentUser == null)
                return;

            if (user.UserID == currentUser.UserID)
            {
                MessageBox.Show("Ви не можете редагувати власний акаунт.", "Обмеження", MessageBoxButton.OK, MessageBoxImage.Warning);
                ViewModel.ReloadUser(user.UserID);
                return;
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                MessageBox.Show("Email не може бути порожнім.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!user.Email.Contains("@"))
            {
                MessageBox.Show("Email має містити символ '@'.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ViewModel.UpdateUser(user);
                MessageBox.Show("Користувача успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UsersDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header?.ToString() == "Пароль (хеш)" && e.Row.Item is User user && user.UserID == 0)
            {
                if (e.EditingElement is TextBox textbox)
                {
                    string plainPassword = textbox.Text;
                    user.PasswordHash = ViewModel.HashPassword(plainPassword);
                }
            }
        }

        public void CommitDataGridEdits()
        {
            if (UsersDataGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                UsersDataGrid.CommitEdit();
            }
        }
    }
}