using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Planner
{
	public class TimeslotEntry : Control
	{
		public event EventHandler TimeslotSelected;
		public event EventHandler TimeslotChanged;
		
		
		private Timeslot representedTimeslot;
		
		private Label timeLabel;
		private DateTimePicker startTimePicker;
		private DateTimePicker endTimePicker;
		
		private Label nameLabel;
		private TextBox nameEditBox;
		
		private bool selected;
		private bool editing;
		
		
		public TimeslotEntry(Timeslot timeslot)
		{
			this.selected = false;
			this.editing = false;
			this.representedTimeslot = timeslot;
			
			DateTime minDate = timeslot.GetStartTimeLocal().Date;
			DateTime maxDate = minDate.AddHours(23).AddMinutes(59);
			
			this.Dock = DockStyle.Top;
			this.Size = new Size(20, 30);
			this.AutoSize = false;
			this.BackColor = Program.Config.GetTimeslotBackColour();
			this.Margin = new Padding(0, 1, 0, 1);
			//timeslotEntry.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			
			startTimePicker = new DateTimePicker();
			startTimePicker.Location = new Point(0, 2);
			startTimePicker.Size = new Size(50, 20);
			startTimePicker.MinDate = minDate;
			startTimePicker.MaxDate = maxDate;
			startTimePicker.CustomFormat = "HH:mm";
			startTimePicker.Format = DateTimePickerFormat.Custom;
			startTimePicker.ShowUpDown = true;
			startTimePicker.Value = minDate;
			startTimePicker.Enabled = false;
			startTimePicker.Hide();
			
			endTimePicker = new DateTimePicker();
			endTimePicker.Location = new Point(60, 2);
			endTimePicker.Size = new Size(50, 20);
			endTimePicker.MinDate = minDate;
			endTimePicker.MaxDate = maxDate;
			endTimePicker.CustomFormat = "HH:mm";
			endTimePicker.Format = DateTimePickerFormat.Custom;
			endTimePicker.ShowUpDown = true;
			//endTimePicker.Dock = DockStyle.Left;
			endTimePicker.Value = maxDate;
			endTimePicker.Enabled = false;
			endTimePicker.Hide();
			
			nameEditBox = new TextBox();
			nameEditBox.Location = new Point(0, 2);
			nameEditBox.Size = new Size(150, 30);
			//nameEditBox.TextAlign = ContentAlignment.MiddleLeft;
			//nameEditBox.Dock = DockStyle.Fill;
			nameEditBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			nameEditBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
			nameEditBox.Multiline = false;
			nameEditBox.Enabled = false;
			nameEditBox.Hide();
			
			startTimePicker.KeyDown += this.TimeslotEntryKeyDown;
			endTimePicker.KeyDown += this.TimeslotEntryKeyDown;
			nameEditBox.KeyDown += this.TimeslotEntryKeyDown;
			
			timeLabel = new Label();
			timeLabel.Dock = DockStyle.Left;
			timeLabel.TextAlign = ContentAlignment.MiddleLeft;
			timeLabel.Size = new Size(70, 20);
			timeLabel.AutoSize = false;
			
			nameLabel = new Label();
			nameLabel.Size = new Size(150, 30);
			//nameLabel.Margin = new Padding(70, 0, 0, 0);
			nameLabel.TextAlign = ContentAlignment.MiddleLeft;
			nameLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
			//nameLabel.Dock = DockStyle.Right;
			//nameLabel.AutoSize = true;
			
			this.Controls.Add(startTimePicker);
			this.Controls.Add(endTimePicker);
			this.Controls.Add(timeLabel);
			this.Controls.Add(nameLabel);
			nameLabel.Controls.Add(nameEditBox);
			
			this.MouseEnter += TimeslotEntryOnMouseEnter;
			this.MouseLeave += TimeslotEntryOnMouseLeave;
			this.MouseDown += TimeslotEntryOnMouseDown;
			this.DoubleClick += TimeslotEntryOnDoubleClick;
			this.KeyDown += TimeslotEntryKeyDown;
			
			timeLabel.MouseDown += TimeslotEntryOnMouseDown;
			timeLabel.DoubleClick += TimeslotEntryOnDoubleClick;
			timeLabel.KeyDown += TimeslotEntryKeyDown;
			
			nameLabel.MouseDown += TimeslotEntryOnMouseDown;
			nameLabel.DoubleClick += TimeslotEntryOnDoubleClick;
			nameLabel.KeyDown += TimeslotEntryKeyDown;
			
			foreach(Control control in this.Controls)
			{
				control.MouseEnter += TimeslotEntryOnMouseEnter;
				control.MouseLeave += TimeslotEntryOnMouseLeave;
				control.KeyDown += TimeslotEntryKeyDown;
			}
			
			RefreshData();
		}
		
		
		public Timeslot GetTimeslot() { return this.representedTimeslot; }
		
		
		public void TimeslotEntrySelected()
		{
			selected = true;
			this.BackColor = Program.Config.GetTimeslotSelectionColour();
		}
		
		public void TimeslotEntryDeselected()
		{
			selected = false;
			this.BackColor = Program.Config.GetTimeslotBackColour();
			
			if(editing)
				ExitEditMode();
		}
		
		public void EnterEditMode()
		{
			editing = true;
			startTimePicker.Value = representedTimeslot.GetStartTimeLocal().DateTime;
			endTimePicker.Value = representedTimeslot.GetEndTimeLocal().DateTime;
			nameEditBox.Text = nameLabel.Text;
			
			//nameLabel.Hide();
			timeLabel.Hide();
			
			startTimePicker.Show();
			endTimePicker.Show();
			nameEditBox.Show();
			
			startTimePicker.Enabled = true;
			endTimePicker.Enabled = true;
			nameEditBox.Enabled = true;
			
			this.Select();
		}
		
		public void ExitEditMode()
		{
			editing = false;
			Program.EditTimeslot(representedTimeslot, startTimePicker.Value, endTimePicker.Value, nameEditBox.Text);
			
			startTimePicker.Enabled = false;
			endTimePicker.Enabled = false;
			nameEditBox.Enabled = false;
			
			startTimePicker.Hide();
			endTimePicker.Hide();
			nameEditBox.Hide();
			
			RefreshData();
			
			nameLabel.Show();
			timeLabel.Show();
			
			TimeslotChanged?.Invoke(this, EventArgs.Empty);
		}
		
		private void RefreshData()
		{
			timeLabel.Text = $"{representedTimeslot.GetStartTimeLocal().ToString("HH:mm")}-{representedTimeslot.GetEndTimeLocal().ToString("HH:mm")}";
			nameLabel.Text = representedTimeslot.GetName();
		}
		
		private void TimeslotEntryOnMouseEnter(object s, EventArgs e)
		{
			if(selected)
				return;
			
			this.BackColor = Program.Config.GetTimeslotHoverColour();
		}
		
		private void TimeslotEntryOnMouseLeave(object s, EventArgs e)
		{
			if(selected)
				return;
			
			this.BackColor = Program.Config.GetTimeslotBackColour();
		}
		
		private void TimeslotEntryOnDoubleClick(object s, EventArgs e)
		{
			if(editing)
				return;
			
			EnterEditMode();
		}
		
		private void TimeslotEntryKeyDown(object s, KeyEventArgs e)
		{
			//if(editing && e.KeyCode == Keys.Return && e.KeyCode == Keys.Enter)
			//	ExitEditMode();
			
			switch(e.KeyCode)
			{
				case Keys.Return:
					if(editing)
						ExitEditMode();
					return;
			}
		}
		
		private void TimeslotEntryOnMouseDown(object s, MouseEventArgs e)
		{
			TimeslotSelected?.Invoke(this, EventArgs.Empty);
		}
	}
}
