using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace Planner
{
	public class LocalisedContextMenuStrip : ContextMenuStrip, ILocalised
	{
		private List<ILocalised> localisedItems;
		
		
		public LocalisedContextMenuStrip() : base()
		{
			localisedItems = new List<ILocalised>();
		}
		
		
		public void AddMenuItem(LocalisedToolStripMenuItem item)
		{
			this.Items.Add(item);
			localisedItems.Add(item);
		}
		
		public void AddMenuItem(ToolStripMenuItem item)
		{
			this.Items.Add(item);
		}
		
		public void ReloadStringResources()
		{
			foreach(ILocalised item in localisedItems)
				item.ReloadStringResources();
		}
	}
}
