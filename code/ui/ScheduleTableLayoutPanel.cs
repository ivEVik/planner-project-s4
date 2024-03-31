using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Planner
{
	public class ScheduleTableLayoutPanel : TableLayoutPanel, ILocalised
	{
		public event EventHandler TimeslotRefreshRequired;
		
		
		private LocalisedContextMenuStrip contextMenu;
		private List<ILocalised> localisedItems;
		private ScheduleControl selectedScheduleControl;
		
		
		public ScheduleTableLayoutPanel()
		{
			localisedItems = new List<ILocalised>();
			
			InitContextMenu();
			RefreshSchedules();
			
			this.BackColor = Program.Config.GetBackColour();
			this.ColumnCount = 1;
			this.Dock = DockStyle.Fill;
			this.AutoScroll = true;
			this.AutoScrollMargin = new Size(0, 30);
			this.Padding = new Padding(0, 0, 5, 0);
		}
		
		
		public void ReloadStringResources()
		{
			contextMenu.ReloadStringResources();
			
			foreach(ILocalised item in localisedItems)
				item.ReloadStringResources();
		}
		
		public void AddItem(ScheduleControl scheduleControl)
		{
			localisedItems.Add(scheduleControl);
			this.Controls.Add(scheduleControl);
			scheduleControl.TimeslotRefreshRequired += (object s, EventArgs e) => TimeslotRefreshRequired?.Invoke(this, EventArgs.Empty);
			
			//if(Program.GetSelectedSchedule().GetID() == scheduleControl.GetScheduleID())
			//	selectedScheduleControl = scheduleControl;
		}
		
		public void RemoveItem(ScheduleControl scheduleControl)
		{
			localisedItems.Remove(scheduleControl);
			this.Controls.Remove(scheduleControl);
			if(selectedScheduleControl == scheduleControl)
				SelectScheduleControl(null);
			
			TimeslotRefreshRequired?.Invoke(this, EventArgs.Empty);
		}
		
		public void RefreshSchedules()
		{
			localisedItems.Clear();
			this.Controls.Clear();
			selectedScheduleControl = null;
			
			long? selectedID = Program.GetSelectedSchedule() is null ? null : Program.GetSelectedSchedule().GetID();
			
			List<Schedule> list = Program.GetSchedules();
			list.Sort((Schedule s1, Schedule s2) => s1.GetName().CompareTo(s2.GetName()));
			
			foreach(Schedule schedule in list)
			{
				ScheduleControl scheduleControl = new ScheduleControl(schedule);
				
				AddItem(scheduleControl);
				
				if(schedule.GetID() == selectedID)
					scheduleControl.SelectSchedule();
			}
		}
		
		public void CreateNewSchedule(object s, EventArgs e)
		{
			ScheduleControl scheduleControl = ScheduleControl.CreateNewSchedule();
			this.AddItem(scheduleControl);
			scheduleControl.BeginRename();
		}
		
		public void SelectScheduleControl(ScheduleControl scheduleControl)
		{
			if(!(selectedScheduleControl is null))
				selectedScheduleControl.DeselectSchedule();
			selectedScheduleControl = scheduleControl;
		}
		
		private void InitContextMenu()
		{
			contextMenu = new LocalisedContextMenuStrip();
			
			contextMenu.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("New Schedule"), null, this.CreateNewSchedule, "New Schedule"));
			
			this.ContextMenuStrip = contextMenu;
		}
		
		protected override void OnClick(EventArgs e)
		{
			SelectScheduleControl(null);
		}
	}
}
