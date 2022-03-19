using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingConverter
{
    public class Note
    {
        private Guid Id { get; set; }
        private string Converted { get; set; }

        public Note(Guid key, string conv)
        {
            Id = key;
            Converted = conv;
        }
    }
}
