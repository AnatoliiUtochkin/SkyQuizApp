using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Enums;
using SkyQuizApp.Models;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.ViewModels.Teacher
{
    public class CreateEditQuestionViewModel : INotifyPropertyChanged
    {
        private readonly Question? _existingQuestion;
        private readonly ILogService _logger;

        public ResourceDictionary? WindowResources { get; set; }

        public string QuestionText { get; set; } = string.Empty;
        public string ShortAnswerText { get; set; } = string.Empty;

        private QuestionType _selectedQuestionType = QuestionType.SingleChoice;

        public QuestionType SelectedQuestionType
        {
            get => _selectedQuestionType;
            set
            {
                _selectedQuestionType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DynamicContent));
            }
        }

        private QuestionTypeOption _selectedQuestionTypeOption;

        public QuestionTypeOption SelectedQuestionTypeOption
        {
            get => _selectedQuestionTypeOption;
            set
            {
                if (_selectedQuestionTypeOption != value)
                {
                    _selectedQuestionTypeOption = value;
                    SelectedQuestionType = value.Type;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<AnswerViewModel> Answers { get; set; } = new();
        public ObservableCollection<MatchingPairViewModel> MatchingPairs { get; set; } = new();

        public ObservableCollection<QuestionTypeOption> QuestionTypeOptions { get; }

        public ICommand AddAnswerCommand { get; }
        public ICommand RemoveAnswerCommand { get; }
        public ICommand AddMatchingPairCommand { get; }
        public ICommand RemoveMatchingPairCommand { get; }
        public ICommand SaveCommand { get; }

        public object DynamicContent => GenerateDynamicContent();

        public CreateEditQuestionViewModel(Question? question = null)
        {
            _logger = (App.Current as App)!.Services.GetRequiredService<ILogService>();

            AddAnswerCommand = new RelayCommand(_ => AddAnswer());
            RemoveAnswerCommand = new RelayCommand(answer => RemoveAnswer(answer as AnswerViewModel));
            AddMatchingPairCommand = new RelayCommand(_ => AddMatchingPair());
            RemoveMatchingPairCommand = new RelayCommand(pair => RemoveMatchingPair(pair as MatchingPairViewModel));
            SaveCommand = new RelayCommand(_ => Save());

            QuestionTypeOptions = new ObservableCollection<QuestionTypeOption>
            {
                new QuestionTypeOption(QuestionType.SingleChoice, "Одна правильна відповідь"),
                new QuestionTypeOption(QuestionType.MultipleChoice, "Кілька правильних відповідей"),
                new QuestionTypeOption(QuestionType.ShortAnswer, "Коротка відповідь"),
                new QuestionTypeOption(QuestionType.TrueFalse, "Правда / Неправда"),
                new QuestionTypeOption(QuestionType.Matching, "Відповідність (Matching)")
            };
            SelectedQuestionTypeOption = QuestionTypeOptions.FirstOrDefault(q => q.Type == SelectedQuestionType);

            _existingQuestion = question;

            if (question != null)
            {
                QuestionText = question.Text;
                SelectedQuestionType = question.QuestionType;

                if (question.QuestionType == QuestionType.SingleChoice || question.QuestionType == QuestionType.MultipleChoice)
                {
                    Answers = new ObservableCollection<AnswerViewModel>(
                        question.Answers.Select(a => new AnswerViewModel
                        {
                            Text = a.Text,
                            IsCorrect = a.IsCorrect
                        }));
                }
                else if (question.QuestionType == QuestionType.ShortAnswer)
                {
                    ShortAnswerText = question.Answers.FirstOrDefault()?.Text ?? string.Empty;
                }
                else if (question.QuestionType == QuestionType.TrueFalse)
                {
                    ShortAnswerText = question.Answers.FirstOrDefault()?.Text == "True" ? "True" : "False";
                }
                else if (question.QuestionType == QuestionType.Matching)
                {
                    MatchingPairs = new ObservableCollection<MatchingPairViewModel>(
                        question.Answers.Select(a =>
                        {
                            var parts = a.Text.Split('=');
                            return new MatchingPairViewModel
                            {
                                Left = parts.ElementAtOrDefault(0) ?? "",
                                Right = parts.ElementAtOrDefault(1) ?? ""
                            };
                        }));
                }
            }
        }

        public CreateEditQuestionViewModel(ILogService logger, Question? question = null)
            : this(question)
        {
            _logger = logger;
        }

        private UIElement GenerateDynamicContent()
        {
            switch (SelectedQuestionType)
            {
                case QuestionType.SingleChoice:
                case QuestionType.MultipleChoice:
                    return GenerateAnswersEditor();

                case QuestionType.ShortAnswer:
                    return GenerateShortAnswerEditor();

                case QuestionType.TrueFalse:
                    return GenerateTrueFalseEditor();

                case QuestionType.Matching:
                    return GenerateMatchingEditor();

                default:
                    return new TextBlock { Text = "Невідомий тип питання", Foreground = Brushes.Red };
            }
        }

        private UIElement GenerateAnswersEditor()
        {
            var stack = new StackPanel();

            var deleteStyle = WindowResources?["RedRoundedButton"] as Style;
            var addStyle = WindowResources?["PurpleRoundedButton"] as Style;

            foreach (var answer in Answers)
            {
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };

                var tb = new TextBox
                {
                    Width = 200,
                    Text = answer.Text,
                    FontSize = 14,
                    Background = (Brush)new BrushConverter().ConvertFrom("#2C2F48")!,
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(6),
                    Margin = new Thickness(0),
                    Height = 32
                };
                tb.TextChanged += (s, e) => answer.Text = tb.Text;

                var cb = new CheckBox
                {
                    IsChecked = answer.IsCorrect,
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    FontSize = 14
                };
                cb.Checked += (s, e) => answer.IsCorrect = true;
                cb.Unchecked += (s, e) => answer.IsCorrect = false;

                var btn = new Button
                {
                    Content = "Видалити",
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 32,
                    FontSize = 14,
                    Padding = new Thickness(10, 2, 10, 2),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Style = deleteStyle
                };
                btn.Click += (s, e) => RemoveAnswer(answer);

                sp.Children.Add(tb);
                sp.Children.Add(cb);
                sp.Children.Add(btn);
                stack.Children.Add(sp);
            }

            var addButton = new Button
            {
                Content = "Додати варіант",
                Margin = new Thickness(0, 10, 0, 0),
                Height = 32,
                FontSize = 14,
                Padding = new Thickness(10, 2, 10, 2),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Style = addStyle
            };
            addButton.Click += (s, e) => AddAnswer();

            stack.Children.Add(addButton);
            return stack;
        }

        private UIElement GenerateShortAnswerEditor()
        {
            var tb = new TextBox
            {
                Width = 300,
                Text = ShortAnswerText,
                FontSize = 14,
                Background = (Brush)new BrushConverter().ConvertFrom("#2C2F48")!,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(6),
                Height = 32
            };
            tb.TextChanged += (s, e) => ShortAnswerText = tb.Text;
            return tb;
        }

        private UIElement GenerateTrueFalseEditor()
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

            var trueRadio = new RadioButton
            {
                Content = "Правда",
                GroupName = "TrueFalse",
                Margin = new Thickness(0, 0, 10, 0),
                IsChecked = ShortAnswerText == "True",
                Foreground = Brushes.White,
                FontSize = 14
            };

            var falseRadio = new RadioButton
            {
                Content = "Неправда",
                GroupName = "TrueFalse",
                IsChecked = ShortAnswerText == "False",
                Foreground = Brushes.White,
                FontSize = 14
            };

            trueRadio.Checked += (s, e) => ShortAnswerText = "True";
            falseRadio.Checked += (s, e) => ShortAnswerText = "False";

            stack.Children.Add(trueRadio);
            stack.Children.Add(falseRadio);

            return stack;
        }

        private UIElement GenerateMatchingEditor()
        {
            var stack = new StackPanel();

            var deleteStyle = WindowResources?["RedRoundedButton"] as Style;
            var addStyle = WindowResources?["PurpleRoundedButton"] as Style;

            foreach (var pair in MatchingPairs)
            {
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

                var tbLeft = new TextBox
                {
                    Width = 150,
                    Text = pair.Left,
                    FontSize = 14,
                    Background = (Brush)new BrushConverter().ConvertFrom("#2C2F48")!,
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(6),
                    Height = 32
                };

                var tbRight = new TextBox
                {
                    Width = 150,
                    Text = pair.Right,
                    FontSize = 14,
                    Background = (Brush)new BrushConverter().ConvertFrom("#2C2F48")!,
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(6),
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 32
                };

                tbLeft.TextChanged += (s, e) => pair.Left = tbLeft.Text;
                tbRight.TextChanged += (s, e) => pair.Right = tbRight.Text;

                var btn = new Button
                {
                    Content = "Видалити",
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 32,
                    FontSize = 14,
                    Padding = new Thickness(10, 2, 10, 2),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Style = deleteStyle
                };
                btn.Click += (s, e) => RemoveMatchingPair(pair);

                sp.Children.Add(tbLeft);
                sp.Children.Add(tbRight);
                sp.Children.Add(btn);
                stack.Children.Add(sp);
            }

            var addButton = new Button
            {
                Content = "Додати пару",
                Margin = new Thickness(0, 10, 0, 0),
                Height = 32,
                FontSize = 14,
                Padding = new Thickness(10, 2, 10, 2),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Style = addStyle
            };
            addButton.Click += (s, e) => AddMatchingPair();
            stack.Children.Add(addButton);

            return stack;
        }

        private void AddAnswer()
        {
            Answers.Add(new AnswerViewModel());
            OnPropertyChanged(nameof(DynamicContent));
        }

        private void RemoveAnswer(AnswerViewModel? answer)
        {
            if (answer != null)
            {
                _logger.Log(LogAction.AnswerDeleted, $"Видалено відповідь: {answer.Text}");
                Answers.Remove(answer);
                OnPropertyChanged(nameof(DynamicContent));
            }
        }

        private void AddMatchingPair()
        {
            MatchingPairs.Add(new MatchingPairViewModel());
            OnPropertyChanged(nameof(DynamicContent));
        }

        private void RemoveMatchingPair(MatchingPairViewModel? pair)
        {
            if (pair != null)
            {
                _logger.Log(LogAction.AnswerDeleted, $"Видалено пару: {pair.Left} = {pair.Right}");
                MatchingPairs.Remove(pair);
                OnPropertyChanged(nameof(DynamicContent));
            }
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(QuestionText))
            {
                MessageBox.Show("Текст запитання обов'язковий.");
                return;
            }

            switch (SelectedQuestionType)
            {
                case QuestionType.SingleChoice:
                    if (Answers.Count < 2 || Answers.Count(a => a.IsCorrect) != 1)
                    {
                        MessageBox.Show("Має бути щонайменше 2 варіанти та одна правильна відповідь.");
                        return;
                    }
                    break;

                case QuestionType.MultipleChoice:
                    if (Answers.Count < 2 || Answers.All(a => !a.IsCorrect))
                    {
                        MessageBox.Show("Має бути щонайменше 2 варіанти та хоча б одна правильна.");
                        return;
                    }
                    break;

                case QuestionType.ShortAnswer:
                    if (string.IsNullOrWhiteSpace(ShortAnswerText))
                    {
                        MessageBox.Show("Правильна відповідь обов'язкова.");
                        return;
                    }
                    break;

                case QuestionType.TrueFalse:
                    if (ShortAnswerText != "True" && ShortAnswerText != "False")
                    {
                        MessageBox.Show("Оберіть True або False.");
                        return;
                    }
                    break;

                case QuestionType.Matching:
                    if (MatchingPairs.Count < 1 || MatchingPairs.Any(p => string.IsNullOrWhiteSpace(p.Left) || string.IsNullOrWhiteSpace(p.Right)))
                    {
                        MessageBox.Show("Усі пари мають бути заповнені.");
                        return;
                    }
                    break;
            }

            var question = new Question
            {
                Text = QuestionText,
                QuestionType = SelectedQuestionType,
                CreatedAt = DateTime.UtcNow,
                Answers = new List<Answer>()
            };

            if (SelectedQuestionType == QuestionType.SingleChoice || SelectedQuestionType == QuestionType.MultipleChoice)
            {
                question.Answers = Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect }).ToList();
            }
            else if (SelectedQuestionType == QuestionType.ShortAnswer || SelectedQuestionType == QuestionType.TrueFalse)
            {
                question.Answers.Add(new Answer { Text = ShortAnswerText, IsCorrect = true });
            }
            else if (SelectedQuestionType == QuestionType.Matching)
            {
                question.Answers = MatchingPairs.Select(p => new Answer { Text = $"{p.Left}={p.Right}", IsCorrect = true }).ToList();
            }

            var isEdit = _existingQuestion != null;
            var window = App.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (window != null)
            {
                window.Tag = question;
                window.DialogResult = true;
                window.Close();
            }

            _logger.Log(isEdit ? LogAction.QuestionUpdated : LogAction.QuestionCreated, $"{(isEdit ? "Оновлено" : "Створено")} питання: {question.Text}");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class QuestionTypeOption
    {
        public QuestionType Type { get; }
        public string DisplayName { get; }

        public QuestionTypeOption(QuestionType type, string displayName)
        {
            Type = type;
            DisplayName = displayName;
        }
    }

    public class AnswerViewModel
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class MatchingPairViewModel
    {
        public string Left { get; set; } = string.Empty;
        public string Right { get; set; } = string.Empty;
    }
}