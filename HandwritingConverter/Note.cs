using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingConverter
{
    public class Note
    {
        public Guid Id { get; set; }
        public int Timestamp { get; set; }
        public string Converted { get; set; }
    }
}
