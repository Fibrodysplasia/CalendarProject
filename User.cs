using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarProject
{
    public class User
    {
        // Properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Username { get; protected set; }
        public string Password { get; protected set; }
        public bool IsManager { get; set; }
        public List<Event> Calendar { get; set; } // Add Calendar property

        // Constructor
        public User(string firstName, string lastName, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            // first letter first name + last name, i.e John Doe is jdoe
            Username = (FirstName.Substring(0, 1) + LastName).ToLower();
            Password = password;
            IsManager = false;
            FullName = firstName + " " + lastName;
            Calendar = new List<Event>();
        }

        // TODO: actual login logic
        public virtual void Login()
        {
            if (IsManager)
            {
                Console.WriteLine($"{Username} logged in as a manager.");
            }
            else
            {
                Console.WriteLine($"{Username} logged in as a standard user");
            }
        }

        // Should these go in Events?
        public void AddEvent(Event newEvent)
        {
            Calendar.Add(newEvent);
        }
        // Using LINQ instead of for loops
        public bool RemoveEvent(int eventId)
        {
            Event eventToRemove = Calendar.FirstOrDefault(e => e.Id == eventId);
            if (eventToRemove != null)
            {
                return Calendar.Remove(eventToRemove);
            }
            return false;
        }

        public Event GetEvent(int eventId)
        {
            return Calendar.FirstOrDefault(e => e.Id == eventId);
        }

        public List<Event> GetEventsByDate(DateTime date)
        {
            return Calendar.Where(e =>
                e.StartTime.Date <= date.Date && e.EndTime.Date >= date.Date
            ).ToList();
        }
    }
}