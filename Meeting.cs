using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarProject
{
    public class Meeting : Event
    {
        // properties
        public List<string> Attendees { get; set; }
        public string Organizer { get; set; }
        public string Location { get; set; }

        // Constructor
        public Meeting(string title, DateTime startTime, DateTime endTime,
                      string organizer, List<string> attendees,
                      string location = "", string description = "")
            : base(title, startTime, endTime, description)
        {
            Organizer = organizer;
            Attendees = attendees ?? new List<string>();
            Location = location;
        }
    }
}
