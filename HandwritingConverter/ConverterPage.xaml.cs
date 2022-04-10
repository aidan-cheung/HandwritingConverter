using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using System.Threading.Tasks;

namespace HandwritingConverter
{
    public sealed partial class ConverterPage : Page
    {
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

        private async void AddData(object sender, RoutedEventArgs e)
        {
            if (recognitionResult.Text != "")
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
                using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    Guid guid = Guid.NewGuid();
                    string result = recognitionResult.Text;

                    long unix = DateTimeOffset.Now.ToUnixTimeSeconds();

                    insertCommand.CommandText = $"INSERT INTO convertedText VALUES (@guid, @unix, @result);";
                    insertCommand.Parameters.AddWithValue("@guid", guid);
                    insertCommand.Parameters.AddWithValue("@unix", unix);
                    insertCommand.Parameters.AddWithValue("@result", result);

                    insertCommand.ExecuteReader();

                    db.Close();
                }

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
        The following code uses modified samples from the Microsoft UWP Documentation, 
        these samples are taken from the following pages granted under the MIT license:

        - https://docs.microsoft.com/en-us/windows/apps/design/input/ink-walkthrough
        */

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
