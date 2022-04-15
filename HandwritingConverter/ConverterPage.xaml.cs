using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HandwritingConverter
{
    public sealed partial class ConverterPage : Page
    {
        private async void AddData(object sender, RoutedEventArgs e)
        {
            if (recognitionResult.Text != "")
            {
                Guid guid = Guid.NewGuid();
                string result = recognitionResult.Text;
                long unix = DateTimeOffset.Now.ToUnixTimeSeconds();

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
                string query = "INSERT INTO convertedText VALUES (@guid, @unix, @result);";

                SqliteConnection connection = new SqliteConnection($"Filename={dbpath}");

                connection.Open();

                SqliteCommand command = new SqliteCommand(query, connection);

                command.Parameters.AddWithValue("@guid", guid);
                command.Parameters.AddWithValue("@unix", unix);
                command.Parameters.AddWithValue("@result", result);

                command.ExecuteReader();

                connection.Close();

                savedFeedback.Glyph = "\uE73E";
                await Task.Delay(1000);
                savedFeedback.Glyph = "\uE74E";
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "Okay";
                dialog.Content = "No text to save";

                await dialog.ShowAsync();
            }
        }

        /*
        The following code uses modified samples from the Microsoft UWP Documentation
        and thus should not be treated as my own code. The samples are taken from the 
        following pages granted under the MIT license:

        - https://docs.microsoft.com/en-us/windows/apps/design/input/ink-walkthrough

        Link to License: https://github.com/MicrosoftDocs/windows-uwp/blob/docs/LICENSE-CODE
        */

        public ConverterPage()
        {
            this.InitializeComponent();

            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;

            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Black;
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);

            convert.Click += Recognize_Click;
        }

        private async void Recognize_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();

            if (currentStrokes.Count > 0)
            {
                InkRecognizerContainer inkRecognizerContainer = new InkRecognizerContainer();

                if (!(inkRecognizerContainer == null))
                {
                    IReadOnlyList<InkRecognitionResult> recognitionResults = await inkRecognizerContainer.RecognizeAsync(inkCanvas.InkPresenter.StrokeContainer, InkRecognitionTarget.All);
                    if (recognitionResults.Count > 0)
                    {
                        string str = "";
                        foreach (var result in recognitionResults)
                        {
                            IReadOnlyList<string> candidate = result.GetTextCandidates();
                            str += candidate[0] + " ";
                        }
                        recognitionResult.Text = str;
                        inkCanvas.InkPresenter.StrokeContainer.Clear();
                    }
                    else
                    {
                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "Error";
                        dialog.PrimaryButtonText = "Okay";
                        dialog.Content = "Couldn't recognise text";

                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog("You must install handwriting recognition engine.");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "Okay";
                dialog.Content = "No text to convert";

                await dialog.ShowAsync();
            }
        }
    }
}
