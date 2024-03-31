using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Planner
{
	public class CellViewPanel : TableLayoutPanel
	{
		public event EventHandler TimeslotSelected;
		public event EventHandler TimeslotChanged;
		public event EventHandler TimeslotRefreshRequired;
		
		
		private LocalisedContextMenuStrip timeslotEntryContextMenu;
		
		private Label titleLabel;
		private List<TimeslotEntry> timeslotEntryList;
		private TimeslotEntry selectedTimeslotEntry;
		
		private DateTime? date;
		
		
		public CellViewPanel()
		{
			selectedTimeslotEntry = null;
			date = null;
			timeslotEntryList = new List<TimeslotEntry>();
			//this.ContextMenuStrip = contextMenu;
			this.BackColor = Program.Config.GetBackColour();
			
			this.RowCount = 0;
			this.ColumnCount = 1;
			this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			this.Dock = DockStyle.Fill;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			
			titleLabel = new Label();
			
			titleLabel.Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
			titleLabel.Dock = DockStyle.Fill;
			titleLabel.AutoSize = false;
			titleLabel.BackColor = Program.Config.GetHoverColour();
			titleLabel.Margin = new Padding(0, 0, 0, 0);
			titleLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			
			this.Controls.Add(titleLabel);
			this.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
			this.Controls.Add(new Control());
			this.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
			this.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
			
			//timeslotEntryRowStyle = new RowStyle(SizeType.Absolute, 20);
			
			InitTimeslotEntryContextMenu();
			RefreshTitleLabel();
		}
		
		
		public void SetDate(DateTime? date)
		{
			this.date = date;
			RefreshAll();
		}
		public DateTime? GetDate() { return this.date; }
		
		
		private void SelectTimeslotEntry(TimeslotEntry timeslotEntry)
		{
			DeselectTimeslotEntry();
			timeslotEntry.TimeslotEntrySelected();
		}
		
		private void DeselectTimeslotEntry()
		{
			selectedTimeslotEntry?.TimeslotEntryDeselected();
			selectedTimeslotEntry = null;
		}
		
		private void AddTimeslotEntries(List<Timeslot> timeslotList)
		{
			if(timeslotList is null)
				return;
			
			foreach(Timeslot timeslot in timeslotList)
				AddTimeslotEntry(new TimeslotEntry(timeslot));
		}
		
		private void AddTimeslotEntry(TimeslotEntry timeslotEntry)
		{
			//this.RowStyles.Add(timeslotEntryRowStyle);
			this.Controls.Add(timeslotEntry);
			timeslotEntryList.Add(timeslotEntry);
			timeslotEntry.ContextMenuStrip = timeslotEntryContextMenu;
			timeslotEntry.TimeslotSelected += SelectTimeslotEntry;
			timeslotEntry.TimeslotChanged += (object s, EventArgs e) => this.TimeslotChanged?.Invoke(s, e);
		}
		
		private void RemoveTimeslotEntry(TimeslotEntry timeslotEntry)
		{
			//this.RowStyles.Remove(timeslotEntryRowStyle);
			this.Controls.Remove(timeslotEntry);
			
			if(selectedTimeslotEntry == timeslotEntry)
				DeselectTimeslotEntry();
		}
		
		private void RefreshTitleLabel()
		{
			titleLabel.Text = date is null ? "" : date?.ToString("dd.MM.yyyy");
		}
		
		private void RefreshAll()
		{
			foreach(TimeslotEntry entry in timeslotEntryList)
				RemoveTimeslotEntry(entry);
			
			timeslotEntryList.Clear();
			
			if(date is null)
			{
				RefreshTitleLabel();
				return;
			}
			
			AddTimeslotEntries(Program.GetTimeslots((DateTime)date));
			RefreshTitleLabel();
		}
		
		private void SelectTimeslotEntry(object s, EventArgs e)
		{
			DeselectTimeslotEntry();
			
			TimeslotEntry timeslotEntry = s as TimeslotEntry;
			
			if(timeslotEntry is null)
				return;
			
			timeslotEntry.TimeslotEntrySelected();
			selectedTimeslotEntry = timeslotEntry;
			
			TimeslotSelected?.Invoke(s, e);
		}
		
		private void InitTimeslotEntryContextMenu()
		{
			timeslotEntryContextMenu = new LocalisedContextMenuStrip();
			
			timeslotEntryContextMenu.AddMenuItem(new LocalisedToolStripMenuItem(
				PlannerIO.GetResourceString("Edit"),
				null,
				(object s, EventArgs e) =>
				{
					if(selectedTimeslotEntry is null)
						return;
					
					selectedTimeslotEntry.EnterEditMode();
				},
				"Edit"));
			timeslotEntryContextMenu.Items.Add(new ToolStripSeparator());
			timeslotEntryContextMenu.AddMenuItem(new LocalisedToolStripMenuItem(
				PlannerIO.GetResourceString("Delete"),
				null,
				(object s, EventArgs e) =>
				{
					if(selectedTimeslotEntry is null)
						return;
					
					Program.DeleteTimeslot(selectedTimeslotEntry.GetTimeslot());
					RemoveTimeslotEntry(selectedTimeslotEntry);
					TimeslotRefreshRequired?.Invoke(this, EventArgs.Empty);
				},
				"Delete"));
		}
	}
}
