using DocStack.MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DocStack.MVVM.ViewModel
{
    public class FavoritesViewModel : INotifyPropertyChanged
    {
        private readonly ModelDatabase _database;
        private ObservableCollection<Paper> _favoritePapers;

        public ObservableCollection<Paper> FavoritePapers
        {
            get => _favoritePapers;
            set
            {
                _favoritePapers = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FavoritesViewModel()
        {
            _database = new ModelDatabase();
            FavoritePapers = new ObservableCollection<Paper>();
            Task.Run(async () => await LoadFavoritePapersAsync());
        }

        // Fetch favorite papers from the database
        private async Task LoadFavoritePapersAsync()
        {
            var papersList = await _database.GetAllFavoritePapersAsync();
            FavoritePapers.Clear();
            foreach (var paper in papersList)
            {
                FavoritePapers.Add(paper);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
