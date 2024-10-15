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
using System.Windows.Media;
using System.Collections.Generic;

namespace DocStack.MVVM.ViewModel
{
    public class Paper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Abstract { get; set; }
        public string Authors { get; set; }
        // To hold the abstract of the paper
        public string ColorCode { get; set; }

        public string DOI { get; set; }
        public string FullTextLink { get; set; }
        public string Journal { get; set; }
        public int PaperID { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }  // Changed from string to int
                                       // Add this property for ColorCode
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class SearchViewModel : INotifyPropertyChanged
    {
        private readonly CallConfigurationModel _callConfigurationModel;
        private readonly ModelDatabase _database;
        private bool _isDetailsPanelVisible;
        private string _searchQuery;
        private ObservableCollection<Paper> _searchResults;
        private Paper _selectedPaper;
        public SearchViewModel()
        {
            _callConfigurationModel = new CallConfigurationModel();
            SearchResults = new ObservableCollection<Paper>();
            _database = new ModelDatabase();

            SearchCommand = new RelayCommand(param => SearchPapers(), param => CanSearch());
            LocatePDFCommand = new RelayCommand(param => LocatePDF(), param => CanLocatePDF());
            AddPaperCommand = new RelayCommand(async param => await AddPaper(), param => CanAddPaper());

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand AddPaperCommand { get; }

        public bool IsDetailsPanelVisible
        {
            get => _isDetailsPanelVisible;
            set
            {
                _isDetailsPanelVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand LocatePDFCommand { get; }

        public ICommand SearchCommand { get; }

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

        private bool CanLocatePDF()
        {
            return SelectedPaper != null &&
                   (!string.IsNullOrWhiteSpace(SelectedPaper.FullTextLink) ||
                    !string.IsNullOrWhiteSpace(SelectedPaper.DOI));
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

        private async void SearchPapers()
        {
    try
    {
        var results = await _callConfigurationModel.SearchPapersAsync(SearchQuery);
        SearchResults.Clear();
        foreach (var result in results)
        {
            var paper = new Paper
            {
                Authors = string.Join(", ", result["authors"]?.Select(a => a["name"]?.ToString() ?? "Unknown") ?? new[] { "Unknown" }),
                Title = result["title"]?.ToString() ?? "Untitled",
                Journal = result["publisher"]?.ToString() ?? "Unknown",
                DOI = result["doi"]?.ToString() ?? string.Empty,
                FullTextLink = result["downloadUrl"]?.ToString() ?? string.Empty,
                Abstract = result["abstract"]?.ToString() ?? string.Empty
            };

            // Safely parse the year
            if (result["yearPublished"] != null && int.TryParse(result["yearPublished"].ToString(), out int year))
            {
                paper.Year = year;
            }
            else
            {
                paper.Year = 0; // Or some default value to indicate unknown year
            }

            SearchResults.Add(paper);
        }

        if (SearchResults.Count == 0)
        {
            MessageBox.Show($"No results found for '{SearchQuery}'. Try different keywords or check your search terms.", "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"An error occurred while searching: {ex.Message}", "Search Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
    }
}