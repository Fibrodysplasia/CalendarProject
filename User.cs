using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
        public List<Event> Calendar { get; set; }

        // list of users for authentication
        private static List<User> users = new List<User>();

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

        // methods
        public static void AddUser(User user)
        {
            users.Add(user);
        }

        public static List<User> GetAllUsers()
        {
            return users;
        }

        public static User Login(string username, string password)
        {
            // Find user by username and check password
            return users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        public void Logout()
        {
            // clear controls etc
            Console.WriteLine($"User {Username} logged out");
        }

        public bool AddEvent(Event newEvent)
        {
            if (string.IsNullOrEmpty(newEvent.Owner))
            {
                newEvent.Owner = this.Username;
            }

            // Check conflicts
            if (HasEventConflict(newEvent))
            {
                return false;
            }

            Calendar.Add(newEvent);
            return true;
        }

        public bool HasEventConflict(Event newEvent)
        {
            foreach (var existingEvent in Calendar)
            {
                if (existingEvent.Id == newEvent.Id)
                    continue;

                // check for overlap
                if (newEvent.StartTime < existingEvent.EndTime &&
                    newEvent.EndTime > existingEvent.StartTime)
                {
                    return true;
                }
            }
            return false;
        }

        public bool DeleteEvent(Event eventToRemove)
        {
            return Calendar.Remove(eventToRemove);
        }

        public List<Event> GetEventsByDate(DateTime date)
        {
            return Calendar.Where(e =>
                e.StartTime.Date <= date.Date && e.EndTime.Date >= date.Date
            ).ToList();
        }

        public List<DateTime> FindAvailableMeetingSlots(DateTime date, TimeSpan duration, List<User> attendees)
        {
            if (!IsManager)
            {
                return new List<DateTime>();
            }

            List<DateTime> availableSlots = new List<DateTime>();

            // 9-5 working hours
            DateTime dayStart = date.Date.AddHours(9);
            DateTime dayEnd = date.Date.AddHours(17);

            // every hour
            for (DateTime slot = dayStart; slot.Add(duration) <= dayEnd; slot = slot.AddMinutes(60))
            {
                DateTime slotEnd = slot.Add(duration);
                bool isAvailable = true;

                foreach (var evt in Calendar)
                {
                    if (slot < evt.EndTime && slotEnd > evt.StartTime)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (!isAvailable)
                    continue;

                foreach (var attendee in attendees)
                {
                    foreach (var evt in attendee.Calendar)
                    {
                        if (slot < evt.EndTime && slotEnd > evt.StartTime)
                        {
                            isAvailable = false;
                            break;
                        }
                    }

                    if (!isAvailable)
                        break;
                }

                if (isAvailable)
                {
                    availableSlots.Add(slot);
                }
            }

            return availableSlots;
        }
    }
}