using CoreAppUAP.Common;
using CoreAppUAP.Helpers;
using CoreAppUAP.ViewModels.SettingsPages;
using System;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Search;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoreAppUAP.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel Provider;

        public SettingsPage()
        {
            InitializeComponent();
            Provider = new SettingsViewModel(Dispatcher);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeHelper.UISettingChanged += OnUISettingChanged;
            UpdateThemeRadio();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ThemeHelper.UISettingChanged -= OnUISettingChanged;
        }

        private void OnUISettingChanged(ApplicationTheme mode) => UpdateThemeRadio();

        private async void UpdateThemeRadio()
        {
            ElementTheme theme = await ThemeHelper.GetActualThemeAsync().ConfigureAwait(false);
            await Dispatcher.ResumeForegroundAsync();
            switch (theme)
            {
                case ElementTheme.Light:
                    Light.IsChecked = true;
                    break;
                case ElementTheme.Dark:
                    Dark.IsChecked = true;
                    break;
                case ElementTheme.Default:
                    Default.IsChecked = true;
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Tag?.ToString())
            {
                case "Reset":
                    SettingsHelper.LocalObject.Clear();
                    SettingsHelper.SetDefaultSettings();
                    if (Reset.Flyout is Flyout flyout_reset)
                    { flyout_reset.Hide(); }
                    Refresh(true);
                    break;
                case "FeedBack":
                    _ = Launcher.LaunchUriAsync(new Uri("https://github.com/wherewhere/CoreAppUAP/issues"));
                    break;
                case "LogFolder":
                    _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                case "NewWindow":
                    _ = await WindowHelper.CreateWindowAsync(window =>
                    {
                        if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                        { CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true; }
                        Frame _frame = new();
                        window.Content = _frame;
                        ThemeHelper.Initialize(window);
                        _ = _frame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                    });
                    break;
                case "NewAppWindow":
                    if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                    {
                        AppWindow newWindow = await AppWindow.TryCreateAsync();
                        if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                        { newWindow.TitleBar.ExtendsContentIntoTitleBar = true; }
                    }
                    break;
                case "SearchFlyout":
                    SearchPane.GetForCurrentView().Show();
                    break;
                case "ExitFullWindow":
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
                    break;
                case "SettingsFlyout":
                    SettingsPane.Show();
                    break;
                case "EnterFullWindow":
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    break;
            }
        }

        private void Button_Checked(object sender, RoutedEventArgs _)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Name)
            {
                case nameof(Dark):
                    ThemeHelper.RootTheme = ElementTheme.Dark;
                    break;
                case nameof(Light):
                    ThemeHelper.RootTheme = ElementTheme.Light;
                    break;
                case nameof(Default):
                    ThemeHelper.RootTheme = ElementTheme.Default;
                    break;
                default:
                    break;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Tag.ToString())
            {
                case "CleanLogs":
                    _ = Provider.CleanLogsAsync();
                    break;
                case "OpenLogFile":
                    _ = Provider.OpenLogFileAsync();
                    break;
                default:
                    break;
            }
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void Rectangle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            if (sender is not FrameworkElement frameworkElement) { return; }
            DataPackage dataPackage = new();
            dataPackage.SetText(frameworkElement.Tag?.ToString());
            Clipboard.SetContent(dataPackage);
            if (e != null) { e.Handled = true; }
        }

        public void Refresh(bool reset = false) => Provider.Refresh(reset);
    }
}
