using System;
using System.Windows.Forms;


namespace Planner
{
	public class LocalisedToolStripMenuItem : ToolStripMenuItem, ILocalised
	{
		public LocalisedToolStripMenuItem (string text, System.Drawing.Image image, EventHandler onClick, string name) : base(text, image, onClick, name)
		{
			this.Click += (object s, EventArgs e) => { this.Select(); };
		}
		
		public void ReloadStringResources()
		{
			this.Text = PlannerIO.GetResourceString(this.Name);
		}
	}
}
