using Microsoft.Win32;
using System;
using System.Windows.Controls;
using System.IO;
using DocStack.MVVM.ViewModel;
using DocStack.MVVM.Model;

namespace DocStack.MVVM.View
{
    public partial class DocumentsView : UserControl
    {
        private readonly ModelDatabase _database;
        private readonly DocumentsViewModel _viewModel;

        public DocumentsView()
        {
            InitializeComponent();
            _database = new ModelDatabase();
            _viewModel = DataContext as DocumentsViewModel;
        }

        private async void OpenFileDialog_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|PDF files (*.pdf)|*.pdf|Markdown files (*.md)|*.md|Research articles (*.doc;*.docx)|*.doc;*.docx|Academic papers (*.tex)|*.tex|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);
                long fileSize = new FileInfo(filePath).Length;
                string dateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await _database.AddDocumentAsync(fileName, filePath, fileSize, dateAdded);
                await _viewModel.RefreshDocumentsAsync();
            }
        }
    }
}