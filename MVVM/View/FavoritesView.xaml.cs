using DocStack.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DocStack.MVVM.View
{
    public partial class FavoritesView : UserControl
    {
        public FavoritesView()
        {
            InitializeComponent();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Paper paper)
            {
                ColorPopup.DataContext = paper;
                ColorPopup.PlacementTarget = button;
                ColorPopup.IsOpen = true;
            }
        }

        private void ColorSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button colorButton &&
                colorButton.Background is SolidColorBrush brush &&
                DataContext is FavoritesViewModel viewModel &&
                ColorPopup.DataContext is Paper paper)
            {
                string colorCode = brush.Color.ToString();
                viewModel.ChangeColorCommand.Execute((paper, colorCode));
                ColorPopup.IsOpen = false;

                // Force the DataGrid to refresh
                FavoritesDataGrid.Items.Refresh();
            }
        }

        private void OpenDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var paper = (Paper)button.CommandParameter;

            var viewModel = (FavoritesViewModel)DataContext;
            viewModel.SelectedPaper = paper;

            DetailPanelColumn.Width = new GridLength(300);
            DetailPanel.Visibility = Visibility.Visible;
            GridSplitter gridSplitter = (GridSplitter)this.FindName("GridSplitter");
            gridSplitter.Visibility = Visibility.Visible;
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is FavoritesViewModel viewModel)
            {
                viewModel.OnSearchKeyDown(sender, e);
            }
        }

        private void CloseColorPopup_Click(object sender, RoutedEventArgs e)
        {
            ColorPopup.IsOpen = false;
        }
    }
}