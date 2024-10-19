using DocStack.Core;
using DocStack.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DocStack.MVVM.ViewModel
{
    public class PapersViewModel : INotifyPropertyChanged
    {
        private readonly ModelDatabase _database;
        private ObservableCollection<Paper> _allPapers;
        private ObservableCollection<Paper> _filteredPapers;
        private Paper _selectedPaper;
        private string _searchQuery;

        public ObservableCollection<Paper> FilteredPapers
        {
            get => _filteredPapers;
            set
            {
                _filteredPapers = value;
                OnPropertyChanged();
            }
        }

        public Paper SelectedPaper
        {
            get => _selectedPaper;
            set
            {
                _selectedPaper = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                SearchPapers();
            }
        }

        public ICommand AddPaperCommand { get; }
        public ICommand LoadPapersCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddToFavoritesCommand { get; }
        public ICommand SearchCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PapersViewModel()
        {
            _database = new ModelDatabase();
            _allPapers = new ObservableCollection<Paper>();
            FilteredPapers = new ObservableCollection<Paper>();

            LoadPapersCommand = new RelayCommand(o => LoadPapersAsync().ConfigureAwait(false));
            RefreshCommand = new RelayCommand(o => LoadPapersAsync().ConfigureAwait(false));
            AddPaperCommand = new RelayCommand(o => AddPaperAsync((Paper)o).ConfigureAwait(false));
            AddToFavoritesCommand = new RelayCommand(async o => await AddToFavoritesAsync(SelectedPaper), o => SelectedPaper != null);
            SearchCommand = new RelayCommand(o => SearchPapers());

            Task.Run(async () => await LoadPapersAsync());
        }

        private async Task LoadPapersAsync()
        {
            var papersList = await _database.GetAllPapersAsync();
            _allPapers.Clear();
            FilteredPapers.Clear();

            foreach (var paper in papersList)
            {
                _allPapers.Add(paper);
                FilteredPapers.Add(paper);
            }
        }

        private void SearchPapers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredPapers = new ObservableCollection<Paper>(_allPapers);
            }
            else
            {
                FilteredPapers = new ObservableCollection<Paper>(
                    _allPapers.Where(p =>
                        ContainsIgnoreCase(p.Title, SearchQuery) ||
                        ContainsIgnoreCase(p.Authors, SearchQuery) ||
                        ContainsIgnoreCase(p.Journal, SearchQuery) ||
                        ContainsIgnoreCase(p.DOI, SearchQuery) ||
                        ContainsIgnoreCase(p.Year.ToString(), SearchQuery)
                    )
                );
            }
        }

        private bool ContainsIgnoreCase(string source, string searchTerm)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(searchTerm))
                return false;

            return source.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchPapers();
            }
        }

        private async Task AddPaperAsync(Paper paper)
        {
            if (paper != null)
            {
                await _database.AddPaperAsync(paper);
                await LoadPapersAsync();
            }
        }

        private async Task AddToFavoritesAsync(Paper paper)
        {
            if (paper != null)
            {
                await _database.AddToFavoritesAsync(paper);
            }
        }
    }
}