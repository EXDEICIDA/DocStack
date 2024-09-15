using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Cerberus.Core;

namespace DocStack.MVVM.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private bool _isLanguagePopupOpen;
        public bool IsLanguagePopupOpen
        {
            get => _isLanguagePopupOpen;
            set
            {
                _isLanguagePopupOpen = value;
                OnPropertyChanged(nameof(IsLanguagePopupOpen));
            }
        }

        public ObservableCollection<string> Languages { get; set; }

        public ICommand OpenLanguagePopupCommand { get; }
        public ICommand CloseLanguagePopupCommand { get; }
        public ICommand SelectLanguageCommand { get; }

        public SettingsViewModel()
        {
            // Sample languages, replace with actual languages if needed
            Languages = new ObservableCollection<string> { "English", "Spanish", "French" };

            // Pass an Action<object> delegate to the RelayCommand constructor
            OpenLanguagePopupCommand = new RelayCommand(param => OpenLanguagePopup());
            CloseLanguagePopupCommand = new RelayCommand(param => CloseLanguagePopup());
            SelectLanguageCommand = new RelayCommand(param => SelectLanguage(param as string));
        }

        private void OpenLanguagePopup()
        {
            IsLanguagePopupOpen = true;
        }

        private void CloseLanguagePopup()
        {
            IsLanguagePopupOpen = false;
        }

        private void SelectLanguage(string language)
        {
            if (language != null)
            {
                // Handle language selection
                Console.WriteLine($"Selected language: {language}");
                CloseLanguagePopup();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
