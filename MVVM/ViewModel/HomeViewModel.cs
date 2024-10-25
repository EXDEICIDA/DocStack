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

namespace DocStack.MVVM.ViewModel
{
    public class HomeViewModel : INotifyPropertyChanged
    {

        private DateTime _currentDate = DateTime.Now;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<CalendarDay> Days { get; set; }
        private readonly ModelDatabase _database;
        private int _paperCount;
        private readonly RecommenderModel _recommender;
        private ObservableCollection<RecommenderModel.RecommendedPaper> _recommendedPapers;
        private bool _isLoadingRecommendations;
        public bool HasNoRecommendations => !IsLoadingRecommendations && (!RecommendedPapers?.Any() ?? true);
        public string CurrentMonthYear => CurrentDate.ToString("MMMM yyyy");
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


        public int PaperCount
        {
            get => _paperCount;
            set
            {
                _paperCount = value;
                OnPropertyChanged(nameof(PaperCount));
                
            }
        }


        private SeriesCollection _chartSeries;
        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            set
            {
                _chartSeries = value;
                OnPropertyChanged(nameof(ChartSeries));
            }
        }


        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand TodayCommand { get; }

        private readonly Calendar _calendar;


    
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
            Task.Run(async () => await InitializeAsync());


        }

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
        private async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("Starting initialization");
                await LoadRecommendationsAsync();
                await LoadPaperCountAsync();
                Debug.WriteLine("Initialization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeAsync: {ex.Message}");
            }
        }
        private async Task LoadRecommendationsAsync()
        {
            try
            {
                IsLoadingRecommendations = true;
                Debug.WriteLine("Starting to load recommendations");

                // Clear existing recommendations
                RecommendedPapers.Clear();

                // Get test keywords
                var keywords = await GetUserKeywordsAsync();
                Debug.WriteLine($"Using keywords: {string.Join(", ", keywords)}");

                // Get recommendations with increased limit
                var recommendations = await _recommender.GetRecommendationsAsync(keywords, limit: 10);
                Debug.WriteLine($"Received {recommendations?.Count ?? 0} recommendations");

                if (recommendations != null && recommendations.Any())
                {
                    // Create new collection on UI thread
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        RecommendedPapers = new ObservableCollection<RecommenderModel.RecommendedPaper>(recommendations);
                        Debug.WriteLine($"Updated UI with {RecommendedPapers.Count} papers");
                    });
                }
                else
                {
                    Debug.WriteLine("No recommendations received");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadRecommendationsAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoadingRecommendations = false;
            }
        }


        private async Task<string[]> GetUserKeywordsAsync()
        {
            // Return more diverse keywords for testing
            return new[]
            {
                "machine learning",
                "artificial intelligence",
                "deep learning",
                "neural networks",
                "data science",
                "quantum computing",
                "cloud computing",
                "blockchain technology"

            };
        }

        // ... rest of your existing HomeViewModel code ...

        private async Task LoadPaperCountAsync()
        {
            PaperCount = await _database.GetPaperCountAsync();

            if (ChartSeries?.Any() == true)
            {
                ((ChartValues<int>)ChartSeries[0].Values)[0] = PaperCount;
            }
        }
        private void PreviousMonth(object obj) => CurrentDate = _calendar.AddMonths(CurrentDate, -1);
        private void NextMonth(object obj) => CurrentDate = _calendar.AddMonths(CurrentDate, 1);
        private void GoToToday(object obj) => CurrentDate = DateTime.Today;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
    }
}