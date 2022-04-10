using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingConverter
{
    public class Note
    {
        public Guid Id;
        public int Timestamp;
        public string Converted;

        public Note(Guid id, int timestamp, string converted)
        {
            Id = id;
            Timestamp = timestamp;
            Converted = converted;
        }
    }
}
