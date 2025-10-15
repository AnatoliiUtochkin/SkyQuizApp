using System.Windows;

namespace SkyQuizApp.Services.Interfaces
{
    public interface IWindowService
    {
        void SetCurrentWindow(Window window);

        void OpenWindow<T>() where T : Window;

        void CloseWindow();

        void HideWindow();

        void ShowWindow();
    }
}