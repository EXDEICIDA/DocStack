using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocStack.MVVM.View
{
    /// <summary>
    /// Interaction logic for DocumentsView.xaml
    /// </summary>
    public partial class DocumentsView : UserControl
    {
        public DocumentsView()
        {
            InitializeComponent();
        }

        private void OpenFileDialog_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = openFileDialog.Filter = "Text files (*.txt)|*.txt|PDF files (*.pdf)|*.pdf|Markdown files (*.md)|*.md|Research articles (*.doc;*.docx)|*.doc;*.docx|Academic papers (*.tex)|*.tex|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                MessageBox.Show("File Selected: " + filePath);
            }
        }
    }
}
