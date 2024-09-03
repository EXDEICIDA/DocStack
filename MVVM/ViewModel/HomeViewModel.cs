using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Cerberus.Core;

namespace DocStack.MVVM.ViewModel
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private DateTime _currentDate = DateTime.Now;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CalendarDay> Days { get; set; }

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

        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand TodayCommand { get; }

        public HomeViewModel()
        {
            Days = new ObservableCollection<CalendarDay>();
            PreviousMonthCommand = new RelayCommand(PreviousMonth);
            NextMonthCommand = new RelayCommand(NextMonth);
            TodayCommand = new RelayCommand(GoToToday);
            UpdateCalendarDays();
        }

        private void UpdateCalendarDays()
        {
            Days.Clear();
            var firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int daysToAdd = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
            var currentDay = firstDayOfMonth.AddDays(-daysToAdd);

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

        private void PreviousMonth(object obj) => CurrentDate = CurrentDate.AddMonths(-1);
        private void NextMonth(object obj) => CurrentDate = CurrentDate.AddMonths(1);
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