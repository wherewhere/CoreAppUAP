using CoreAppUAP.Common;
using CoreAppUAP.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using WinRT;

namespace CoreAppUAP.ViewModels.SettingsPages
{
    public partial class SettingsViewModel : INotifyPropertyChanged
    {
        public static ConditionalWeakTable<CoreDispatcher, SettingsViewModel> Caches { get; } = [];

        public static string SDKVersion { get; } = Assembly.GetAssembly(typeof(PackageSignatureKind)).GetName().Version.ToString();

        public static string WinRTVersion { get; } = Assembly.GetAssembly(typeof(TrustLevel)).GetName().Version.ToString(3);

        public static string DeviceFamily { get; } = AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        public static string VersionTextBlockText { get; } = $"{Package.Current.DisplayName} v{Package.Current.Id.Version.ToFormattedString(3)}";

        public CoreDispatcher Dispatcher { get; }

        public bool IsExtendsTitleBar
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar);
            set
            {
                if (IsExtendsTitleBar != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsExtendsTitleBar, value);
                    ThemeHelper.UpdateExtendViewIntoTitleBar(value);
                    ThemeHelper.UpdateSystemCaptionButtonColors();
                    RaisePropertyChangedEvent();
                }
            }
        }

        private static bool isCleanLogsButtonEnabled = true;
        public bool IsCleanLogsButtonEnabled
        {
            get => isCleanLogsButtonEnabled;
            set => SetProperty(ref isCleanLogsButtonEnabled, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected static async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<CoreDispatcher, SettingsViewModel> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name));
                }
            }
        }

        protected static async void RaisePropertyChangedEvent(params string[] names)
        {
            if (names != null)
            {
                foreach (KeyValuePair<CoreDispatcher, SettingsViewModel> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    names.ForEach(name => cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name)));
                }
            }
        }

        [SuppressMessage("Performance", "CA1822:将成员标记为 static", Justification = "<挂起>")]
        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public SettingsViewModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Caches.AddOrUpdate(dispatcher, this);
        }

        public async Task<bool> OpenLogFileAsync()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            if (files is [StorageFile file, ..])
            {
                await Dispatcher.ResumeForegroundAsync();
                return await Launcher.LaunchFileAsync(file);
            }
            return false;
        }

        public async Task CleanLogsAsync()
        {
            IsCleanLogsButtonEnabled = false;
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
                await folder.DeleteAsync();
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<SettingsViewModel>().LogError(ex, "Failed to clean the logs. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }
            finally
            {
                IsCleanLogsButtonEnabled = true;
            }
        }

        public void Refresh(bool reset)
        {
            if (reset)
            {
                RaisePropertyChangedEvent(nameof(IsExtendsTitleBar));
            }
        }
    }
}
