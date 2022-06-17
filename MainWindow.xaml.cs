using System.Windows;
using System.Windows.Controls;
using GlobalKeyboardHooker;
using MemeBox.ViewModels;

namespace MemeBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;
        private GlobalKeyboardHook globalKeyboardHook;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            globalKeyboardHook = new();
            globalKeyboardHook.KeyboardPressed += viewModel.OnKeyDownGlobal;
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

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            globalKeyboardHook?.Dispose();
        }
    }
}
