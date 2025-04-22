using System;
using System.Drawing;
using System.Windows.Forms;

namespace CalendarProject
{
    public partial class CalendarView : Form
    {
        // Calendar variables
        private Panel[,] dayCells;
        private int currentMonth = DateTime.Now.Month;
        private int currentYear = DateTime.Now.Year;

        public CalendarView()
        {
            InitializeComponent();
        }

        // Generate calendar when the form loads
        private void CalendarView_Load(object sender, EventArgs e)
        {
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
                    // create a panel for each day
                    Panel dayCell = new Panel();
                    dayCell.BorderStyle = BorderStyle.FixedSingle;
                    dayCell.BackColor = Color.White;
                    dayCell.Dock = DockStyle.Fill;
                    dayCell.Margin = new Padding(1);

                    // create a label for day number
                    Label dayLabel = new Label();
                    dayLabel.Font = new Font("Arial", 10F, FontStyle.Bold);
                    dayLabel.ForeColor = Color.Black;
                    dayLabel.Location = new Point(5, 5);
                    dayLabel.AutoSize = true;
                    dayLabel.Name = "dayLabel";
                    dayCell.Controls.Add(dayLabel);

                    // create event area
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

                    // highlight weekends
                    if (col == 0 || col == 6)
                    {
                        dayCells[row, col].BackColor = Color.Azure;
                    }

                    dayCounter++;
                }
            }
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
                    DateTime selectedDate = new DateTime(currentYear, currentMonth, day);

                    // PLACEHOLDER FOR EVENT OPTIONS (ADD, EDIT, DELETE)
                    MessageBox.Show($"Selected date: {selectedDate.ToShortDateString()}");

                    // highlight selected cell (ugly rn)
                    HighlightSelectedDay(clickedCell);
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
    }
}