using CoreAppUAP.Pages.SettingsPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace CoreAppUAP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            MainFrame.ContentTransitions = [new NavigationThemeTransition()];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainFrame.Navigate(typeof(HomePage));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Tag?.ToString())
            {
                case "Home":
                    if (MainFrame.Content is not HomePage)
                    {
                        MainFrame.Navigate(typeof(HomePage));
                    }
                    break;
                case "Setting":
                    if (MainFrame.Content is not SettingsPage)
                    {
                        MainFrame.Navigate(typeof(SettingsPage));
                    }
                    break;
            }
        }
    }
}
