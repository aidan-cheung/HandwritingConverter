using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            public ObservableCollection<Note> Notes { get { return notes; } }

            public NoteViewModel()
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
                using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand selectCommand = new SqliteCommand("SELECT guid,converted FROM convertedText", db);
                    SqliteDataReader query = selectCommand.ExecuteReader();

                    while (query.Read())
                    {
                        Notes.Add(new Note() { Id = query.GetGuid(0), Converted = query.GetString(1) });
                    }

                    db.Close();
                }

            }
        }

        private void deleteNote(object sender, RoutedEventArgs e)
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

                deleteCommand.CommandText = $"DELETE FROM convertedText WHERE guid='{guid}'";
                deleteCommand.ExecuteReader();

                db.Close();
            }
        }

        private static string binarySearch(string[] array, string searchTerm)
        {
            int lower = 0;
            int upper = array.Length;
            int mid = 0;
            bool found = false;

            while (lower <= upper)
            {
                mid = (lower + upper) / 2;

                if (array[mid] == searchTerm)
                {
                    found = true;
                }
                else if (string.Compare(array[mid], searchTerm) > 0)
                {
                    upper = mid - 1;
                }
                else
                {
                    lower = mid + 1;
                }
            }

            if (found)
            {
                return array[mid];
            }
            else
            {
                return "No result";
            }
        }
    }
}
