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
        public static ObservableCollection<Note> Notes { get; } = new ObservableCollection<Note>();
        public NotesPage()
        {
            this.InitializeComponent();
            Output.ItemsSource = getNotes();
        }

        public void deleteNote(object sender, RoutedEventArgs e)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = db;

                Guid guid = new Guid();

                deleteCommand.CommandText = $"DELETE FROM convertedText WHERE guid={guid}";
                deleteCommand.ExecuteReader();

                db.Close();
            }
        }

        public static IReadOnlyList<Note> getNotes()
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
        }
    }
}
