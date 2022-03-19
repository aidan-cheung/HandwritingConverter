using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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
        public NotesPage()
        {
            this.InitializeComponent();
            Output.ItemsSource = note.getNotes();
        }

        // Use the note class to store notes
        public class note
        {
            public Guid guid;
            public Image image;
            public string converted;

            public note(Guid key, Image img, string conv)
            {
                guid = key;
                image = img;
                converted = conv;
            }

            public static List<String> getNotes()
            {
                List<Guid> guid = new List<Guid>();
                List<String> converted = new List<string>();

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
                using (SqliteConnection db =
                   new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand selectCommand = new SqliteCommand("SELECT guid,converted FROM convertedText", db);

                    SqliteDataReader query = selectCommand.ExecuteReader();

                    while (query.Read())
                    {
                        Guid temp_guid = query.GetGuid(0);
                        String temp_converted = query.GetString(1);

                        converted.Add(temp_converted);
                        guid.Add(temp_guid);
                    }

                    db.Close();
                }

                return converted;
            }

            private void deleteNote(object sender, RoutedEventArgs e)
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
        }
    }
}
