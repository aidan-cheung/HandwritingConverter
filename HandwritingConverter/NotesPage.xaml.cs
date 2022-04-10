using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;

namespace HandwritingConverter
{
    /*
    The following code uses modified samples from the Microsoft UWP Documentation, 
    these samples are taken from the following pages granted under the MIT license:

    - https://docs.microsoft.com/en-us/windows/apps/design/controls/navigationview
    */
    public sealed partial class NotesPage : Page
    {
        public NoteViewModel ViewModel { get; set; }
        public NotesPage()
        {
            this.InitializeComponent();
            this.ViewModel = new NoteViewModel();
        }

        public class NoteViewModel
        {
            private ObservableCollection<Note> notes = new ObservableCollection<Note>();
            public ObservableCollection<Note> Notes
            {
                get { return notes; }
                set { notes = value; }
            }

            public NoteViewModel()
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
                using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand selectCommand = new SqliteCommand("SELECT guid,timestamp,converted FROM convertedText", db);
                    SqliteDataReader query = selectCommand.ExecuteReader();

                    while (query.Read())
                    {
                        Notes.Add(new Note() { Id = query.GetGuid(0), Timestamp = query.GetInt32(1), Converted = query.GetString(2) });
                    }

                    db.Close();
                }
            }
        }

        private async void deleteNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                ViewModel.Notes.Remove(note);

                Guid guid = note.Id;

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
                using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand deleteCommand = new SqliteCommand();
                    deleteCommand.Connection = db;

                    deleteCommand.CommandText = $"DELETE FROM convertedText WHERE guid=@guid";
                    deleteCommand.Parameters.AddWithValue("@guid", guid);

                    deleteCommand.ExecuteReader();

                    db.Close();
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "Okay";
                dialog.Content = "No note was selected.";

                await dialog.ShowAsync();
            }
        }

        private async void copyNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                string textToCopy = note.Converted;
                DataPackage dataPackage = new DataPackage();

                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(textToCopy);

                Clipboard.SetContent(dataPackage);

                copyButton.Icon = new SymbolIcon(Symbol.Accept);
                copyButton.Label = "Copied!";
                await Task.Delay(1000);
                copyButton.Icon = new SymbolIcon(Symbol.Copy);
                copyButton.Label = "Copy";
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "Okay";
                dialog.Content = "No note was selected.";

                await dialog.ShowAsync();
            }
        }

        private async void exportNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                string noteToSave = note.Converted;

                var filePicker = new Windows.Storage.Pickers.FileSavePicker();
                filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                filePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                filePicker.SuggestedFileName = "New Note";

                StorageFile file = await filePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);

                    await FileIO.WriteTextAsync(file, noteToSave);

                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "Okay";
                dialog.Content = "No note was selected.";

                await dialog.ShowAsync();
            }
        }

        private async void narrateNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                string noteToNarrate = note.Converted;

                MediaElement mediaElement = new MediaElement();
                var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
                Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(noteToNarrate);
                mediaElement.SetSource(stream, stream.ContentType);
                mediaElement.Play();
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "Okay";
                dialog.Content = "No note was selected.";

                await dialog.ShowAsync();
            }
        }

        private async void detailNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;

                DateTimeOffset noteOffset = DateTimeOffset.FromUnixTimeSeconds(note.Timestamp);
                DateTime noteDate = noteOffset.UtcDateTime;

                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Note Details";
                dialog.PrimaryButtonText = "Okay";
                dialog.Content = noteDate;

                await dialog.ShowAsync();
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.PrimaryButtonText = "Okay";
                dialog.Content = "No note was selected.";

                await dialog.ShowAsync();
            }
        }

        private void sortNotesConverted(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Note> tempArray = new ObservableCollection<Note>(bubbleSortConverted(ViewModel.Notes));
            ViewModel.Notes.Clear();

            for (int i = 0; i < tempArray.Count; i++)
            {
                ViewModel.Notes.Add(tempArray[i]);
            }
        }
        
        private void sortNotesTimestamp(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Note> tempArray = new ObservableCollection<Note>(bubbleSortTimestamp(ViewModel.Notes));
            ViewModel.Notes.Clear();

            for (int i = 0; i < tempArray.Count; i++)
            {
                ViewModel.Notes.Add(tempArray[i]);
            }
        }

        private static ObservableCollection<Note> bubbleSortConverted(ObservableCollection<Note> array)
        {
            int counter = 0;
            bool swapped = true;
            int swaps = 0;

            while (swapped)
            {
                while (counter < array.Count - 1)
                {
                    if (string.Compare(array[counter].Converted, array[counter + 1].Converted) > 0)
                    {
                        Note temp = array[counter];
                        array[counter] = array[counter + 1];
                        array[counter + 1] = temp;

                        swaps++;
                    }
                    counter++;
                }
                if (swaps == 0)
                {
                    swapped = false;
                }
                else
                {
                    swaps = 0;
                    counter = 0;
                }
            }
            return array;
        }

        private static ObservableCollection<Note> bubbleSortTimestamp(ObservableCollection<Note> array)
        {
            int counter = 0;
            bool swapped = true;
            int swaps = 0;

            while (swapped)
            {
                while (counter < array.Count - 1)
                {
                    if (array[counter].Timestamp > array[counter + 1].Timestamp)
                    {
                        Note temp = array[counter];
                        array[counter] = array[counter + 1];
                        array[counter + 1] = temp;

                        swaps++;
                    }
                    counter++;
                }
                if (swaps == 0)
                {
                    swapped = false;
                }
                else
                {
                    swaps = 0;
                    counter = 0;
                }
            }
            return array;
        }
    }
}
