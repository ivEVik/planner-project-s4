using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Planner
{
	public class TimeslotLabel : Label
	{
		private long scheduleID;
		private long timeslotID;
		
		private bool selected;
		
		
		public TimeslotLabel(Timeslot timeslot)
		{
			selected = false;
			
			this.timeslotID = timeslot.GetID();
			this.scheduleID = timeslot.GetScheduleID();
			this.Size = new Size(10, 20);
			this.BackColor = Program.Config.GetTimeslotBackColour();
			this.Dock = DockStyle.Top;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			
			this.MouseEnter += TimeslotLabelOnMouseEnter;
			this.MouseLeave += TimeslotLabelOnMouseLeave;
			
			RefreshData();
		}
		
		
		public long GetScheduleID() { return scheduleID; }
		public long GetTimeslotID() { return timeslotID; }
		
		
		public void RefreshData()
		{
			this.Text = Program.GetTimeslot(scheduleID, timeslotID).GetName();
		}
		
		public void TimeslotLabelSelected()
		{
			selected = true;
			this.BackColor = Program.Config.GetTimeslotSelectionColour();
		}
		
		public void TimeslotLabelDeselected()
		{
			selected = false;
			this.BackColor = Program.Config.GetTimeslotBackColour();
		}
		
		private void TimeslotLabelOnMouseEnter(object s, EventArgs e)
		{
			if(selected)
				return;
			
			this.BackColor = Program.Config.GetTimeslotHoverColour();
		}
		
		private void TimeslotLabelOnMouseLeave(object s, EventArgs e)
		{
			if(selected)
				return;
			
			this.BackColor = Program.Config.GetTimeslotBackColour();
		}
	}
}
