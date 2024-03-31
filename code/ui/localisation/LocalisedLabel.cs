using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace Planner
{
	public class LocalisedLabel : Label, ILocalised
	{
		private string unlocalisedText;
		
		
		public LocalisedLabel(string unlocalisedText) : base()
		{
			this.unlocalisedText = unlocalisedText;
			ReloadStringResources();
		}
		
		
		public void SetUnlocalisedText(string unlocalisedText) { this.unlocalisedText = unlocalisedText; }
		public string GetUnlocalisedText() { return this.unlocalisedText; }
		
		
		public void ReloadStringResources()
		{
			this.Text = PlannerIO.GetResourceString(this.unlocalisedText);
		}
	}
}
