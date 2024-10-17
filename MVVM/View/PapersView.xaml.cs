using DocStack.MVVM.ViewModel;
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
    /// Interaction logic for PapersView.xaml
    /// </summary>
    public partial class PapersView : UserControl
    {
        public PapersView()
        {
            InitializeComponent();
        }

        //A simple search method for Papers view search bar

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Get the view model from the DataContext
                var viewModel = DataContext as PapersViewModel;

                // If the view model and the search text are valid, call the Search method
                if (viewModel != null && !string.IsNullOrWhiteSpace(viewModel.SearchText))
                {
                    viewModel.Search();
                }
            }
        }
    }
}
