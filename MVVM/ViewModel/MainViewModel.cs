using Cerberus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocStack.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        /*This class will help us to create relay commands which 
        * pla a role as for coordinating and moving between pages
        * binding this relay commands to the button in our MainWindow
        */

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand PapersViewCommand { get; set; }
        public RelayCommand SettingsViewCommand { get; set; }
        public RelayCommand SearchViewCommand { get; set; }
        public RelayCommand FavoriteViewCommand { get; set; }
        



        public HomeViewModel HomeVm { get; set; }
        public PapersViewModel PapersVm { get; set; }
        public SettingsViewModel SettingsVm { get; set; }
        public SearchViewModel SearchVm { get; set; }
        public FavoritesViewModel FavoritesVm { get; set; }


        private object _currentView;


        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public MainViewModel()
        {
            HomeVm = new HomeViewModel();
            CurrentView = HomeVm;
            PapersVm = new PapersViewModel();
            SettingsVm = new SettingsViewModel();
            SearchVm = new SearchViewModel();
            FavoritesVm = new FavoritesViewModel();

            //initializations for other commands or properties
            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVm;
            });

            FavoriteViewCommand = new RelayCommand( o =>
            {
                CurrentView = FavoritesVm;
                                
            });


            SearchViewCommand = new RelayCommand(o =>
            {
                CurrentView = SearchVm;
            });


            PapersViewCommand = new RelayCommand(o =>
            {
                CurrentView = PapersVm;
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsVm;
            });


        }
    }
}
