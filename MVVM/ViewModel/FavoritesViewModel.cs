using DocStack.Core;
using DocStack.MVVM.Model;
using System;
using System.Collections.Generic;
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
using Microsoft.Win32;


namespace DocStack.MVVM.ViewModel
{
    public class FavoritesViewModel : INotifyPropertyChanged
    {
        private readonly ModelDatabase _database;
        private ObservableCollection<Paper> _favoritePapers;
        private ObservableCollection<Paper> _filteredPapers;
        private bool _isDataGridView = true;
        private string _searchQuery;


        // Add new properties
        private string _selectedFilterOption;

        private Paper _selectedPaper;

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
            SelectedFilterOption = "None"; // Default value
            AddPaperToFavoritesCommand = new RelayCommand(o => AddExternalPaperAsync().ConfigureAwait(false));


            SearchCommand = new RelayCommand(o => SearchPapers());



            Task.Run(async () => await LoadFavoritesAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public ICommand AddPaperToFavoritesCommand { get; }

        public ICommand ChangeColorCommand { get; }

        public ObservableCollection<Paper> FavoritePapers
        {
            get => _favoritePapers;
            set
            {
                _favoritePapers = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Paper> FilteredPapers
        {
            get => _filteredPapers;
            set
            {
                _filteredPapers = value;
                OnPropertyChanged();
            }
        }

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

        public ICommand LoadFavoritesCommand { get; }

        public ICommand LocatePDFCommand { get; }

        public ICommand RefreshCommand { get; }

        public ICommand SearchCommand { get; }
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

        public string SelectedFilterOption
        {
            get => _selectedFilterOption;
            set
            {
                _selectedFilterOption = value;
                OnPropertyChanged();
                ApplySelectedFilter();
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
        public ICommand SwitchViewCommand { get; }
        public void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchPapers();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        //A method for adding  papers from external source into favoites 
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
                    await _database.AddToFavoritesAsync(newPaper);
                    await LoadFavoritesAsync(); // Refresh the list

                    MessageBox.Show("Paper added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding paper: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ApplySelectedFilter()
        {
            switch (SelectedFilterOption)
            {
                case "By Journal":
                    FilterByJournal();
                    break;
                case "By Year":
                    FilterByYear();
                    break;
                case "By Color":
                    FilterByColor();
                    break;
                default:
                    // Reset to original order
                    FilteredPapers = new ObservableCollection<Paper>(FavoritePapers);
                    break;
            }
        }

        private bool CanLocatePDF()
        {
            return SelectedPaper != null &&
                   (!string.IsNullOrWhiteSpace(SelectedPaper.FullTextLink) ||
                    !string.IsNullOrWhiteSpace(SelectedPaper.DOI));
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

        private bool ContainsIgnoreCase(string source, string searchTerm)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(searchTerm))
                return false;

            return source.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
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
        private void FilterByColor()
        {
            // Color priority: Red > Blue > Green > Orange > Purple > Yellow > Grey
            var colorPriority = new Dictionary<string, int>
    {
        { "#F44336", 1 }, // Red
        { "#2196F3", 2 }, // Blue
        { "#4CAF50", 3 }, // Green
        { "#FF9800", 4 }, // Orange
        { "#9C27B0", 5 }, // Purple
        { "#FFEB3B", 6 }, // Yellow
        { "#9E9E9E", 7 }  // Grey
    };

            var colorOrderedPapers = FavoritePapers
                .OrderBy(p => colorPriority.ContainsKey(p.ColorCode)
                    ? colorPriority[p.ColorCode]
                    : int.MaxValue)              // Sort by color priority
                .ThenBy(p => p.Title);           // Sort by title within each color group

            FilteredPapers = new ObservableCollection<Paper>(colorOrderedPapers);
        }

        private void FilterByJournal()
        {
        var groupedPapers = FavoritePapers
            .GroupBy(p => p.Journal)
            .OrderBy(g => g.Key)
            .SelectMany(g => g.OrderBy(p => p.Title));

        FilteredPapers = new ObservableCollection<Paper>(groupedPapers);
    }

        private void FilterByYear()
        {
            var orderedPapers = FavoritePapers
                .OrderBy(p => p.Year)               // Sort by year in ascending order (from oldest)
                .ThenBy(p => p.Journal)             // Group by journal name
                .ThenBy(p => p.Title);              // Sort by title within each journal

            FilteredPapers = new ObservableCollection<Paper>(orderedPapers);
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

        private void SwitchView(object parameter)
        {
            if (parameter is string viewType)
            {
                IsDataGridView = viewType == "DataGrid";
            }
        }
    }
}