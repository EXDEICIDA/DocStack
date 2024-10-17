using DocStack.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;
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

      



        private void CloseColorPopup_Click(object sender, RoutedEventArgs e)
        {
            ColorPopup.IsOpen = false;
        }
    }
}