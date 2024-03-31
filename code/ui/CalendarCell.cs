using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Planner
{
	public class CalendarCell : TableLayoutPanel
	{
		private DateTime date;
		
		private bool selected;
		
		private EventHandler onCellClick;
		private EventHandler onTimeslotLabelClick;
		
		private List<TimeslotLabel> timeslotList;
		private Label dateLabel;
		private Control timeslotMark;
		
		
		public CalendarCell(LocalisedContextMenuStrip contextMenu, Font font, EventHandler onCellClick, EventHandler onTimeslotLabelClick)
		{
			this.timeslotList = new List<TimeslotLabel>();
			this.selected = false;
			this.ContextMenuStrip = contextMenu;
			this.BackColor = Program.Config.GetBackColour();
			
			this.RowCount = 0;
			this.ColumnCount = 1;
			this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			this.Dock = DockStyle.Fill;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			
			this.MouseEnter += CalendarCellOnMouseEnter;
			this.MouseLeave += CalendarCellOnMouseLeave;
			
			
			dateLabel = new Label();
			dateLabel.Dock = DockStyle.Fill;
			dateLabel.MouseEnter += CalendarCellOnMouseEnter;
			dateLabel.MouseLeave += CalendarCellOnMouseLeave;
			dateLabel.Font = font;
			this.Controls.Add(dateLabel);
			RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
			
			timeslotMark = new Control();
			timeslotMark.BackColor = Program.Config.GetTimeslotBackColour();
			timeslotMark.Size = new Size(10, 10);
			timeslotMark.Location = new Point(0, 7);
			dateLabel.Controls.Add(timeslotMark);
			dateLabel.Padding = new Padding(10, 0, 0 , 0);
			timeslotMark.Hide();
			
			timeslotMark.MouseEnter += CalendarCellOnMouseEnter;
			timeslotMark.MouseLeave += CalendarCellOnMouseLeave;
			timeslotMark.MouseDown += TriggerOnClick;
			
			this.onCellClick = onCellClick;
			this.onTimeslotLabelClick = onTimeslotLabelClick;
			
			this.MouseDown += (object s, MouseEventArgs e) => this.onCellClick(s, new EventArgs());
			dateLabel.MouseDown += this.TriggerOnClick;
			
			UpdateData();
		}
		
		
		public void SetDate(DateTime date) { this.date = date.Date; UpdateData(); }
		public void SetDate(DateTimeOffset date) { this.date = date.Date; UpdateData(); }
		public DateTime GetDate() { return this.date; }
		
		public TimeslotLabel FindTimeslotLabel(long scheduleID, long timeslotID)
		{
			foreach(TimeslotLabel timeslotLabel in timeslotList)
				if(timeslotLabel.GetScheduleID() == scheduleID && timeslotLabel.GetTimeslotID() == timeslotID)
					return timeslotLabel;
			
			return null;
		}
		
		
		public void CellSelected()
		{
			selected = true;
			this.BackColor = Program.Config.GetSelectionColour();
		}
		
		public void CellDeselected()
		{
			selected = false;
			this.BackColor = Program.Config.GetBackColour();
		}
		
		public void AddTimeslots(List<Timeslot> timeslotList)
		{
			foreach(Timeslot timeslot in timeslotList)
				AddTimeslot(timeslot);
		}
		
		public void AddTimeslot(Timeslot timeslot)
		{
			TimeslotLabel label = new TimeslotLabel(timeslot);
			
			label.Click += TriggerOnClick;
			label.Click += onTimeslotLabelClick;
			label.MouseEnter += CalendarCellOnMouseEnter;
			label.MouseLeave += CalendarCellOnMouseLeave;
			
			timeslotList.Add(label);
			this.Controls.Add(label);
			
			UpdateData();
		}
		
		public void RemoveTimeslotLabel(TimeslotLabel timeslotLabel)
		{
			timeslotList.Remove(timeslotLabel);
			this.Controls.Remove(timeslotLabel);
			
			UpdateData();
		}
		
		public void ClearTimeslotLabels()
		{
			foreach(TimeslotLabel timeslotLabel in timeslotList)
				this.Controls.Remove(timeslotLabel);
			
			timeslotList.Clear();
			
			UpdateData();
		}
		
		private void UpdateData()
		{
			dateLabel.Text = $"{date.Day.ToString("00")}.{date.Month.ToString("00")}";
			
			if(timeslotList.Count > 0)
				timeslotMark.Show();
			else
				timeslotMark.Hide();
		}
		
		private void TriggerOnClick(object s, EventArgs e)
		{
			onCellClick(this, e);
		}
		
		private void CalendarCellOnMouseEnter(object s, EventArgs e)
		{
			if(selected)
				return;
			
			this.BackColor = Program.Config.GetHoverColour();
		}
		
		private void CalendarCellOnMouseLeave(object s, EventArgs e)
		{
			if(selected)
				return;
			
			this.BackColor = Program.Config.GetBackColour();
		}
	}
}
