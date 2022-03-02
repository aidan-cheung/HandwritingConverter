using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.ApplicationModel.Core;
using muxc = Microsoft.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel;

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

            string appVersion = string.Format("Handwriting Converter {0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);

            versiontext.Text = appVersion;
        }
        /*
        private void OnThemeRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var selectedTheme = ((RadioButton)sender)?.Tag?.ToString();
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (selectedTheme != null)
            {
                ThemeHelper.RootTheme = App.GetEnum<ElementTheme>(selectedTheme);
            }
        }

        private void OnThemeRadioButtonKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Up)
            {
                NavigationRootPage.Current.PageHeader.Focus(FocusState.Programmatic);
            }
        }*/
    }
}
