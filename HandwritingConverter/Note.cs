using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingConverter
{
    public class Note
    {
        public Guid id;
        public int timestamp;
        public string converted;

        public Note(Guid identifier, int unixtime, string text)
        {
            id = identifier;
            timestamp = unixtime;
            converted = text;
        }
    }
}
