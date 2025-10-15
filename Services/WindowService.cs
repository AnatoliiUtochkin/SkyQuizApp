using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.Services
{
    public class WindowService : IWindowService
    {
        private Window? _currentWindow;
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        public void OpenWindow<T>() where T : Window
        {
            _currentWindow?.Hide();
            var newWindow = _serviceProvider.GetRequiredService<T>();
            newWindow.Show();
        }

        public void CloseWindow()
        {
            _currentWindow?.Close();
        }

        public void HideWindow()
        {
            _currentWindow?.Hide();
        }

        public void ShowWindow()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Show();
                _currentWindow.Activate();
            }
        }
    }
}