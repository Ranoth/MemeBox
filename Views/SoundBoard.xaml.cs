using System.Windows;
using System.Windows.Controls;

namespace MemeBox.Views
{
    /// <summary>
    /// Interaction logic for SoundBoard.xaml
    /// </summary>
    public partial class SoundBoard : UserControl
    {
        public SoundBoard()
        {
            InitializeComponent();
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void PositionSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var viewModel = DataContext as ViewModels.SoundBoardViewModel;
            viewModel.PositionChangedCommand.Execute((int)PositionSlider.Value);
        }

        private void PositionSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            var viewModel = DataContext as ViewModels.SoundBoardViewModel;
            viewModel.PositionChangingCommand.Execute(null);
        }
    }
}
