using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;


namespace Planner
{
	public class ScheduleControl : TableLayoutPanel, ILocalised
	{
		public event EventHandler TimeslotRefreshRequired;
		
		
		private long scheduleID;
		
		private TextBox renameBox;
		private Panel panel;
		private Label label;
		private CheckBox checkBox;
		private LocalisedContextMenuStrip contextMenu;
		
		
		public ScheduleControl(Schedule schedule) : this(schedule.GetID()) { }
		
		public ScheduleControl(long scheduleID)
		{
			this.scheduleID = scheduleID;
			
			this.Size = new Size(20, 30);
			this.RowCount = 1;
			this.ColumnCount = 2;
			this.Dock = DockStyle.Top;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			
			checkBox = new CheckBox();
			checkBox.Dock = DockStyle.Fill;
			checkBox.Click += (object s, EventArgs e) => { Program.SetScheduleShow(scheduleID, checkBox.Checked); TimeslotRefreshRequired?.Invoke(this, EventArgs.Empty); };
			
			panel = new Panel();
			panel.Size = new Size(20, 20);
			panel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			
			label = new Label();
			label.Dock = DockStyle.Fill;
			label.AutoSize = false;
			panel.Controls.Add(label);
			
			InitContextMenu();
			
			LoadScheduleData();
			
			this.Controls.Add(checkBox);
			this.Controls.Add(panel);
			
			this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
			this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			
			label.Click += (object s, EventArgs e) => { this.OnClick(e); };
			label.MouseEnter += this.ScheduleControlOnMouseEnter;
			label.MouseLeave += this.ScheduleControlOnMouseLeave;
			
			this.MouseEnter += this.ScheduleControlOnMouseEnter;
			this.MouseLeave += this.ScheduleControlOnMouseLeave;
			this.KeyDown += this.ScheduleControlKeyDown;
			//this.Enter += SelectSchedule;
		}
		
		
		public long GetScheduleID() { return scheduleID; }
		public string GetLabelText() { return label.Text; }
		
		
		public static ScheduleControl CreateNewSchedule()
		{
			//ScheduleControl scheduleControl = new ScheduleControl(Program.CreateNewSchedule(PlannerIO.GetResourceString("New Schedule")));
			//scheduleControl.BeginRename();
			return new ScheduleControl(Program.CreateNewSchedule(PlannerIO.GetResourceString("New Schedule")));
		}
		
		
		public void ReloadStringResources()
		{
			contextMenu.ReloadStringResources();
		}
		
		private void InitContextMenu()
		{
			contextMenu = new LocalisedContextMenuStrip();
			
			contextMenu.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("Delete"), null, Delete, "Delete"));
			contextMenu.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("Rename"), null, (object s, EventArgs e) => { BeginRename(); }, "Rename"));
			
			this.ContextMenuStrip = contextMenu;
		}
		
		public void BeginRename()
		{
			this.Focus();
			
			label.Visible = false;
			
			renameBox = new TextBox();
			renameBox.Dock = DockStyle.Fill;
			renameBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			renameBox.Multiline = false;
			renameBox.Text = label.Text;
			panel.Controls.Add(renameBox);
			
			renameBox.Leave += this.EndRename;
			renameBox.KeyDown += this.RenameBoxKeyDown;
			
			renameBox.Select();
			//renameBox.Shown += (object s, EventArgs e) => { Focus(); };
		}
		
		public void SelectSchedule()
		{
			((ScheduleTableLayoutPanel)this.Parent).SelectScheduleControl(this);
			Program.SelectSchedule(scheduleID);
			
			this.BackColor = Program.Config.GetSelectionColour();
		}
		
		public void DeselectSchedule()
		{
			Program.DeselectSchedule();
			
			TimeslotRefreshRequired?.Invoke(this, EventArgs.Empty);
			
			this.BackColor = Program.Config.GetBackColour();
		}
		
		private void LoadScheduleData()
		{
			label.Text = Program.GetSchedule(scheduleID).GetName();
			checkBox.Checked = Program.GetSchedule(scheduleID).GetShow();
		}
		
		private void Delete(object s, EventArgs e)
		{
			Program.DeleteSchedule(scheduleID);
			if((ScheduleTableLayoutPanel)this.Parent is null)
				return;
			
			((ScheduleTableLayoutPanel)this.Parent).RemoveItem(this);
		}
		
		private void EndRename(object s, EventArgs e)
		{
			Program.RenameSchedule(scheduleID, renameBox.Text);
			
			if((ScheduleTableLayoutPanel)this.Parent is null)
				return;
			
			((ScheduleTableLayoutPanel)this.Parent).RefreshSchedules();
		}
		
		private void RenameBoxKeyDown(object s, KeyEventArgs e)
		{
			TextBox textBox = s as TextBox;
			
			if(textBox != null && e.KeyCode == Keys.Return && e.KeyCode == Keys.Enter)
				this.Select();
		}
		
		private void ScheduleControlKeyDown(object s, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.F2)
				BeginRename();
		}
		
		private void ScheduleControlOnMouseEnter(object s, EventArgs e)
		{
			Schedule schedule = Program.GetSelectedSchedule();
			if(!(schedule is null) && schedule.GetID() == scheduleID)
				return;
			
			this.BackColor = Program.Config.GetHoverColour();
		}
		
		private void ScheduleControlOnMouseLeave(object s, EventArgs e)
		{
			Schedule schedule = Program.GetSelectedSchedule();
			if(!(schedule is null) && schedule.GetID() == scheduleID)
				return;
			
			this.BackColor = Program.Config.GetBackColour();
		}
		
		protected override void OnClick(EventArgs e)
		{
			this.Focus();
			this.SelectSchedule();
		}
	}
}
