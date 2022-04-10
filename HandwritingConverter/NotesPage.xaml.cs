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
    public sealed partial class NotesPage : Page
    {
        public ObservableCollection<Note> Notes = new ObservableCollection<Note>();
        public NotesPage()
        {
            InitializeComponent();
            GrabNotes();
        }

        private void GrabNotes()
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT guid,timestamp,converted FROM convertedText", db);
                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Notes.Add(new Note(query.GetGuid(0), query.GetInt32(1), query.GetString(2)));
                }

                db.Close();
            }
        }

        private async void DeleteNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                Notes.Remove(note);

                Guid guid = note.id;

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

        private async void CopyNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                string textToCopy = note.converted;
                DataPackage dataPackage = new DataPackage();

                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(textToCopy);

                Clipboard.SetContent(dataPackage);

                CopyButton.Icon = new SymbolIcon(Symbol.Accept);
                CopyButton.Label = "Copied!";
                await Task.Delay(1000);
                CopyButton.Icon = new SymbolIcon(Symbol.Copy);
                CopyButton.Label = "Copy";
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

        private async void ExportNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                string noteToSave = note.converted;

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

        private async void NarrateNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                string noteToNarrate = note.converted;

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

        private async void DetailNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;

                DateTimeOffset noteOffset = DateTimeOffset.FromUnixTimeSeconds(note.timestamp);
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

        private void SortNotesConverted(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Note> tempArray = new ObservableCollection<Note>(BubbleSortConverted(Notes));
            Notes.Clear();

            for (int i = 0; i < tempArray.Count; i++)
            {
                Notes.Add(tempArray[i]);
            }
        }
        
        private void SortNotesTimestamp(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Note> tempArray = new ObservableCollection<Note>(BubbleSortTimestamp(Notes));
            Notes.Clear();

            for (int i = 0; i < tempArray.Count; i++)
            {
                Notes.Add(tempArray[i]);
            }
        }

        private ObservableCollection<Note> BubbleSortConverted(ObservableCollection<Note> array)
        {
            int counter = 0;
            bool swapped = true;
            int swaps = 0;

            while (swapped)
            {
                while (counter < array.Count - 1)
                {
                    if (string.Compare(array[counter].converted, array[counter + 1].converted) > 0)
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

        private ObservableCollection<Note> BubbleSortTimestamp(ObservableCollection<Note> array)
        {
            int counter = 0;
            bool swapped = true;
            int swaps = 0;

            while (swapped)
            {
                while (counter < array.Count - 1)
                {
                    if (array[counter].timestamp > array[counter + 1].timestamp)
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
