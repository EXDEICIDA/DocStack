using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using Cerberus.Core;
using System;
using System.Linq;
using DocStack.MVVM.Model;

namespace DocStack.MVVM.ViewModel
{
    public class DocumentsViewModel : INotifyPropertyChanged
    {
        private readonly ModelDatabase _database;
        private ObservableCollection<DocumentsModel> _documents;
        
        private DocumentsModel _selectedDocument;


        public ObservableCollection<DocumentsModel> Documents
        {
            get => _documents;
            set
            {
                _documents = value;
                OnPropertyChanged();
            }
        }

      

      

        public DocumentsModel SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                _selectedDocument = value;
                OnPropertyChanged();
            }
        }

        //Commands
        public ICommand OpenFileCommand { get; private set; }
        public ICommand ShareCommand { get; private set; }


        public DocumentsViewModel()
        {
            _database = new ModelDatabase();
            Documents = new ObservableCollection<DocumentsModel>();
            OpenFileCommand = new RelayCommand(OpenFile, CanOpenFile);
            ShareCommand = new RelayCommand(ShareFile, CanShareFile);

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

        private void ShareFile(object parameter)
        {
            if (parameter is DocumentsModel document)
            {
                try
                {
                    string subject = Uri.EscapeDataString($"Sharing document: {document.Name}");
                    string body = Uri.EscapeDataString($"I'm sharing the following document with you: {document.Name}\n\nFile path: {document.FilePath}");
                    string mailtoUri = $"mailto:?subject={subject}&body={body}";
                    Process.Start(new ProcessStartInfo(mailtoUri) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sharing file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private bool CanShareFile(object parameter)
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