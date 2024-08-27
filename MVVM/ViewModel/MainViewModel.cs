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





        public HomeViewModel HomeVm { get; set; }



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


            //initializations for other commands or properties
            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVm;
            });
        }
    }
}
