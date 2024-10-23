using DocStack.Core;
using DocStack.MVVM.Model;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.IO;


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
        public ICommand AddExternalPaperCommand { get; }
        public ICommand SwitchViewCommand { get; }



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
            AddExternalPaperCommand = new RelayCommand(o => AddExternalPaperAsync().ConfigureAwait(false));
            SwitchViewCommand = new RelayCommand(SwitchView);



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

        private void SwitchView(object parameter)
        {
            if (parameter is string viewType)
            {
                IsDataGridView = viewType == "DataGrid";
            }
        }

        private async Task AddExternalPaperAsync()
        {
            try
            {
                // Create OpenFileDialog
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    Title = "Select a PDF paper"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(filePath);

                    // Extract metadata from PDF
                    var metadata = await ExtractPDFMetadataAsync(filePath);

                    // Create new Paper object
                    Paper newPaper = new Paper
                    {
                        Title = metadata.Title ?? fileName,
                        Authors = metadata.Authors ?? "Unknown Authors",
                        Year = metadata.Year ?? DateTime.Now.Year,
                        Journal = metadata.Journal ?? "Unknown Journal",
                        FullTextLink = filePath, // Store the local file path
                        ColorCode = "#808080" // Default color
                    };

                    // Add to database
                    await _database.AddPaperAsync(newPaper);
                    await LoadPapersAsync(); // Refresh the list

                    MessageBox.Show("Paper added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding paper: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<(string Title, string Authors, int? Year, string Journal)> ExtractPDFMetadataAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfReader pdfReader = new PdfReader(filePath))
                    using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                    {
                        // Get document info
                        var info = pdfDoc.GetDocumentInfo();

                        // Extract first page text for additional metadata
                        var strategy = new LocationTextExtractionStrategy();
                        var firstPageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetFirstPage(), strategy);

                        // Try to parse title (first line of first page, usually)
                        string title = info.GetTitle();
                        if (string.IsNullOrEmpty(title))
                        {
                            var firstLines = firstPageText.Split('\n');
                            title = firstLines.FirstOrDefault()?.Trim() ?? Path.GetFileNameWithoutExtension(filePath);
                        }

                        // Try to get authors
                        string authors = info.GetAuthor();
                        if (string.IsNullOrEmpty(authors))
                        {
                            // Try to find authors in first page text
                            // This is a simple approach - might need refinement
                            var lines = firstPageText.Split('\n');
                            authors = lines.Skip(1).FirstOrDefault()?.Trim() ?? "Unknown Authors";
                        }

                        // Try to extract year
                        int? year = null;
                        var yearMatch = System.Text.RegularExpressions.Regex.Match(firstPageText, @"(?:19|20)\d{2}");
                        if (yearMatch.Success)
                        {
                            year = int.Parse(yearMatch.Value);
                        }

                        // Try to extract journal name
                        string journal = "Unknown Journal";
                        // Look for common journal indicators in first page
                        var journalIndicators = new[] { "Journal of", "Proceedings of", "IEEE", "ACM" };
                        foreach (var indicator in journalIndicators)
                        {
                            var index = firstPageText.IndexOf(indicator);
                            if (index >= 0)
                            {
                                var endIndex = firstPageText.IndexOf('\n', index);
                                if (endIndex > index)
                                {
                                    journal = firstPageText.Substring(index, endIndex - index).Trim();
                                    break;
                                }
                            }
                        }

                        return (title, authors, year, journal);
                    }
                }
                catch
                {
                    // If metadata extraction fails, return null values
                    return (null, null, null, null);
                }
            });
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