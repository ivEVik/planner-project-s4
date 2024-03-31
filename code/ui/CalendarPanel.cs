using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Planner
{
	public class CalendarPanel : TableLayoutPanel, ILocalised
	{
		public event EventHandler ScheduleRefreshRequired;
		
		
		private LocalisedContextMenuStrip cellContextMenu;
		private LocalisedContextMenuStrip timeslotContextMenu;
		
		private Label monthLabel;
		
		private TableLayoutPanel cellPanel;
		private CellViewPanel cellViewPanel;
		private Font cellFont;
		
		private CalendarCell[][] cellArray;
		
		private CalendarCell selectedCell;
		
		private TimeslotLabel selectedTimeslotLabel;
		
		private List<ILocalised> localisedUIList;
		
		
		public CalendarPanel()
		{
			localisedUIList = new List<ILocalised>();
			InitCellContextMenu();
			InitTimeslotContextMenu();
			
			cellFont = new Font(FontFamily.GenericSansSerif, 14);
			
			this.RowCount = 4;
			this.ColumnCount = 8;
			this.Dock = DockStyle.Fill;
			this.AutoScroll = false;
			
			RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
			RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
			RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
			RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Program.Config.GetCalendarCellViewPanelWidth()));
			
			InitCalendarLabel("Calendar", new Font(FontFamily.GenericSansSerif, 14));
			this.SetColumnSpan(this.Controls[this.Controls.Count - 1], 7);
			
			InitCellViewPanel();
			
			InitMonthLabel();
			
			InitCalendarLabel("Monday");
			InitCalendarLabel("Tuesday");
			InitCalendarLabel("Wednesday");
			InitCalendarLabel("Thursday");
			InitCalendarLabel("Friday");
			InitCalendarLabel("Saturday");
			InitCalendarLabel("Sunday");
			
			InitCellPanel();
			
			this.Click += ClickChild;
			
			RefreshCells();
		}
		
		
		
		public void ReloadStringResources()
		{
			RefreshMonthName();
			cellContextMenu.ReloadStringResources();
			timeslotContextMenu.ReloadStringResources();
			
			foreach(ILocalised item in localisedUIList)
				item.ReloadStringResources();
		}
		
		public void RefreshCells()
		{
			DeselectCell();
			DeselectTimeslotLabel();
			DateTime date = Program.GetListStartDate();
			
			for(int y = 0; y < cellArray.Length; ++y)
				for(int x = 0; x < cellArray[y].Length; ++x)
				{
					cellArray[y][x].ClearTimeslotLabels();
					cellArray[y][x].SetDate(date);
					
					List<Timeslot> list = Program.GetTimeslots(date);
					
					if(!(list is null))
						cellArray[y][x].AddTimeslots(list);
					date = date.AddDays(1.0);
				}
		}
		
		private void RefreshMonthName()
		{
			monthLabel.Text = Program.GetSelectedMonthAndYearName();
		}
		
		private void ClickCell(object s, EventArgs e)
		{
			SetCellContextMenuEnabled();
			CalendarCell cell = s as CalendarCell;
			
			if(cell is null)
				return;
			
			cell.Focus();
			SelectCell(cell);
		}
		
		private void SelectCell(CalendarCell cell)
		{
			DeselectCell();
			
			cell.CellSelected();
			selectedCell = cell;
			
			cellViewPanel.SetDate(cell.GetDate());
		}
		
		private void DeselectCell()
		{
			if(selectedCell is null)
				return;
			
			selectedCell.CellDeselected();
			selectedCell = null;
			
			cellViewPanel.SetDate(null);
		}
		
		private void ClickTimeslotLabel(object s, EventArgs e)
		{
			TimeslotLabel label = s as TimeslotLabel;
			
			if(label is null)
				return;
			
			label.Focus();
			SelectTimeslotLabel(label);
		}
		
		private void SelectTimeslotLabel(TimeslotLabel timeslotLabel)
		{
			DeselectTimeslotLabel();
			
			selectedTimeslotLabel = timeslotLabel;
			selectedTimeslotLabel.TimeslotLabelSelected();
			
			Program.SelectSchedule(timeslotLabel.GetScheduleID());
			
			ScheduleRefreshRequired?.Invoke(this, EventArgs.Empty);
		}
		
		private TimeslotLabel FindTimeslotLabel(Timeslot timeslot)
		{
			for(int y = 0; y < cellArray.Length; ++y)
				for(int x = 0; x < cellArray[y].Length; ++x)
				{
					TimeslotLabel timeslotLabel = cellArray[y][x].FindTimeslotLabel(timeslot.GetScheduleID(), timeslot.GetID());
					
					if(timeslotLabel is null)
						continue;
					
					return timeslotLabel;
				}
			return null;
		}
		
		private void UpdateTimeslotEntry(Timeslot timeslot)
		{
			FindTimeslotLabel(timeslot).RefreshData();
		}
		
		private void DeselectTimeslotLabel()
		{
			if(selectedTimeslotLabel is null)
				return;
			
			selectedTimeslotLabel.TimeslotLabelDeselected();
			selectedTimeslotLabel = null;
		}
		
		private void ClickChild(object s, EventArgs e)
		{
			this.Focus();
			DeselectCell();
		}
		
		private void DeleteTimeslot(object s, EventArgs e)
		{
			TimeslotLabel timeslotLabel = s as TimeslotLabel;
			
			if(timeslotLabel is null)
				return;
			
			Program.DeleteTimeslot(timeslotLabel.GetScheduleID(), timeslotLabel.GetTimeslotID());
			
			if(timeslotLabel == selectedTimeslotLabel)
				DeselectTimeslotLabel();
			
			CalendarCell cell = timeslotLabel.Parent as CalendarCell;
			
			if(cell is null)
				return;
			
			cell.RemoveTimeslotLabel(timeslotLabel);
		}
		
		private void IncrementMonth(int x)
		{
			Program.IncrementListStartMonth(x);
			RefreshMonthName();
			RefreshCells();
		}
		
		private void InitCellPanel()
		{
			cellPanel = new TableLayoutPanel();
			this.Controls.Add(cellPanel);
			this.SetColumnSpan(Controls[Controls.Count - 1], 7);
			
			cellPanel.RowCount = 6;
			cellPanel.ColumnCount = 7;
			cellPanel.Dock = DockStyle.Fill;
			cellPanel.AutoScroll = false;
			
			cellArray = new CalendarCell[cellPanel.RowCount][];
			
			for(int y = 0; y < cellPanel.RowCount; ++y)
			{
				cellArray[y] = new CalendarCell[cellPanel.ColumnCount];
				cellPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
				
				for(int x = 0; x < cellPanel.ColumnCount; ++x)
				{
					cellArray[y][x] = new CalendarCell(cellContextMenu, cellFont, ClickCell, ClickTimeslotLabel);
					cellPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
					cellPanel.Controls.Add(cellArray[y][x]);
				}
			}
		}
		
		private void SetCellContextMenuEnabled()
		{
			if(Program.GetSelectedSchedule() is null || !Program.GetSelectedSchedule().GetShow())
			{
				cellContextMenu.Enabled = false;
				return;
			}
			cellContextMenu.Enabled = true;
		}
		
		private void InitCellContextMenu()
		{
			cellContextMenu = new LocalisedContextMenuStrip();
			
			cellContextMenu.AddMenuItem(new LocalisedToolStripMenuItem(
				PlannerIO.GetResourceString("New Event"),
				null,
				(object s, EventArgs e) => 
				{
					if(Program.GetSelectedSchedule() is null || selectedCell is null || !Program.GetSelectedSchedule().GetShow())
						return;
					
					Program.AddTimeslot(Program.GetSelectedSchedule(), new Timeslot("test", (DateTimeOffset)selectedCell.GetDate().ToUniversalTime().AddDays(1)));
					RefreshCells();
				},
				"New Event"));
		}
		
		private void InitTimeslotContextMenu()
		{
			timeslotContextMenu = new LocalisedContextMenuStrip();
			
			timeslotContextMenu.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("Delete"), null, DeleteTimeslot, "Delete"));
		}
		
		private void InitCellViewPanel()
		{
			cellViewPanel = new CellViewPanel();
			
			cellViewPanel.TimeslotSelected += (object s, EventArgs e) =>
				{
					TimeslotEntry timeslotEntry = s as TimeslotEntry;
					
					if(timeslotEntry is null)
						return;
					
					this.SelectTimeslotLabel(FindTimeslotLabel(timeslotEntry.GetTimeslot()));
				};
			
			cellViewPanel.TimeslotChanged += (object s, EventArgs e) =>
				{
					TimeslotEntry timeslotEntry = s as TimeslotEntry;
					
					if(timeslotEntry is null)
						return;
					
					this.UpdateTimeslotEntry(timeslotEntry.GetTimeslot());
				};
			
			this.Controls.Add(cellViewPanel);
			this.SetRowSpan(this.Controls[this.Controls.Count - 1], this.RowCount);
			cellViewPanel.TimeslotRefreshRequired += (object s, EventArgs e) => this.RefreshCells();
		}
		
		private void InitMonthLabel()
		{
			Label label = new Label();
			label.Font = new Font(FontFamily.GenericSansSerif, 12);
			label.TextAlign = ContentAlignment.MiddleCenter;
			label.Dock = DockStyle.Fill;
			label.AutoSize = false;
			label.Margin = new Padding(0, 0, 0, 0);
			label.BackColor = Program.Config.GetBackColour();
			label.Click += this.ClickChild;
			this.SetColumnSpan(label, this.ColumnCount - 1);
			
			this.Controls.Add(label);
			monthLabel = label;
			
			RefreshMonthName();
			
			Button button = new Button();
			button.Size = new Size(20, 20);
			button.Dock = DockStyle.Left;
			button.Text = "<";
			button.Click += (object s, EventArgs e) => this.IncrementMonth(-1);
			monthLabel.Controls.Add(button);
			
			button = new Button();
			button.Size = new Size(20, 20);
			button.Dock = DockStyle.Right;
			button.Text = ">";
			button.Click += (object s, EventArgs e) => this.IncrementMonth(1);
			monthLabel.Controls.Add(button);
		}
		
		private void InitCalendarLabel(string text, Font font = null)
		{
			LocalisedLabel label = new LocalisedLabel(text);
			label.TextAlign = ContentAlignment.MiddleCenter;
			label.Dock = DockStyle.Fill;
			label.AutoSize = false;
			label.Margin = new Padding(0, 0, 0, 0);
			label.BackColor = Program.Config.GetBackColour();
			label.Click += this.ClickChild;
			
			if(!(font is null))
				label.Font = font;
			
			localisedUIList.Add(label);
			this.Controls.Add(label);
		}
	}
}
