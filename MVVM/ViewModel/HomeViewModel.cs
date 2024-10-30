using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DocStack.Core;
using System.Globalization;
using System.Linq;
using DocStack.MVVM.Model;
using LiveCharts;
using LiveCharts.Wpf;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

namespace DocStack.MVVM.ViewModel
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
    }

    public class HomeViewModel : INotifyPropertyChanged
    {

        private const int KeywordsPerCategory = 3;
        // Get 3 keywords from each category
        private const int TotalPapersLimit = 30;

        private readonly Calendar _calendar;
        private readonly ModelDatabase _database;
        private readonly KeywordManager _keywordManager;
        private readonly RecommenderModel _recommender;
        private SeriesCollection _chartSeries;
        private DateTime _currentDate = DateTime.Now;
        private bool _isLoadingRecommendations;
        private ObservableCollection<DocumentsModel> _recentDocuments;

        // Add to properties
        public ObservableCollection<DocumentsModel> RecentDocuments
        {
            get => _recentDocuments;
            set
            {
                _recentDocuments = value;
                OnPropertyChanged(nameof(RecentDocuments));
            }
        }

        private int _paperCount;

        private ObservableCollection<RecommenderModel.RecommendedPaper> _recommendedPapers;

        public HomeViewModel()
        {
            _database = new ModelDatabase();

            _calendar = CultureInfo.CurrentCulture.Calendar;
            Days = new ObservableCollection<CalendarDay>();

            PreviousMonthCommand = new RelayCommand(PreviousMonth);
            NextMonthCommand = new RelayCommand(NextMonth);
            TodayCommand = new RelayCommand(GoToToday);
            InitializeChartSeries();
            UpdateCalendarDays();
            // Add to existing constructor
            _recommender = new RecommenderModel();
            _recommendedPapers = new ObservableCollection<RecommenderModel.RecommendedPaper>();
            _recentDocuments = new ObservableCollection<DocumentsModel>();

            // Fix: Properly pass the parameter to LocatePDF and CanLocatePDF methods
            LocatePDFCommand = new RelayCommand(
                param => LocatePDF(param as RecommenderModel.RecommendedPaper),
                param => CanLocatePDF(param as RecommenderModel.RecommendedPaper)
            );
            _keywordManager = new KeywordManager();
            OpenDocumentCommand = new RelayCommand(OpenDocument);


            Task.Run(async () => await InitializeAsync());


        }

        public event PropertyChangedEventHandler PropertyChanged;
        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            set
            {
                _chartSeries = value;
                OnPropertyChanged(nameof(ChartSeries));
            }
        }

        private void OpenDocument(object parameter)
        {
            if (parameter is DocumentsModel document)
            {
                try
                {
                    var startInfo = new ProcessStartInfo(document.FilePath)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error opening document: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }


        public DateTime CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged(nameof(CurrentDate));
                OnPropertyChanged(nameof(CurrentMonthYear));
                UpdateCalendarDays();
            }
        }

        public string CurrentMonthYear => CurrentDate.ToString("MMMM yyyy");
        public ObservableCollection<CalendarDay> Days { get; set; }
        public bool HasNoRecommendations => !IsLoadingRecommendations && (!RecommendedPapers?.Any() ?? true);
        public bool IsLoadingRecommendations
        {
            get => _isLoadingRecommendations;
            set
            {
                _isLoadingRecommendations = value;
                OnPropertyChanged(nameof(IsLoadingRecommendations));
                OnPropertyChanged(nameof(HasNoRecommendations));
            }
        }

        public ICommand LocatePDFCommand { get; }

        public ICommand NextMonthCommand { get; }
        public ICommand OpenDocumentCommand { get; private set; }


        public int PaperCount
        {
            get => _paperCount;
            set
            {
                _paperCount = value;
                OnPropertyChanged(nameof(PaperCount));

            }
        }

        public ICommand PreviousMonthCommand { get; }

        public ObservableCollection<RecommenderModel.RecommendedPaper> RecommendedPapers
        {
            get => _recommendedPapers;
            set
            {
                _recommendedPapers = value;
                OnPropertyChanged(nameof(RecommendedPapers));
                OnPropertyChanged(nameof(HasNoRecommendations));
            }
        }

        public ICommand TodayCommand { get; }

        // Aim for 30 total papers
        public void LocatePDF(RecommenderModel.RecommendedPaper paper)
        {
            if (paper != null)
            {
                string url = null;
                if (!string.IsNullOrWhiteSpace(paper.FullTextLink))
                {
                    url = paper.FullTextLink;
                }
                else if (!string.IsNullOrWhiteSpace(paper.Doi))
                {
                    url = $"https://doi.org/{paper.Doi}";
                }

                if (url != null)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No direct link or DOI available for this paper.", "Link Unavailable",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        // Add this method
        private async Task LoadRecentDocumentsAsync()
        {
            try
            {
                var documents = await _database.GetRecentDocumentsAsync(10);
                App.Current.Dispatcher.Invoke(() =>
                {
                    RecentDocuments.Clear();
                    foreach (var doc in documents)
                    {
                        RecentDocuments.Add(doc);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading recent documents: {ex.Message}");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanLocatePDF(RecommenderModel.RecommendedPaper paper)
        {
            return paper != null &&
                   (!string.IsNullOrWhiteSpace(paper.FullTextLink) ||
                    !string.IsNullOrWhiteSpace(paper.Doi));
        }
        private Task<string[]> GetUserKeywordsAsync()
        {
            // Get 3 random keywords from each of the 6 categories (18 total keywords)
            var keywords = _keywordManager.GetRandomKeywordsFromCategories(KeywordsPerCategory);
            Debug.WriteLine($"Selected {keywords.Length} keywords: {string.Join(", ", keywords)}");
            return Task.FromResult(keywords);
        }

        private void GoToToday(object obj) => CurrentDate = DateTime.Today;

        private async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("Starting initialization");
                await Task.WhenAll(
                    LoadRecommendationsAsync(),
                    LoadPaperCountAsync(),
                    LoadRecentDocumentsAsync()
                );
                Debug.WriteLine("Initialization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeAsync: {ex.Message}");
            }
        }

        private void InitializeChartSeries()
        {
            ChartSeries = new SeriesCollection
        {
            new PieSeries
            {
                Title = "Papers",
                Values = new ChartValues<int> { PaperCount },
                DataLabels = true,
                LabelPoint = point => point.Y.ToString()
            }
        };
        }

        private async Task LoadPaperCountAsync()
        {
            PaperCount = await _database.GetPaperCountAsync();

            if (ChartSeries?.Any() == true)
            {
                ((ChartValues<int>)ChartSeries[0].Values)[0] = PaperCount;
            }
        }

        private async Task LoadRecommendationsAsync()
        {
            try
            {
                IsLoadingRecommendations = true;
                Debug.WriteLine("Starting to load recommendations");

                RecommendedPapers.Clear();

                var keywords = await GetUserKeywordsAsync();
                Debug.WriteLine($"Using {keywords.Length} keywords");

                var recommendations = await _recommender.GetRecommendationsAsync(keywords, limit: TotalPapersLimit);
                Debug.WriteLine($"Received {recommendations?.Count ?? 0} recommendations");

                if (recommendations != null && recommendations.Any())
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        RecommendedPapers = new ObservableCollection<RecommenderModel.RecommendedPaper>(recommendations);
                        Debug.WriteLine($"Updated UI with {RecommendedPapers.Count} papers");
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadRecommendationsAsync: {ex.Message}");
            }
            finally
            {
                IsLoadingRecommendations = false;
            }
        }

        private void NextMonth(object obj) => CurrentDate = _calendar.AddMonths(CurrentDate, 1);

        private void PreviousMonth(object obj) => CurrentDate = _calendar.AddMonths(CurrentDate, -1);

        private void UpdateCalendarDays()
        {
            Days.Clear();
            var firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            var lastDayOfMonth = _calendar.AddMonths(firstDayOfMonth, 1).AddDays(-1);

            int firstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int currentDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            int daysToSubtract = (currentDayOfWeek - firstDayOfWeek + 7) % 7;

            var currentDay = firstDayOfMonth.AddDays(-daysToSubtract);

            while (currentDay <= lastDayOfMonth || Days.Count % 7 != 0)
            {
                Days.Add(new CalendarDay
                {
                    Date = currentDay,
                    IsCurrentMonth = currentDay.Month == CurrentDate.Month,
                    IsToday = currentDay.Date == DateTime.Today
                });
                currentDay = currentDay.AddDays(1);
            }
        }
    }
}