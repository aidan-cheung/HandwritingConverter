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

namespace HandwritingConverter
{
    /*
    The following code uses modified samples from the Microsoft UWP Documentation, 
    these samples are taken from the following pages granted under the MIT license:

    - https://docs.microsoft.com/en-us/windows/apps/design/input/ink-walkthrough

    - https://docs.microsoft.com/en-us/windows/apps/design/controls/navigationview
    */
    public sealed partial class ConverterPage : Page
    {
        public ConverterPage()
        {
            this.InitializeComponent();

            // Set supported inking device types.
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;

            // Set initial ink stroke attributes.
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Black;
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);

            // Listen for button click to initiate recognition.
            convert.Click += Recognize_Click; // change to reference convert button
        }

        private async void Recognize_Click(object sender, RoutedEventArgs e)
        {
            // Get all strokes on the InkCanvas.
            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // Ensure an ink stroke is present.
            if (currentStrokes.Count > 0)
            {
                // Create a manager for the InkRecognizer object 
                // used in handwriting recognition.
                InkRecognizerContainer inkRecognizerContainer =
                    new InkRecognizerContainer();

                // inkRecognizerContainer is null if a recognition engine is not available.
                if (!(inkRecognizerContainer == null))
                {
                    // Recognize all ink strokes on the ink canvas.
                    IReadOnlyList<InkRecognitionResult> recognitionResults =
                        await inkRecognizerContainer.RecognizeAsync(
                            inkCanvas.InkPresenter.StrokeContainer,
                            InkRecognitionTarget.All);
                    // Process and display the recognition results.
                    if (recognitionResults.Count > 0)
                    {
                        string str = "";
                        foreach (var result in recognitionResults)
                        {
                            IReadOnlyList<string> candidate = result.GetTextCandidates();
                            str += candidate[0] + " ";
                        }
                        // Display the recognition candidates.
                        recognitionResult.Text = str;
                        // Clear the ink canvas once recognition is complete.
                        inkCanvas.InkPresenter.StrokeContainer.Clear();
                    }
                    else
                    {
                        recognitionResult.Text = "No recognition results.";
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
                recognitionResult.Text = "No ink strokes to recognize.";
            }
        }
    }
}
