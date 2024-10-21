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
    public class FavoritesViewModel : INotifyPropertyChanged
    {
        private readonly ModelDatabase _database;
        private ObservableCollection<Paper> _favoritePapers;
        private ObservableCollection<Paper> _filteredPapers;
        private string _searchQuery;


        public ObservableCollection<Paper> FavoritePapers
        {
            get => _favoritePapers;
            set
            {
                _favoritePapers = value;
                OnPropertyChanged();
            }
        }

        private Paper _selectedPaper;
        public Paper SelectedPaper
        {
            get => _selectedPaper;
            set
            {
                _selectedPaper = value;
                OnPropertyChanged();
            }
        }


        private bool _isDataGridView = true;
        public bool IsDataGridView
        {
            get => _isDataGridView;
            set
            {
                _isDataGridView = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsListView));
            }
        }
        public bool IsListView => !IsDataGridView;

        public ObservableCollection<Paper> FilteredPapers
        {
            get => _filteredPapers;
            set
            {
                _filteredPapers = value;
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

        public ICommand LoadFavoritesCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ChangeColorCommand { get; }
        public ICommand LocatePDFCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SwitchViewCommand { get; }





        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FavoritesViewModel()
        {
            _database = new ModelDatabase();
            FavoritePapers = new ObservableCollection<Paper>();
            FilteredPapers = new ObservableCollection<Paper>();


            LoadFavoritesCommand = new RelayCommand(o => LoadFavoritesAsync().ConfigureAwait(false));
            RefreshCommand = new RelayCommand(o => LoadFavoritesAsync().ConfigureAwait(false));
            ChangeColorCommand = new RelayCommand(ChangeColorAsync);
            LocatePDFCommand = new RelayCommand(param => LocatePDF(), param => CanLocatePDF());
            SwitchViewCommand = new RelayCommand(SwitchView);

            SearchCommand = new RelayCommand(o => SearchPapers());



            Task.Run(async () => await LoadFavoritesAsync());
        }


        private void SwitchView(object parameter)
        {
            if (parameter is string viewType)
            {
                IsDataGridView = viewType == "DataGrid";
            }
        }


        private async Task LoadFavoritesAsync()
        {
            var favoritesList = await _database.GetAllFavoritePapersAsync();
            FavoritePapers.Clear();
            FilteredPapers.Clear();

            foreach (var paper in favoritesList)
            {
                FavoritePapers.Add(paper);
                FilteredPapers.Add(paper);

            }
        }

        private void SearchPapers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredPapers = new ObservableCollection<Paper>(FavoritePapers);
            }
            else
            {
                FilteredPapers = new ObservableCollection<Paper>(
                    FavoritePapers.Where(p =>
                        ContainsIgnoreCase(p.Title, SearchQuery) ||
                        ContainsIgnoreCase(p.Authors?.ToString(), SearchQuery) ||
                        ContainsIgnoreCase(p.DOI, SearchQuery) 
                    )
                );
            }
        }


        //A method for adding external Papers
        private void AddExternalPaper()
        {
          
            /*
              TODO
             */

        }

        public void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchPapers();
            }
        }
        private bool ContainsIgnoreCase(string source, string searchTerm)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(searchTerm))
                return false;

            return source.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private async void ChangeColorAsync(object parameter)
        {
            if (parameter is ValueTuple<Paper, string> tuple)
            {
                var (paper, color) = tuple;
                paper.ColorCode = color;
                await _database.UpdateFavoritePaperColorAsync(paper.DOI, color);
                int index = FavoritePapers.IndexOf(paper);
                if (index != -1)
                {
                    FavoritePapers[index] = paper;
                }
            }
        }

        private bool CanLocatePDF()
        {
            return SelectedPaper != null &&
                   (!string.IsNullOrWhiteSpace(SelectedPaper.FullTextLink) ||
                    !string.IsNullOrWhiteSpace(SelectedPaper.DOI));
        }

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