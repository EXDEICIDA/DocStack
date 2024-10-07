﻿using System.Collections.ObjectModel;
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
using System.IO;
using Microsoft.Win32;



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
        public ICommand ExportAllCommand { get; private set; }


        public DocumentsViewModel()
        {
            _database = new ModelDatabase();
            Documents = new ObservableCollection<DocumentsModel>();
            OpenFileCommand = new RelayCommand(OpenFile, CanOpenFile);
            ShareCommand = new RelayCommand(ShareFile, CanShareFile);
            ExportAllCommand = new RelayCommand(ExportAll, CanExportAll);

            _ = RefreshDocumentsAsync();
        }

        private void ExportAll(object parameter)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Select folder to export documents",
                    FileName = "Select Folder", // Dummy filename
                    CheckFileExists = false,
                    CheckPathExists = true,
                    OverwritePrompt = false
                };

                if (dialog.ShowDialog() == true)
                {
                    string targetFolder = Path.GetDirectoryName(dialog.FileName);
                    string exportFolderName = "ExportedDocuments_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string exportPath = Path.Combine(targetFolder, exportFolderName);

                    Directory.CreateDirectory(exportPath);

                    int successCount = 0;
                    int failCount = 0;

                    foreach (var document in Documents)
                    {
                        try
                        {
                            string destinationPath = Path.Combine(exportPath, Path.GetFileName(document.FilePath));
                            File.Copy(document.FilePath, destinationPath, true);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            Debug.WriteLine($"Failed to copy {document.FilePath}: {ex.Message}");
                        }
                    }

                    MessageBox.Show(
                        $"Export completed.\nSuccessful: {successCount}\nFailed: {failCount}\nLocation: {exportPath}",
                        "Export Results",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during export: {ex.Message}",
                    "Export Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private bool CanExportAll(object parameter)
        {
            return Documents.Any();
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