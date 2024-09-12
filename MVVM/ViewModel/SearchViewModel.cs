using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using DocStack.MVVM.Model;
using Cerberus.Core;

namespace DocStack.MVVM.ViewModel
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        private readonly CallConfigurationModel _callConfigurationModel;
        private string _searchQuery;
        private ObservableCollection<Paper> _searchResults;

        public SearchViewModel()
        {
            _callConfigurationModel = new CallConfigurationModel();
            SearchResults = new ObservableCollection<Paper>();
            SearchCommand = new RelayCommand(param => SearchPapers(), param => CanSearch());
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

        public ICommand SearchCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    Year = result["yearPublished"].ToString()
                });
            }
        }

        private bool CanSearch()
        {
            return !string.IsNullOrWhiteSpace(SearchQuery);
        }
    }

    public class Paper
    {
        public string Authors { get; set; }
        public string Title { get; set; }
        public string Journal { get; set; }
        public string Year { get; set; }
    }
}