using System.Windows;
using System.Windows.Controls;
using TopazWPF.Windows;
using TopazWPF.Themes;
using TopazWPF.ColorUtils;

namespace TopazWPFTestApp
{
    public partial class MainWindow : CustomChromeWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            

            // Load all styles in App.xaml first!
            this.Topmost = true;

            //ThemeManager.SetPrimaryColor("#809B00");
            //ThemeManager.SetErrorColor("#a03050");
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string indexStr)
            {
                if (int.TryParse(indexStr, out int index))
                {
                    MainTabControl.SelectedIndex = index;
                }


                foreach (var child in TabSwitches.Children.OfType<Button>())
                {
                    child.Style = (child == button)
                        ? (Style)FindResource("CaptionBarNavButtonActiveStyle")
                        : (Style)FindResource("CaptionBarNavButtonStyle");
                }
            }
        }

        private void TopMostToggle(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }
    }
}
