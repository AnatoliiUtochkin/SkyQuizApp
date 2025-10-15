using System.Windows.Controls;
using SkyQuizApp.Data;
using SkyQuizApp.ViewModels.Student.Tabs;

namespace SkyQuizApp.Views.Student.Tabs
{
    public partial class StatisticsTab : UserControl
    {
        public StatisticsTab(AppDbContext dbContext, int currentUserId)
        {
            InitializeComponent();
            DataContext = new StatisticsViewModel(dbContext, currentUserId);
        }
    }
}