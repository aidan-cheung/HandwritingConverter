using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Storage;
using System;
using Windows.UI.Xaml;
using System.IO;
using Microsoft.Data.Sqlite;

namespace HandwritingConverter
{
    /*
    The following code uses modified samples from the Microsoft UWP Documentation, 
    these samples are taken from the following pages granted under the MIT license:

    - https://docs.microsoft.com/en-us/windows/apps/design/controls/navigationview
    */
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            string appVersion = $"Handwriting Converter {Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build} Alpha";

            versiontext.Text = appVersion;
        }

        private async void openDb(object sender, RoutedEventArgs e)
        {
            StorageFolder db_folder = ApplicationData.Current.LocalFolder;
            await Windows.System.Launcher.LaunchFolderAsync(db_folder);
        }
    }
}
