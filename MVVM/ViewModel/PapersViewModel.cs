using DocStack.Core;
using DocStack.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
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
        private string _selectedFilterOption;


        public ObservableCollection<Paper> FilteredPapers
        {
            get => _filteredPapers;
            set
            {
                _filteredPapers = value;
                OnPropertyChanged();
            }
        }

        public string SelectedFilterOption
        {
            get => _selectedFilterOption;
            set
            {
                _selectedFilterOption = value;
                OnPropertyChanged();
                ApplyFilter();
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
        public ICommand LocatePDFCommand { get; }


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
            _selectedFilterOption = "None"; // Default filter option


            LoadPapersCommand = new RelayCommand(o => LoadPapersAsync().ConfigureAwait(false));
            RefreshCommand = new RelayCommand(o => LoadPapersAsync().ConfigureAwait(false));
            AddPaperCommand = new RelayCommand(o => AddPaperAsync((Paper)o).ConfigureAwait(false));
            AddToFavoritesCommand = new RelayCommand(async o => await AddToFavoritesAsync(SelectedPaper), o => SelectedPaper != null);
            LocatePDFCommand = new RelayCommand(param => LocatePDF(), param => CanLocatePDF());

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

        private void ApplyFilter()
        {
            if (_allPapers == null || !_allPapers.Any())
                return;

            var filteredList = _allPapers.ToList();

            switch (SelectedFilterOption)
            {
                case "By Year":
                    filteredList = _allPapers
                        .OrderByDescending(p => p.Year)
                        .ThenBy(p => p.Title)
                        .ToList();
                    break;

                case "By Journal":
                    filteredList = _allPapers
                        .OrderBy(p => p.Journal)
                        .ThenBy(p => p.Year)
                        .ThenBy(p => p.Title)
                        .ToList();
                    break;

                case "By Color":
                    filteredList = _allPapers
                        .OrderBy(p => p.ColorCode)
                        .ThenBy(p => p.Title)
                        .ToList();
                    break;
            }

            FilteredPapers = new ObservableCollection<Paper>(filteredList);
        }


        private void SearchPapers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                ApplyFilter(); // Apply current filter to all papers
            }
            else
            {
                var searchResults = _allPapers.Where(p =>
                    ContainsIgnoreCase(p.Title, SearchQuery) ||
                    ContainsIgnoreCase(p.Authors, SearchQuery) ||
                    ContainsIgnoreCase(p.Journal, SearchQuery) ||
                    ContainsIgnoreCase(p.DOI, SearchQuery) ||
                    ContainsIgnoreCase(p.Year.ToString(), SearchQuery)
                ).ToList();

                // Apply current filter to search results
                switch (SelectedFilterOption)
                {
                    case "By Year":
                        searchResults = searchResults
                            .OrderByDescending(p => p.Year)
                            .ThenBy(p => p.Title)
                            .ToList();
                        break;

                    case "By Journal":
                        searchResults = searchResults
                            .OrderBy(p => p.Journal)
                            .ThenBy(p => p.Year)
                            .ThenBy(p => p.Title)
                            .ToList();
                        break;

                    case "By Color":
                        searchResults = searchResults
                            .OrderBy(p => p.ColorCode)
                            .ThenBy(p => p.Title)
                            .ToList();
                        break;
                }

                FilteredPapers = new ObservableCollection<Paper>(searchResults);
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




        private bool CanLocatePDF()
        {
            return SelectedPaper != null &&
                   (!string.IsNullOrWhiteSpace(SelectedPaper.FullTextLink) ||
                    !string.IsNullOrWhiteSpace(SelectedPaper.DOI));
        }

        //A private method for locating full text soruce
        private void LocatePDF()
        {
            if (SelectedPaper != null)
            {
                string url = null;
                if (!string.IsNullOrWhiteSpace(SelectedPaper.FullTextLink))
                {
                    url = SelectedPaper.FullTextLink;
                }
                else if (!string.IsNullOrWhiteSpace(SelectedPaper.DOI))
                {
                    url = $"https://doi.org/{SelectedPaper.DOI}";
                }
                if (url != null)
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("No direct link or DOI available for this paper.", "Link Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}