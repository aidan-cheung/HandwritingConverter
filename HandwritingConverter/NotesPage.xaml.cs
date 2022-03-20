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
                        // this.recordings.Add(new Recording(){ ArtistName = "Johann Sebastian Bach",
                        // CompositionName = "Mass in B minor", ReleaseDateTime = new DateTime(1748, 7, 8)
                        Notes.Add(new Note() { Id = query.GetGuid(0), Converted = query.GetString(1)});
                    }

                    db.Close();
                }

            }
        }

        public void deleteNote(object sender, RoutedEventArgs e)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = db;

                var tmpnote = (Note)e.OriginalSource;

                deleteCommand.CommandText = $"DELETE FROM convertedText WHERE guid={tmpnote.Id}";
                deleteCommand.ExecuteReader();

                db.Close();
            }
        }

        /* public static IReadOnlyList<Note> getNotes()
        {
            Notes.Clear();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT guid,converted FROM convertedText", db);
                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Notes.Add(new Note(query.GetGuid(0), query.GetString(1)));
                }

                db.Close();
            }

            return Notes;
        } */
    }
}
