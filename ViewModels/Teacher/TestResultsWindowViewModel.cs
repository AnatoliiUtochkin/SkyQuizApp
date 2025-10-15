using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SkyQuizApp.Commands;
using SkyQuizApp.DTOs;
using SkyQuizApp.Enums;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.ViewModels.Teacher
{
    public class TestResultsWindowViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;
        private readonly ILogService _logger;
        private readonly IUserSessionService _session;

        public string TestTitle { get; set; } = string.Empty;
        public string TestDescription { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
        public decimal SuccessRate { get; set; }
        public int TotalAttempts { get; set; }

        public ObservableCollection<StudentResultDto> Results { get; set; } = new();

        public ICommand ExportToPdfCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        public TestResultsWindowViewModel(int testId)
        {
            _services = ((App)System.Windows.Application.Current).Services;
            _logger = _services.GetRequiredService<ILogService>();
            _session = _services.GetRequiredService<IUserSessionService>();

            ExportToPdfCommand = new RelayCommand(_ => ExportToPdf());
            ExportToExcelCommand = new RelayCommand(_ => ExportToExcel());

            LoadData(testId);
        }

        private void LoadData(int testId)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

            var test = db.Tests.FirstOrDefault(t => t.TestID == testId);
            if (test == null) return;

            TestTitle = test.Title;
            TestDescription = test.Description;

            var sessions = db.TestSessions
                .Include(s => s.User)
                .Include(s => s.Result)
                .Where(s => s.TestID == testId && s.Result != null)
                .AsNoTracking()
                .ToList();

            TotalAttempts = sessions.Count;

            var scores = sessions.Select(s => s.Result!.Score).ToList();
            if (scores.Any())
            {
                AverageScore = scores.Average();
                SuccessRate = scores.Count(s => s >= 60) * 100m / scores.Count;
            }

            Results.Clear();
            foreach (var s in sessions)
            {
                var dto = new StudentResultDto
                {
                    FullName = s.User?.FullName ?? "(невідомо)",
                    CompletedAt = s.CompletedAt,
                    Score = $"{s.Result!.Score:F1}%",
                    Grade12 = ConvertTo12Scale(s.Result.Score),
                    Grade100 = ConvertTo100Scale(s.Result.Score)
                };

                Results.Add(dto);
            }

            _logger.Log(LogAction.ResultViewed, $"Викладач переглянув результати тесту: {TestTitle}", _session.CurrentUser?.UserID ?? 0);

            OnPropertyChanged(nameof(TestTitle));
            OnPropertyChanged(nameof(TestDescription));
            OnPropertyChanged(nameof(AverageScore));
            OnPropertyChanged(nameof(SuccessRate));
            OnPropertyChanged(nameof(TotalAttempts));
            OnPropertyChanged(nameof(Results));
        }

        private void ExportToPdf()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Результати_{TestTitle}_{DateTime.Now:dd.MM.yyyy_HH-mm}.pdf"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Orientation = PdfSharp.PageOrientation.Landscape;
                var gfx = XGraphics.FromPdfPage(page);
                var fontTitle = new XFont("Verdana", 16, XFontStyleEx.Bold);
                var fontHeader = new XFont("Verdana", 12, XFontStyleEx.Bold);
                var fontRow = new XFont("Verdana", 11);

                gfx.DrawString("Результати тесту: " + TestTitle, fontTitle, XBrushes.DarkBlue,
                    new XRect(40, 30, page.Width - 80, 30), XStringFormats.TopLeft);

                int y = 70;
                int rowHeight = 25;

                gfx.DrawString("ПІБ", fontHeader, XBrushes.Black, new XRect(40, y, 200, rowHeight), XStringFormats.CenterLeft);
                gfx.DrawString("Дата", fontHeader, XBrushes.Black, new XRect(250, y, 150, rowHeight), XStringFormats.CenterLeft);
                gfx.DrawString("%", fontHeader, XBrushes.Black, new XRect(410, y, 60, rowHeight), XStringFormats.Center);
                gfx.DrawString("12 б.", fontHeader, XBrushes.Black, new XRect(480, y, 60, rowHeight), XStringFormats.Center);
                gfx.DrawString("100 б.", fontHeader, XBrushes.Black, new XRect(550, y, 60, rowHeight), XStringFormats.Center);

                y += rowHeight;
                foreach (var r in Results)
                {
                    gfx.DrawString(r.FullName, fontRow, XBrushes.Black, new XRect(40, y, 200, rowHeight), XStringFormats.CenterLeft);
                    gfx.DrawString(r.CompletedAt.ToString("yyyy-MM-dd HH:mm"), fontRow, XBrushes.Black, new XRect(250, y, 150, rowHeight), XStringFormats.CenterLeft);
                    gfx.DrawString(r.Score, fontRow, XBrushes.Black, new XRect(410, y, 60, rowHeight), XStringFormats.Center);
                    gfx.DrawString(r.Grade12, fontRow, XBrushes.Black, new XRect(480, y, 60, rowHeight), XStringFormats.Center);
                    gfx.DrawString(r.Grade100, fontRow, XBrushes.Black, new XRect(550, y, 60, rowHeight), XStringFormats.Center);
                    y += rowHeight;

                    if (y > page.Height - 60)
                    {
                        page = doc.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;
                    }
                }

                doc.Save(dialog.FileName);
            }
        }

        private void ExportToExcel()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Результати_{TestTitle}_{DateTime.Now:dd.MM.yyyy_HH-mm}.xlsx"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Результати");
                
                ws.Cell(1, 1).Value = "ПІБ";
                ws.Cell(1, 2).Value = "Дата проходження";
                ws.Cell(1, 3).Value = "Оцінка (%)";
                ws.Cell(1, 4).Value = "Оцінка (12 б.)";
                ws.Cell(1, 5).Value = "Оцінка (100 б.)";

                var headerRange = ws.Range("A1:E1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                
                int row = 2;
                foreach (var r in Results)
                {
                    ws.Cell(row, 1).Value = r.FullName;
                    ws.Cell(row, 2).Value = r.CompletedAt.ToString("yyyy-MM-dd HH:mm");
                    ws.Cell(row, 3).Value = r.Score;
                    ws.Cell(row, 4).Value = r.Grade12;
                    ws.Cell(row, 5).Value = r.Grade100;

                    for (int col = 1; col <= 5; col++)
                    {
                        var cell = ws.Cell(row, col);
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    row++;
                }
                
                ws.Columns().AdjustToContents();

                wb.SaveAs(dialog.FileName);
            }
        }

        private string ConvertTo12Scale(decimal score)
        {
            if (score >= 90) return "12";
            if (score >= 85) return "11";
            if (score >= 80) return "10";
            if (score >= 75) return "9";
            if (score >= 70) return "8";
            if (score >= 65) return "7";
            if (score >= 60) return "6";
            if (score >= 55) return "5";
            if (score >= 50) return "4";
            if (score >= 45) return "3";
            if (score >= 35) return "2";
            return "1";
        }

        private string ConvertTo100Scale(decimal score)
        {
            return ((int)Math.Round(score)).ToString();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}