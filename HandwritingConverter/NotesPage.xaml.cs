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
            Debug.WriteLine(GetData());
        }

        class note
        {
            public Guid guid;
            public Image image;
            public string converted;
        }

        public static List<String> GetData()
        {
            List<String> entries = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "handwritingConverter.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * FROM convertedText", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return entries;
        }
    }
}
