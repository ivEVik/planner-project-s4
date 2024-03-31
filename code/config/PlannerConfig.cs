using System.Drawing;
using System.Globalization;
using System.Text.Json.Serialization;


namespace Planner
{
	[JsonIncludePrivateFields]
	public class PlannerConfig
	{
		private string currentCultureCode;
		
		private int mainFormHeight;
		private int mainFormWidth;
		
		private int mainFormMinHeight;
		private int mainFormMinWidth;
		
		private int splitPanelLeftMinWidth;
		private int splitPanelRightMinWidth;
		
		private int calendarCellViewPanelWidth;
		
		private int selectionColourR;
		private int selectionColourG;
		private int selectionColourB;
		private int selectionColourA;
		
		private int hoverColourR;
		private int hoverColourG;
		private int hoverColourB;
		private int hoverColourA;
		
		private int backColourR;
		private int backColourG;
		private int backColourB;
		private int backColourA;
		
		private int timeslotSelectionColourR;
		private int timeslotSelectionColourG;
		private int timeslotSelectionColourB;
		private int timeslotSelectionColourA;
		
		private int timeslotHoverColourR;
		private int timeslotHoverColourG;
		private int timeslotHoverColourB;
		private int timeslotHoverColourA;
		
		private int timeslotBackColourR;
		private int timeslotBackColourG;
		private int timeslotBackColourB;
		private int timeslotBackColourA;
		
		
		public PlannerConfig()
		{
			this.currentCultureCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
			
			this.mainFormMinHeight = 700;
			this.mainFormMinWidth = 1080;
			
			this.mainFormHeight = 700;
			this.mainFormWidth = 1080;
			
			this.splitPanelLeftMinWidth = 150;
			this.splitPanelRightMinWidth = 900;
			
			this.calendarCellViewPanelWidth = 300;
			
			SetSelectionColour(Color.LightSkyBlue);
			SetHoverColour(Color.LightBlue);
			SetBackColour(Color.White);
			
			SetTimeslotSelectionColour(Color.LightSeaGreen);
			SetTimeslotHoverColour(Color.MediumSeaGreen);
			SetTimeslotBackColour(Color.LightGreen);
		}
		
		
		public void SetCurrentCultureCode(string currentCultureCode) { this.currentCultureCode = currentCultureCode; }
		public string GetCurrentCultureCode() { return currentCultureCode; }
		
		public void SetMainFormHeight(int mainFormHeight) { this.mainFormHeight = mainFormHeight < this.mainFormMinHeight ? this.mainFormMinHeight : mainFormHeight; }
		public int GetMainFormHeight() { return mainFormHeight; }
		
		public void SetMainFormWidth(int mainFormWidth) { this.mainFormWidth = mainFormWidth < this.mainFormMinWidth ? this.mainFormMinWidth : mainFormWidth; }
		public int GetMainFormWidth() { return mainFormWidth; }
		
		public void SetSplitPanelLeftMinWidth(int splitPanelLeftMinWidth) { this.splitPanelLeftMinWidth = splitPanelLeftMinWidth; }
		public int GetSplitPanelLeftMinWidth() { return splitPanelLeftMinWidth; }
		
		public void SetSplitPanelRightMinWidth(int splitPanelRightMinWidth) { this.splitPanelRightMinWidth = splitPanelRightMinWidth; }
		public int GetSplitPanelRightMinWidth() { return splitPanelRightMinWidth; }
		
		public void SetCalendarCellViewPanelWidth(int calendarCellViewPanelWidth) { this.calendarCellViewPanelWidth = calendarCellViewPanelWidth; }
		public int GetCalendarCellViewPanelWidth() { return calendarCellViewPanelWidth; }
		
		public void SetSelectionColour(Color selectionColour)
			{ SetColour(selectionColour, out selectionColourR, out selectionColourG, out selectionColourB, out selectionColourA); }
		public Color GetSelectionColour() { return Color.FromArgb(selectionColourA, selectionColourR, selectionColourG, selectionColourB); }
		
		public void SetHoverColour(Color hoverColour)
			{ SetColour(hoverColour, out hoverColourR, out hoverColourG, out hoverColourB, out hoverColourA); }
		public Color GetHoverColour() { return Color.FromArgb(hoverColourA, hoverColourR, hoverColourG, hoverColourB); }
		
		public void SetBackColour(Color backColour)
			{ SetColour(backColour, out backColourR, out backColourG, out backColourB, out backColourA); }
		public Color GetBackColour() { return Color.FromArgb(backColourA, backColourR, backColourG, backColourB); }
		
		public void SetTimeslotSelectionColour(Color timeslotSelectionColour)
			{ SetColour(timeslotSelectionColour, out timeslotSelectionColourR, out timeslotSelectionColourG, out timeslotSelectionColourB, out timeslotSelectionColourA); }
		public Color GetTimeslotSelectionColour() { return Color.FromArgb(timeslotSelectionColourA, timeslotSelectionColourR, timeslotSelectionColourG, timeslotSelectionColourB); }
		
		public void SetTimeslotHoverColour(Color timeslotHoverColour)
			{ SetColour(timeslotHoverColour, out timeslotHoverColourR, out timeslotHoverColourG, out timeslotHoverColourB, out timeslotHoverColourA); }
		public Color GetTimeslotHoverColour() { return Color.FromArgb(timeslotHoverColourA, timeslotHoverColourR, timeslotHoverColourG, timeslotHoverColourB); }
		
		public void SetTimeslotBackColour(Color timeslotBackColour)
			{ SetColour(timeslotBackColour, out timeslotBackColourR, out timeslotBackColourG, out timeslotBackColourB, out timeslotBackColourA); }
		public Color GetTimeslotBackColour() { return Color.FromArgb(timeslotBackColourA, timeslotBackColourR, timeslotBackColourG, timeslotBackColourB); }
		
		private void SetColour(Color colour, out int colourR, out int colourG, out int colourB, out int colourA)
		{
			colourR = colour.R;
			colourG = colour.G;
			colourB = colour.B;
			colourA = colour.A;
		}
		
		public void SetMainFormSize(Size mainFormSize)
		{
			SetMainFormHeight(mainFormSize.Height);
			SetMainFormWidth(mainFormSize.Width);
		}
		public Size GetMainFormSize() { return new Size(this.mainFormWidth, this.mainFormHeight); }
		
		public Size GetMainFormMinSize() {return new Size(this.mainFormMinWidth, this.mainFormMinHeight); }
		
		
		public void ApplyConfig()
		{
			SetUILanguage();
		}
		
		public void Save()
		{
			PlannerIO.SaveConfig(this);
		}
		
		public void SetUILanguage(string cultureCode, bool save = true)
		{
			currentCultureCode = cultureCode;
			SetUILanguage(save);
		}
		
		public void SetUILanguage(bool save = true)
		{
			CultureInfo.CurrentUICulture = new CultureInfo(currentCultureCode, true);
			
			if(save)
				Save();
		}
	}
}
