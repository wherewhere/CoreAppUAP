using CoreAppUAP.Helpers;
using Microsoft.Extensions.Logging;
using System;
using Windows.ApplicationModel.Search;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace CoreAppUAP.Common
{
    public static class SettingsPaneRegister
    {
        public static void Register(Window window)
        {
            try
            {
                SettingsPane settingsPane = SettingsPane.GetForCurrentView();
                settingsPane.CommandsRequested -= OnCommandsRequested;
                settingsPane.CommandsRequested += OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                window.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(SettingsPaneRegister)).LogError(ex, "Failed to register settings pane. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }
        }

        public static void Unregister(Window window)
        {
            try
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(SettingsPaneRegister)).LogError(ex, "Failed to unregister settings pane. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }
        }

        private static void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Feedback",
                    "Feedback",
                    handler => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/wherewhere/CoreAppUAP/issues"))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "LogFolder",
                    "LogFolder",
                    async handler => _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Repository",
                    "Repository",
                    handler => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/wherewhere/CoreAppUAP"))));
        }

        private static void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType is CoreAcceleratorKeyEventType.KeyDown or CoreAcceleratorKeyEventType.SystemKeyDown)
            {
                CoreWindow window = CoreWindow.GetForCurrentThread();
                CoreVirtualKeyStates ctrl = window.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CoreVirtualKeyStates shift = window.GetKeyState(VirtualKey.Shift);
                    if (shift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        switch (args.VirtualKey)
                        {
                            case VirtualKey.X:
                                SettingsPane.Show();
                                args.Handled = true;
                                break;
                            case VirtualKey.Q:
                                SearchPane.GetForCurrentView().Show();
                                args.Handled = true;
                                break;
                        }
                    }
                }
            }
        }
    }
}
