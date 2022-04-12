using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Storage;
using System;
using Windows.UI.Xaml;

namespace HandwritingConverter
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            string appVersion = $"Handwriting Converter {Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build} Beta";
            versiontext.Text = appVersion;
        }

        private async void OpenDb(object sender, RoutedEventArgs e)
        {
            StorageFolder db_folder = ApplicationData.Current.LocalFolder;
            await Windows.System.Launcher.LaunchFolderAsync(db_folder);
        }
    }
}
