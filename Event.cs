using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarProject
{
    public class Event
    {
        // Properties
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Owner { get; set; }
        public TimeSpan Duration
        {
            get { return EndTime - StartTime; }
        }

        // constructor
        public Event(string title, DateTime startTime, DateTime endTime, string description = "")
        {
            // unique ID
            Id = DateTime.Now.GetHashCode() & 0x7FFFFFFF;
            Title = title;
            Description = description;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
