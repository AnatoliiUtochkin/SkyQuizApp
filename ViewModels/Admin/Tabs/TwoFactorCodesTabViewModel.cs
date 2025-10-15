using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Models;

namespace SkyQuizApp.ViewModels.Admin.Tabs
{
    public class TwoFactorCodesTabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;
        public ObservableCollection<TwoFactorCode> TwoFactorCodes { get; } = new();

        public TwoFactorCodesTabViewModel(AppDbContext context)
        {
            _context = context;
            LoadCodesAsync();
        }

        private async void LoadCodesAsync()
        {
            var codes = await _context.TwoFactorCodes
                .OrderBy(c => c.CodeID)
                .AsNoTracking()
                .ToListAsync();

            TwoFactorCodes.Clear();
            foreach (var code in codes)
                TwoFactorCodes.Add(code);
        }

        public void Dispose() => _context.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}