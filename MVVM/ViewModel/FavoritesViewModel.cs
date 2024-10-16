using DocStack.Core;
using DocStack.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public ICommand LoadFavoritesCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ChangeColorCommand { get; }



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FavoritesViewModel()
        {
            _database = new ModelDatabase();
            FavoritePapers = new ObservableCollection<Paper>();

            LoadFavoritesCommand = new RelayCommand(o => LoadFavoritesAsync().ConfigureAwait(false));
            RefreshCommand = new RelayCommand(o => LoadFavoritesAsync().ConfigureAwait(false));
            ChangeColorCommand = new RelayCommand(ChangeColorAsync);

            Task.Run(async () => await LoadFavoritesAsync());
        }

        private async Task LoadFavoritesAsync()
        {
            var favoritesList = await _database.GetAllFavoritePapersAsync();
            FavoritePapers.Clear();
            foreach (var paper in favoritesList)
            {
                FavoritePapers.Add(paper);
            }
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
    }
}