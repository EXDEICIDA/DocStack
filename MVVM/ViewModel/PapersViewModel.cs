using DocStack.Core;
using DocStack.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace DocStack.MVVM.ViewModel
{
    public class PapersViewModel : INotifyPropertyChanged
    {
        private readonly ModelDatabase _database;
        private ObservableCollection<Paper> _papers;
        private ModelDatabase _modelDatabase;
        private Paper _selectedPaper;
        private string _searchText;


        public ObservableCollection<Paper> Papers
        {
            get => _papers;
            set
            {
                _papers = value;
                OnPropertyChanged();
            }
        }

        // Selected paper property for binding and use in commands
        public Paper SelectedPaper
        {
            get => _selectedPaper;
            set
            {
                _selectedPaper = value;
                OnPropertyChanged();
            }
        }

        //for search purpose on the view 
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                
                }
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
           //Init
            Papers = new ObservableCollection<Paper>();
            _database = new ModelDatabase();
            Papers = new ObservableCollection<Paper>();

            _modelDatabase = new ModelDatabase();
            Papers = new ObservableCollection<Paper>();

   

            //Relay Commands
            RefreshCommand = new RelayCommand(o => LoadPapersAsync().ConfigureAwait(false));

            AddPaperCommand = new RelayCommand(o => AddPaper((Paper)o).ConfigureAwait(false));


            AddToFavoritesCommand = new RelayCommand(async o => await AddToFavoritesAsync(SelectedPaper), o => SelectedPaper != null);
           // SearchCommand = new RelayCommand(o => Search());


            // Load papers initially
            Task.Run(async () => await LoadPapersAsync());
        }


        //Simple Text Search method

        public void Search()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                // If the search text is empty, reload all papers
                Task.Run(async () => await LoadPapersAsync());
                return;
            }

            // Perform a case-insensitive search across multiple fields with null checks
            var filteredPapers = _papers.Where(paper =>
                (!string.IsNullOrEmpty(paper.Title) && paper.Title.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(paper.Authors) && paper.Authors.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(paper.Journal) && paper.Journal.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(paper.DOI) && paper.DOI.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (paper.Year.ToString().IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            // Clear the current list of papers and add only the filtered ones
            Papers.Clear();
            foreach (var paper in filteredPapers)
            {
                Papers.Add(paper);
            }
        }



        private async Task AddPaper(Paper paper)
        {
            if (paper != null)
            {
                await _database.AddPaperAsync(paper);
                await LoadPapersAsync(); // Reload papers to refresh the list
            }
        }


        private async Task LoadPapersAsync()
        {
            var papersList = await _modelDatabase.GetAllPapersAsync();
            Papers.Clear();
            foreach (var paper in papersList)
            {
                Papers.Add(paper);
            }
        }

        // Method for adding the selected paper to the favorites
        private async Task AddToFavoritesAsync(Paper paper)
        {
            if (paper != null)
            {
                await _database.AddToFavoritesAsync(paper);
            }
        }
    }

}
