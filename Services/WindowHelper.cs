using System.Windows;
using System.Windows.Input;

namespace SkyQuizApp.Services
{
    public static class WindowHelper
    {
        public static void HandleDrag(Window window, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                window.DragMove();
        }

        public static void HandleMinimize(Window window)
        {
            window.WindowState = WindowState.Minimized;
        }

        public static void HandleClose(Window window)
        {
            window.Close();
        }
    }
}