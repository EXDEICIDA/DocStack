using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Linq;

namespace DocStack.MVVM.ViewModel
{
    public class FavoritesViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<DocumentItem> _favoriteDocuments;
        public ObservableCollection<DocumentItem> FavoriteDocuments => _favoriteDocuments;

        public FavoritesViewModel()
        {
            _favoriteDocuments = new ObservableCollection<DocumentItem>
            {
                new DocumentItem
                {
                    ColorCode = Brushes.Blue,
                    Authors = "Cynthia H. Gemmell",
                    Year = 1997,
                    Title = "A flow cytometric immunoassay to quantify ad...",
                    Journal = "Journal of Biomedical Materia...",
                    LastAuthor = "Cynthia H. Gemmell",
                    Rating = 0,
                    LastRead = null
                },
                new DocumentItem
                {
                    ColorCode = Brushes.Orange,
                    Authors = "Byrappa Venkatesh et al.",
                    Year = 2014,
                    Title = "Elephant shark genome provides unique insights i...",
                    Journal = "Nature",
                    LastAuthor = "Wesley C. Warren",
                    Rating = 5,
                    LastRead = new DateTime(2019, 8, 12, 13, 28, 0)
                },
                // Add more sample items here
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DocumentItem : INotifyPropertyChanged
    {
        public Brush ColorCode { get; set; }
        public string Authors { get; set; }
        public int Year { get; set; }
        public string Title { get; set; }
        public string Journal { get; set; }
        public string LastAuthor { get; set; }

        private int _rating;
        public int Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                OnPropertyChanged(nameof(Rating));
                OnPropertyChanged(nameof(RatingStars));
            }
        }

        public DateTime? LastRead { get; set; }

        public ObservableCollection<bool> RatingStars => new ObservableCollection<bool>(Enumerable.Range(0, 5).Select(i => i < Rating));

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}