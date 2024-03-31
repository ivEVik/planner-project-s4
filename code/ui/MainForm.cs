using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;


namespace Planner
{
	public class MainForm : Form, ILocalised
	{
		private Timer mainTimer;
		
		private MenuStrip toolbar;
		private NotifyIcon notifyIcon;
		private LocalisedContextMenuStrip contextMenuTray;
		private List<ILocalised> localisedUIList;
		
		private ScheduleTableLayoutPanel schedulePanel;
		private CalendarPanel calendarPanel;
		private SplitContainer splitContainer;
		
		private bool allowClosing;
		
		public MainForm()
		{
			allowClosing = false;
			this.FormClosing += this.MainFormClosing;
			this.SizeChanged += this.MainFormSizeChanged;
			
			Name = "Planner Prototype";
			Text = PlannerIO.GetResourceString(Name);
			
			localisedUIList = new List<ILocalised>();
			
			splitContainer = new SplitContainer();
			splitContainer.Dock = DockStyle.Fill;
			splitContainer.Location = new Point(0, 0);
			splitContainer.SplitterIncrement = 10;
			splitContainer.SplitterWidth = 6;
			splitContainer.FixedPanel = FixedPanel.Panel1;
			
			splitContainer.SplitterMoving += new SplitterCancelEventHandler(splitContainer_SplitterMoving);
			splitContainer.SplitterMoved += new SplitterEventHandler(splitContainer_SplitterMoved);
			
			splitContainer.Panel1.Click += this.ClickChild;
			splitContainer.Panel2.Click += this.ClickChild;
			
			schedulePanel = new ScheduleTableLayoutPanel();
			localisedUIList.Add(schedulePanel);
			splitContainer.Panel1.Controls.Add(schedulePanel);
			schedulePanel.TimeslotRefreshRequired += OnTimeslotRefreshRequired;
			
			calendarPanel = new CalendarPanel();
			localisedUIList.Add(calendarPanel);
			splitContainer.Panel2.Controls.Add(calendarPanel);
			calendarPanel.ScheduleRefreshRequired += OnScheduleRefreshRequired;
			
			InitToolbar();
			InitSystemTrayIcon();
			
			//schedulePanel.Click += ClickChild;
			
			splitContainer.Panel1.Padding = new Padding(0, toolbar.Size.Height, 0, 0);
			splitContainer.Panel2.Padding = new Padding(0, toolbar.Size.Height, 0, 0);
			
			//this.AcceptButton = new Button();
			this.Controls.Add(splitContainer);
			
			this.Size = Program.Config.GetMainFormSize();
			this.MinimumSize = Program.Config.GetMainFormMinSize();
			splitContainer.Panel1MinSize = Program.Config.GetSplitPanelLeftMinWidth();
			splitContainer.Panel2MinSize = Program.Config.GetSplitPanelRightMinWidth();
			
			InitTimer();
		}
		
		public void ReloadStringResources()
		{
			Text = PlannerIO.GetResourceString(Name);
			notifyIcon.Text = PlannerIO.GetResourceString(Name);
			
			foreach(ILocalised localisedItem in localisedUIList)
				localisedItem.ReloadStringResources();
		}
		
		private void MainFormClosing(object s, FormClosingEventArgs e)
		{
			if(!allowClosing)
			{
				e.Cancel = true;
				MinimiseToTray();
				return;
			}
		}
		
		private void MainFormSizeChanged(object s, EventArgs e)
		{
			int splitterDistance = this.Width - Program.Config.GetSplitPanelRightMinWidth();
			
			if(this.WindowState != FormWindowState.Minimized && splitContainer.SplitterDistance > splitterDistance && splitterDistance > 0)
				splitContainer.SplitterDistance = splitterDistance;
			
			//splitContainer.Invalidate();
		}
		
		private void MinimiseToTray()
		{
			this.WindowState = FormWindowState.Minimized;
			Hide();
		}
		
		private void FullClose(object s, EventArgs e)
		{
			allowClosing = true;
			notifyIcon.Visible = false;
			
			ToastNotificationManagerCompat.Uninstall();
			
			this.Close();
		}
		
		private void notifyIcon_DoubleClick(object s, EventArgs e)
		{
			Show();
			this.WindowState = FormWindowState.Normal;
			//Activate();
		}
		
		private void splitContainer_SplitterMoving(object s, SplitterCancelEventArgs e)
		{
			Cursor.Current = Cursors.NoMoveVert;
		}
		
		private void splitContainer_SplitterMoved(object s, SplitterEventArgs e)
		{
			Cursor.Current = Cursors.Default;
		}
		
		private void SetUILanguage(string cultureCode)
		{
			Program.Config.SetUILanguage(cultureCode);
			ReloadStringResources();
		}
		
		private void InitToolbar()
		{
			LocalisedContextMenuStrip contextMenuFile = new LocalisedContextMenuStrip();
			localisedUIList.Add(contextMenuFile);
			
			contextMenuFile.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("New Schedule"), null, schedulePanel.CreateNewSchedule, "New Schedule"));
			contextMenuFile.Items.Add(new ToolStripSeparator());
			contextMenuFile.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("Exit"), null, FullClose, "Exit"));
			
			LocalisedToolStripMenuItem tsItemFile = new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("File"), null, null, "File");
			tsItemFile.DropDown = contextMenuFile;
			localisedUIList.Add(tsItemFile);
			
			LocalisedContextMenuStrip contextMenuLanguage = new LocalisedContextMenuStrip();
			localisedUIList.Add(contextMenuLanguage);
			contextMenuLanguage.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("English"), null, (object s, EventArgs e) => { SetUILanguage("en"); }, "English"));
			contextMenuLanguage.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("Russian"), null, (object s, EventArgs e) => { SetUILanguage("ru"); }, "Russian"));
			
			LocalisedContextMenuStrip contextMenuSettings = new LocalisedContextMenuStrip();
			localisedUIList.Add(contextMenuSettings);
			
			LocalisedToolStripMenuItem tsItemLanguage = new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("Language"), null, null, "Language");
			tsItemLanguage.DropDown = contextMenuLanguage;
			contextMenuSettings.AddMenuItem(tsItemLanguage);
			
			LocalisedToolStripMenuItem tsItemSettings = new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("Settings"), null, null, "Settings");
			localisedUIList.Add(tsItemSettings);
			tsItemSettings.DropDown = contextMenuSettings;
			
			toolbar = new MenuStrip();
			toolbar.Dock = DockStyle.Top;
			toolbar.Items.Add(tsItemFile);
			toolbar.Items.Add(tsItemSettings);
			toolbar.Click += this.ClickChild;
			
			//this.ContextMenuStrip = contextMenuFile;
			this.Controls.Add(toolbar);
		}
		
		private void InitSystemTrayIcon()
		{
			contextMenuTray = new LocalisedContextMenuStrip();
			localisedUIList.Add(contextMenuTray);
			
			contextMenuTray.AddMenuItem(new LocalisedToolStripMenuItem(PlannerIO.GetResourceString("Exit"), null, FullClose, "Exit"));
			
			notifyIcon = new NotifyIcon();
			notifyIcon.Icon = PlannerIO.GetResourceIcon("Icon");
			notifyIcon.ContextMenuStrip = contextMenuTray;
			notifyIcon.Text = PlannerIO.GetResourceString(Name);
			notifyIcon.Visible = true;
			notifyIcon.DoubleClick += this.notifyIcon_DoubleClick;
		}
		
		private void InitTimer()
		{
			mainTimer = new Timer();
			mainTimer.Interval = 1000;
			mainTimer.Tick += OnTimerTick;
			
			mainTimer.Start();
		}
		
		protected override void OnClick(EventArgs e)
		{
			ClickChild(this, e);
		}
		
		private void ClickChild(object s, EventArgs e)
		{
			toolbar.Focus();
		}
		
		private void OnScheduleRefreshRequired(object s, EventArgs e)
		{
			schedulePanel.RefreshSchedules();
		}
		
		private void OnTimeslotRefreshRequired(object s, EventArgs e)
		{
			calendarPanel.RefreshCells();
		}
		
		private void OnTimerTick(object s, EventArgs e)
		{
			List<Timeslot> timeslots = Program.GetNewActiveTimeslots();
			
			foreach(Timeslot timeslot in timeslots)
				SendNotification(timeslot);
		}
		
		private void SendNotification(Timeslot timeslot)
		{
			new ToastContentBuilder()
				.AddText(timeslot.GetName())
				.AddText($"{timeslot.GetStartTimeLocal().ToString("HH:mm")}-{timeslot.GetEndTimeLocal().ToString("HH:mm")}")
				.Show();
		}
	}
}
