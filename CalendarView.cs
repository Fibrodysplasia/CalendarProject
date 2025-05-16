using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CalendarProject
{
    public partial class CalendarView : Form
    {
        // Calendar variables
        private User currentUser;
        private Panel[,] dayCells;
        private int currentMonth = DateTime.Now.Month;
        private int currentYear = DateTime.Now.Year;
        private Panel selectedDayCell = null;
        private DateTime selectedDate;

        public CalendarView(User user)
        {
            currentUser = user;
            InitializeComponent();
            // manager check
            createMeetingButton.Visible = (user != null && user.IsManager);
        }

        // generate calendar when the form loads
        private void CalendarView_Load(object sender, EventArgs e)
        {
            // title
            this.Text = $"Calendar - {currentUser.FullName}";

            // show/hide manager controls
            if (currentUser.IsManager)
            {
                createMeetingButton.Visible = true;
            }
            else
            {
                createMeetingButton.Visible = false;
            }

            InitializeCalendar();
            GenerateCalendar(currentYear, currentMonth);
        }

        private void InitializeCalendar()
        {
            // 6 weeks x 7 days
            dayCells = new Panel[6, 7];

            // add day cells to TableLayoutPanel
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    // panel for each day
                    Panel dayCell = new Panel();
                    dayCell.BorderStyle = BorderStyle.FixedSingle;
                    dayCell.BackColor = Color.White;
                    dayCell.Dock = DockStyle.Fill;
                    dayCell.Margin = new Padding(1);
                    dayCell.Tag = new Point(row, col); // for later reference

                    // day number
                    Label dayLabel = new Label();
                    dayLabel.Font = new Font("Arial", 10F, FontStyle.Bold);
                    dayLabel.ForeColor = Color.Black;
                    dayLabel.Location = new Point(5, 5);
                    dayLabel.AutoSize = true;
                    dayLabel.Name = "dayLabel";
                    dayCell.Controls.Add(dayLabel);

                    // event area
                    Panel eventArea = new Panel();
                    eventArea.Location = new Point(5, 25);
                    eventArea.Size = new Size(dayCell.Width - 10, dayCell.Height - 30);
                    eventArea.AutoScroll = true;
                    eventArea.Name = "eventArea";
                    dayCell.Controls.Add(eventArea);

                    // event handler for clicking days / labels
                    dayCell.Click += DayCell_Click;
                    dayLabel.Click += DayCell_Click;

                    // store dayCell in array
                    dayCells[row, col] = dayCell;

                    // adds starting from row 2 (after headers)
                    tableLayoutPanel1.Controls.Add(dayCell, col, row + 2);
                }
            }
        }

        // generate current month's calendar
        private void GenerateCalendar(int year, int month)
        {
            // update header / get info
            DateTime displayDate = new DateTime(year, month, 1);
            label8.Text = displayDate.ToString("MMMM yyyy");
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // clear all cells
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    Panel dayCell = dayCells[row, col];
                    Label dayLabel = (Label)dayCell.Controls["dayLabel"];

                    // clear label and events
                    dayLabel.Text = "";
                    dayCell.BackColor = Color.White;
                    Panel eventArea = (Panel)dayCell.Controls["eventArea"];
                    eventArea.Controls.Clear();
                }
            }

            // label days
            int dayCounter = 1;

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    // skip to first day of the month
                    if (row == 0 && col < startDayOfWeek)
                    {
                        // grey out days from prior month (beginning of calendar)
                        dayCells[row, col].BackColor = Color.WhiteSmoke;
                        continue;
                    }

                    // grey out days from next month (end of calendar)
                    if (dayCounter > daysInMonth)
                    {
                        dayCells[row, col].BackColor = Color.WhiteSmoke;
                        continue;
                    }

                    // set numbers
                    Label dayLabel = (Label)dayCells[row, col].Controls["dayLabel"];
                    dayLabel.Text = dayCounter.ToString();

                    // date for cell
                    DateTime cellDate = new DateTime(year, month, dayCounter);
                    dayCells[row, col].Tag = cellDate;

                    // highlight weekends
                    if (col == 0 || col == 6)
                    {
                        dayCells[row, col].BackColor = Color.Azure;
                    }

                    // highlight today
                    if (cellDate.Date == DateTime.Today)
                    {
                        dayCells[row, col].BackColor = Color.LightGreen;
                    }

                    // show events for this day
                    DisplayEventsForDay(dayCells[row, col], cellDate);

                    dayCounter++;
                }
            }
        }

        // display events for a specific day
        private void DisplayEventsForDay(Panel dayCell, DateTime date)
        {
            if (currentUser == null || currentUser.Calendar == null || dayCell == null)
            {
                return;
            }

            // get eventArea
            Panel eventArea = null;
            foreach (Control control in dayCell.Controls)
            {
                if (control is Panel && control.Name == "eventArea")
                {
                    eventArea = (Panel)control;
                    break;
                }
            }
            if (eventArea == null)
            {
                // create eventArea if it doesn't exist
                eventArea = new Panel();
                eventArea.Location = new Point(5, 25);
                eventArea.Size = new Size(dayCell.Width - 10, dayCell.Height - 30);
                eventArea.AutoScroll = true;
                eventArea.Name = "eventArea";
                dayCell.Controls.Add(eventArea);
            }

            // update and display
            eventArea.Controls.Clear();

            List<Event> dayEvents = GetEventsForDay(date);

            int yPosition = 0;
            foreach (Event evt in dayEvents)
            {
                Label eventLabel = new Label();
                eventLabel.AutoSize = false;
                eventLabel.Width = eventArea.Width - 5;
                eventLabel.Height = 20;
                eventLabel.Location = new Point(0, yPosition);
                eventLabel.Text = evt.Title;
                eventLabel.Font = new Font("Arial", 8F);
                eventLabel.BackColor = (evt is Meeting) ? Color.LightSalmon : Color.LightBlue;
                eventLabel.Tag = evt;
                eventLabel.Click += EventLabel_Click;

                // more details
                ToolTip tooltip = new ToolTip();
                tooltip.SetToolTip(eventLabel, $"{evt.Title}\n{evt.StartTime.ToShortTimeString()} - {evt.EndTime.ToShortTimeString()}\n{evt.Description}");

                eventArea.Controls.Add(eventLabel);
                yPosition += 22;
            }
        }

        // Get events for specific day
        private List<Event> GetEventsForDay(DateTime date)
        {
            if (currentUser == null || currentUser.Calendar == null)
                return new List<Event>();

            List<Event> dayEvents = new List<Event>();

            foreach (Event evt in currentUser.Calendar)
            {
                if (evt.StartTime.Date <= date.Date && evt.EndTime.Date >= date.Date)
                {
                    dayEvents.Add(evt);
                }
            }

            return dayEvents;
        }

        // handleevent label click
        private void EventLabel_Click(object sender, EventArgs e)
        {
            Label eventLabel = sender as Label;
            if (eventLabel != null && eventLabel.Tag is Event)
            {
                Event selectedEvent = (Event)eventLabel.Tag;
                ShowEventDetails(selectedEvent);
            }
        }

        // event details
        private void ShowEventDetails(Event selectedEvent)
        {
            string eventType = selectedEvent is Meeting ? "Meeting" : "Event";
            string details = $"{eventType}: {selectedEvent.Title}\n" +
                            $"Date: {selectedEvent.StartTime.ToShortDateString()}\n" +
                            $"Time: {selectedEvent.StartTime.ToShortTimeString()} - {selectedEvent.EndTime.ToShortTimeString()}\n" +
                            $"Description: {selectedEvent.Description}";

            if (selectedEvent is Meeting meeting)
            {
                details += $"\nOrganizer: {meeting.Organizer}\n" +
                           $"Location: {meeting.Location}\n" +
                           $"Attendees: {string.Join(", ", meeting.Attendees)}";
            }

            MessageBox.Show(details, "Event Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // handle day cell click event
        private void DayCell_Click(object sender, EventArgs e)
        {
            Panel clickedCell = sender as Panel;
            if (clickedCell == null && sender is Label label)
            {
                clickedCell = label.Parent as Panel;
            }

            if (clickedCell != null)
            {
                // find the date
                Label dayLabel = clickedCell.Controls["dayLabel"] as Label;
                if (dayLabel != null && !string.IsNullOrEmpty(dayLabel.Text))
                {
                    int day = int.Parse(dayLabel.Text);
                    selectedDate = new DateTime(currentYear, currentMonth, day);
                    selectedDayCell = clickedCell;

                    // Show day menu
                    ShowDayMenu();

                    // highlight selected cell
                    HighlightSelectedDay(clickedCell);
                }
            }
        }

        // Show menu
        private void ShowDayMenu()
        {
            ContextMenuStrip dayMenu = new ContextMenuStrip();
            ToolStripMenuItem addEventItem = new ToolStripMenuItem("Add Event");
            addEventItem.Click += (s, e) => AddEvent_Click();
            dayMenu.Items.Add(addEventItem);

            List<Event> dayEvents = GetEventsForDay(selectedDate);
            if (dayEvents.Count > 0)
            {
                ToolStripMenuItem viewEventsItem = new ToolStripMenuItem("View Events");
                foreach (Event evt in dayEvents)
                {
                    ToolStripMenuItem eventItem = new ToolStripMenuItem(evt.Title);
                    eventItem.Tag = evt;
                    eventItem.Click += (s, e) => ShowEventDetails((Event)((ToolStripMenuItem)s).Tag);
                    viewEventsItem.DropDownItems.Add(eventItem);
                }
                dayMenu.Items.Add(viewEventsItem);
            }

            if (dayMenu.Items.Count > 0)
            {
                dayMenu.Show(this, this.PointToClient(Cursor.Position));
            }
        }

        //event handler
        private void AddEvent_Click()
        {
            string title = "New Event";
            string description = "Event Description";

            using (Form inputForm = new Form())
            {
                inputForm.Width = 300;
                inputForm.Height = 400;
                inputForm.Text = "New Event";
                
                Label lblTitle = new Label() 
                { 
                    Left = 20, 
                    Top = 20, 
                    Text = "Event Title:" 
                };

                TextBox txtTitle = new TextBox() 
                { 
                    Left = 20, 
                    Top = 40, 
                    Width = 240 
                };

                Label lblDescription = new Label() 
                { 
                    Left = 20, 
                    Top = 70, 
                    Text = "Description:" 
                };

                TextBox txtDescription = new TextBox() 
                { 
                    Left = 20, 
                    Top = 90, 
                    Width = 240,                     
                    Height = 60,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical 
                };

                Label lblDate = new Label() 
                { 
                    Left = 20, 
                    Top = 160, 
                    Text = "Date:" 
                };

                DateTimePicker dtpDate = new DateTimePicker()
                {
                    Left = 20,
                    Top = 180,
                    Width = 240,
                    Format = DateTimePickerFormat.Short
                };

                // set default
                if (selectedDayCell != null && selectedDayCell.Tag is DateTime)
                {
                    dtpDate.Value = (DateTime)selectedDayCell.Tag;
                }

                Label lblStartTime = new Label() 
                { 
                    Left = 20, 
                    Top = 210, 
                    Text = "Start Time:" 
                };

                DateTimePicker dtpStartTime = new DateTimePicker() 
                { 
                    Left = 20, 
                    Top = 230, 
                    Width = 240, 
                    Format = DateTimePickerFormat.Time, 
                    ShowUpDown = true 
                };
                dtpStartTime.Value = DateTime.Today.AddHours(9); //default 9am start

                Label lblEndTime = new Label() 
                { 
                    Left = 20, 
                    Top = 260, 
                    Text = "End Time:" 
                };
                DateTimePicker dtpEndTime = new DateTimePicker() 
                { 
                    Left = 20, 
                    Top = 280, 
                    Width = 240, 
                    Format = DateTimePickerFormat.Time, 
                    ShowUpDown = true 
                };
                dtpEndTime.Value = DateTime.Today.AddHours(10); //default 10am end

                Button btnCreate = new Button() 
                { 
                    Text = "Create", 
                    Left = 20, 
                    Top = 320, 
                    Width = 100 
                };
                Button btnCancel = new Button() 
                { 
                    Text = "Cancel", 
                    Left = 160, 
                    Top = 320, 
                    Width = 100 
                };
                btnCreate.Click += (s, args) => {
                    // validation!
                    if (string.IsNullOrWhiteSpace(txtTitle.Text))
                    {
                        MessageBox.Show("Event title is required", "Missing Title",
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dtpEndTime.Value.TimeOfDay <= dtpStartTime.Value.TimeOfDay)
                    {
                        MessageBox.Show("End time must be after start time", "Invalid Times",
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //create event
                    title = txtTitle.Text;
                    description = txtDescription.Text;
                    inputForm.Tag = new Tuple<DateTime, DateTime, DateTime>(
                        dtpDate.Value.Date,
                        dtpStartTime.Value,
                        dtpEndTime.Value
                    );

                    inputForm.DialogResult = DialogResult.OK;
                };
                btnCancel.Click += (s, args) => { inputForm.DialogResult = DialogResult.Cancel; };

                // add controls
                inputForm.Controls.Add(lblTitle);
                inputForm.Controls.Add(txtTitle);
                inputForm.Controls.Add(lblDescription);
                inputForm.Controls.Add(txtDescription);
                inputForm.Controls.Add(lblDate);
                inputForm.Controls.Add(dtpDate);
                inputForm.Controls.Add(lblStartTime);
                inputForm.Controls.Add(dtpStartTime);
                inputForm.Controls.Add(lblEndTime);
                inputForm.Controls.Add(dtpEndTime);
                inputForm.Controls.Add(btnCreate);
                inputForm.Controls.Add(btnCancel);

                inputForm.AcceptButton = btnCreate;
                inputForm.CancelButton = btnCancel;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    // get event details and make the event
                    var eventData = (Tuple<DateTime, DateTime, DateTime>)inputForm.Tag;
                    DateTime eventDate = eventData.Item1;
                    DateTime startTime = eventData.Item2;
                    DateTime endTime = eventData.Item3;

                    DateTime startDateTime = eventDate.Date.Add(startTime.TimeOfDay);
                    DateTime endDateTime = eventDate.Date.Add(endTime.TimeOfDay);

                    Event newEvent = new Event(
                        title,
                        startDateTime,
                        endDateTime,
                        description
                    );
                    newEvent.Owner = currentUser.Username;

                    currentUser.Calendar.Add(newEvent);

                    // trying to refresh the cell of the date of the event
                    // was causing a bunch of out-of-reference/null object errors
                    // refreshing the whol calendar seems to work fine
                    GenerateCalendar(currentYear, currentMonth);
                }
            }
        }

        // selected cell highlight
        private void HighlightSelectedDay(Panel selectedCell)
        {
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    dayCells[row, col].BorderStyle = BorderStyle.FixedSingle;
                }
            }
            selectedCell.BorderStyle = BorderStyle.Fixed3D;
        }

        // navigate between months
        public void PreviousMonth()
        {
            currentMonth--;
            if (currentMonth < 1)
            {
                currentMonth = 12;
                currentYear--;
            }
            GenerateCalendar(currentYear, currentMonth);
        }

        public void NextMonth()
        {
            currentMonth++;
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
            }
            GenerateCalendar(currentYear, currentMonth);
        }

        // go to specific month/year
        public void GoToDate(int year, int month)
        {
            currentYear = year;
            currentMonth = month;
            GenerateCalendar(currentYear, currentMonth);
        }

        // handlers for month navigation
        private void nextMonthButton_Click(object sender, EventArgs e)
        {
            NextMonth();
        }

        private void previousMonthButton_Click(object sender, EventArgs e)
        {
            PreviousMonth();
        }

        // Create meeting button click handler
        private void createMeetingButton_Click(object sender, EventArgs e)
        {
            if (!currentUser.IsManager)
            {
                MessageBox.Show("Only managers can create meetings.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // TODO: Fix generic form
            using (Form meetingForm = new Form())
            {
                meetingForm.Width = 300;
                meetingForm.Height = 200;
                meetingForm.Text = "Create Meeting";

                Label lblTitle = new Label() { Left = 20, Top = 20, Text = "Meeting Title:" };
                TextBox txtTitle = new TextBox() { Left = 20, Top = 40, Width = 240 };

                Label lblAttendees = new Label() { Left = 20, Top = 70, Text = "Attendees (comma separated):" };
                TextBox txtAttendees = new TextBox() { Left = 20, Top = 90, Width = 240 };

                Button btnCreate = new Button() { Text = "Create", Left = 20, Top = 130, Width = 100 };
                Button btnCancel = new Button() { Text = "Cancel", Left = 160, Top = 130, Width = 100 };

                btnCreate.Click += (s, e) => { meetingForm.DialogResult = DialogResult.OK; };
                btnCancel.Click += (s, e) => { meetingForm.DialogResult = DialogResult.Cancel; };

                meetingForm.Controls.Add(lblTitle);
                meetingForm.Controls.Add(txtTitle);
                meetingForm.Controls.Add(lblAttendees);
                meetingForm.Controls.Add(txtAttendees);
                meetingForm.Controls.Add(btnCreate);
                meetingForm.Controls.Add(btnCancel);

                meetingForm.AcceptButton = btnCreate;
                meetingForm.CancelButton = btnCancel;

                if (meetingForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    // Get attendee usernames
                    List<string> attendees = new List<string>();
                    string[] attendeeNames = txtAttendees.Text.Split(',');
                    foreach (string name in attendeeNames)
                    {
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            attendees.Add(name.Trim().ToLower());
                        }
                    }

                    // create new meeting
                    Meeting newMeeting = new Meeting(
                        txtTitle.Text,
                        DateTime.Now.Date.AddDays(1).AddHours(10), // Tomorrow at 10 AM
                        DateTime.Now.Date.AddDays(1).AddHours(11), // Tomorrow at 11 AM
                        currentUser.Username,
                        attendees,
                        "Conference Room A",
                        "Meeting organized by " + currentUser.FullName
                    );

                    // add to calendar and refresh
                    currentUser.Calendar.Add(newMeeting);
                    GenerateCalendar(currentYear, currentMonth);

                    MessageBox.Show("Meeting created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}