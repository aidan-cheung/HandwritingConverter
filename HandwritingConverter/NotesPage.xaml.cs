using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            string query = "SELECT guid,timestamp,converted FROM convertedText";

            SqliteConnection connection = new SqliteConnection($"Filename={dbpath}");
            connection.Open();

            SqliteCommand command = new SqliteCommand(query, connection);
            SqliteDataReader response = command.ExecuteReader();

            while (response.Read())
            {
                Notes.Add(new Note(response.GetGuid(0), response.GetInt32(1), response.GetString(2)));
            }

            connection.Close();
        }

        private async void CopyNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                string textToCopy = note.converted;

                DataPackage dataPackage = new DataPackage();
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

        private async void DeleteNote(object sender, RoutedEventArgs e)
        {
            if (NotesGridView.SelectedItem != null)
            {
                Note note = NotesGridView.SelectedItem as Note;
                Notes.Remove(note);

                Guid guid = note.id;

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
                string query = "DELETE FROM convertedText WHERE guid=@guid";

                SqliteConnection connection = new SqliteConnection($"Filename={dbpath}");
                connection.Open();

                SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@guid", guid);

                command.ExecuteReader();
                connection.Close();
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
                filePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                filePicker.SuggestedFileName = "New Note";

                StorageFile file = await filePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, noteToSave);
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

        private void SortNotesConverted(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Note> TempArray = new ObservableCollection<Note>(BubbleSortConverted(Notes));

            Notes.Clear();

            for (int i = 0; i < TempArray.Count; i++)
            {
                Notes.Add(TempArray[i]);
            }
        }

        private void SortNotesTimestamp(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Note> TempArray = new ObservableCollection<Note>(BubbleSortTimestamp(Notes));

            Notes.Clear();

            for (int i = 0; i < TempArray.Count; i++)
            {
                Notes.Add(TempArray[i]);
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
