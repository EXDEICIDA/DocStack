using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocStack.MVVM.Model;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using Cerberus.Core;
using System;

namespace DocStack.MVVM.ViewModel
{
    public class DocumentsViewModel : INotifyPropertyChanged
    {
        private readonly ModelDatabase _database;
        private ObservableCollection<DocumentsModel> _documents;
        private bool _isListView = true;

        public ObservableCollection<DocumentsModel> Documents
        {
            get => _documents;
            set
            {
                _documents = value;
                OnPropertyChanged();
            }
        }

        public bool IsListView
        {
            get => _isListView;
            set
            {
                _isListView = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenFileCommand { get; private set; }

        public DocumentsViewModel()
        {
            _database = new ModelDatabase();
            Documents = new ObservableCollection<DocumentsModel>();
            OpenFileCommand = new RelayCommand(OpenFile, CanOpenFile);

            _ = RefreshDocumentsAsync();
        }


        private void OpenFile(object parameter)
        {
            if (parameter is DocumentsModel document)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(document.FilePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanOpenFile(object parameter)
        {
            return parameter is DocumentsModel;
        }

        public async Task RefreshDocumentsAsync()
        {
            var documents = await _database.GetAllDocumentsAsync();
            Documents.Clear();
            foreach (var document in documents)
            {
                Documents.Add(document);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}