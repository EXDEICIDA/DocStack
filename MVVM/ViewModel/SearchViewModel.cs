using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using DocStack.MVVM.Model;
using DocStack.Core;
using System.Diagnostics;
using System.Windows;

namespace DocStack.MVVM.ViewModel
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        private readonly CallConfigurationModel _callConfigurationModel;
        private string _searchQuery;
        private ObservableCollection<Paper> _searchResults;
        private Paper _selectedPaper;
        private bool _isDetailsPanelVisible;
        private readonly ModelDatabase _database;


        public SearchViewModel()
        {
            _callConfigurationModel = new CallConfigurationModel();
            SearchResults = new ObservableCollection<Paper>();
            _database = new ModelDatabase();

            SearchCommand = new RelayCommand(param => SearchPapers(), param => CanSearch());
            LocatePDFCommand = new RelayCommand(param => LocatePDF(), param => CanLocatePDF());
            AddPaperCommand = new RelayCommand(async param => await AddPaper(), param => CanAddPaper());

        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }



        public ObservableCollection<Paper> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged();
            }
        }

        public Paper SelectedPaper
        {
            get => _selectedPaper;
            set
            {
                _selectedPaper = value;
                IsDetailsPanelVisible = _selectedPaper != null;
                OnPropertyChanged();
            }
        }

        public bool IsDetailsPanelVisible
        {
            get => _isDetailsPanelVisible;
            set
            {
                _isDetailsPanelVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand LocatePDFCommand { get; }
        public ICommand AddPaperCommand { get; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task AddPaper()
        {
            if (SelectedPaper != null)
            {
                await _database.AddPaperAsync(SelectedPaper);
                MessageBox.Show("Paper added to the database successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanAddPaper()
        {
            return SelectedPaper != null;
        }

        private async void SearchPapers()
        {
            var results = await _callConfigurationModel.SearchPapersAsync(SearchQuery);
            SearchResults.Clear();
            foreach (var result in results)
            {
                SearchResults.Add(new Paper
                {
                    Authors = string.Join(", ", result["authors"].Select(a => a["name"].ToString())),
                    Title = result["title"].ToString(),
                    Journal = result["publisher"].ToString(),
                    Year = int.Parse(result["yearPublished"].ToString()),
                    DOI = result["doi"].ToString(),
                    FullTextLink = result["downloadUrl"]?.ToString()
                });
            }
        }

        private bool CanSearch()
        {
            return !string.IsNullOrWhiteSpace(SearchQuery);
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

        private bool CanLocatePDF()
        {
            return SelectedPaper != null &&
                   (!string.IsNullOrWhiteSpace(SelectedPaper.FullTextLink) ||
                    !string.IsNullOrWhiteSpace(SelectedPaper.DOI));
        }
    }
    public class Paper
    {
        public int PaperID { get; set; }
        public string Authors { get; set; }
        public string Title { get; set; }
        public string Journal { get; set; }
        public int Year { get; set; }  // Changed from string to int
        public string DOI { get; set; }
        public string FullTextLink { get; set; }
    }
}