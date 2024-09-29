using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocStack.MVVM.Model;

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

        public DocumentsViewModel()
        {
            _database = new ModelDatabase();
            Documents = new ObservableCollection<DocumentsModel>();
            _ = RefreshDocumentsAsync();
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