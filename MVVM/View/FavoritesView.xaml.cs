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
            /*
            if (sender is Button button && button.DataContext is DocumentItem selectedItem)
            {
                ColorPopup.DataContext = selectedItem;
                ColorPopup.PlacementTarget = button;
                ColorPopup.IsOpen = true;
            }
            */
        }

        private void ColorSelectionButton_Click(object sender, RoutedEventArgs e)
        {   /*
            if (sender is Button button && ColorPopup.DataContext is DocumentItem selectedItem)
            {
                selectedItem.ColorCode = button.Background;
                ColorPopup.IsOpen = false;
            }
            */
        }

        private void CloseColorPopup_Click(object sender, RoutedEventArgs e)
        {
            ColorPopup.IsOpen = false;
        }
    }
}