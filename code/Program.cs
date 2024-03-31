using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Planner
{
	public static class Program
	{
		public static PlannerConfig Config;
		
		private static Dictionary<long, Schedule> scheduleDict;
		private static Schedule selectedSchedule;
		
		private static Dictionary<DateTime, List<Timeslot>> timeslotDict;
		private static DateTime listStartDate;
		
		
		[STAThread]
		public static void Main()
		{
			PlannerIO.Init();
			
			Config = PlannerIO.LoadConfig();
			Config.ApplyConfig();
			
			scheduleDict = PlannerIO.LoadAllSchedules();
			
			SetListStartDateDefault();
			PopulateTimeslotDict();
			
			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm());
			
			PlannerIO.SaveAllSchedules();
			PlannerIO.SaveConfig(Config);
		}
		
		
		public static Schedule GetSchedule(long id) { return scheduleDict[id]; }
		public static List<Schedule> GetSchedules() { return new List<Schedule>(scheduleDict.Values); }
		
		public static Schedule GetSelectedSchedule() { return selectedSchedule; }
		
		public static void SetListStartDateDefault() { listStartDate = GetStartOfWeek(GetStartOfMonth(DateTime.UtcNow.Date)); }
		//public static void SetListStartDate(DateTime newListStartDate) { listStartDate = GetStartOfWeek(GetStartOfMonth(newListStartDate.ToUniversalTime().Date)); }
		public static DateTime GetListStartDate() { return listStartDate; }
		public static DateTime GetListEndDate() { return listStartDate.AddDays(42); }
		
		public static void IncrementListStartMonth(int n)
		{
			DateTime newStartDate = listStartDate;
			
			if(listStartDate.Month != GetSelectedMonth())
				newStartDate = newStartDate.AddDays(7);
			
			listStartDate = GetStartOfWeek(newStartDate.AddMonths(n));
			PopulateTimeslotDict();
		}
		
		public static DateTime GetStartOfWeek(DateTime date) { return date.AddDays(-1.0 * GetDayOfWeek(date)); }
		public static DateTime GetStartOfMonth(DateTime date) { return new DateTime(date.Year, date.Month, 1); }
		
		public static int GetSelectedMonth() { return listStartDate.AddDays(10).Month; }
		
		public static string GetSelectedMonthAndYearName()
		{
			DateTime date = listStartDate.AddDays(10);
			return $"{PlannerIO.GetResourceString(date.ToString("MMMM", CultureInfo.InvariantCulture))} {date.ToString("yyyy", CultureInfo.InvariantCulture)}";
		}
		
		public static void AddTimeslot(Schedule schedule, Timeslot timeslot)
		{
			schedule.Add(timeslot);
			
			if(!schedule.GetShow())
				return;
			
			PopulateTimeslotDict();
		}
		public static List<Timeslot> GetTimeslots(DateTime date) { return timeslotDict.ContainsKey(date) ? timeslotDict[date] : null; }
		public static List<Timeslot> GetNewActiveTimeslots()
		{
			List<Timeslot> result = new List<Timeslot>();
			List<Timeslot> timeslotList = GetTimeslots(DateTimeOffset.UtcNow.Date);
			
			if(timeslotList is null)
				return result;
			
			foreach(Timeslot timeslot in timeslotList)
				if(timeslot.IsNewlyActive())
					result.Add(timeslot);
			
			return result;
		}
		public static Timeslot GetTimeslot(long scheduleID, long timeslotID) { return scheduleDict[scheduleID].GetTimeslot(timeslotID); }
		
		public static void EditTimeslot(Timeslot timeslot, DateTime startTime, DateTime endTime, string name)
		{
			timeslot.SetStartTime((DateTimeOffset)startTime.AddDays(-1));
			timeslot.SetEndTime(endTime > startTime ? (DateTimeOffset)endTime.AddDays(-1) : (DateTimeOffset)startTime.AddDays(-1));
			timeslot.SetName(name);
		}
		
		public static void PopulateTimeslotDict()
		{
			timeslotDict = new Dictionary<DateTime, List<Timeslot>>();
			
			foreach(Schedule schedule in scheduleDict.Values)
			{
				if(!schedule.GetShow())
					continue;
				
				Dictionary<DateTime, List<Timeslot>> newTimeslots = schedule.GetTimeslotsInRange(new DateTimeOffset(listStartDate), new DateTimeOffset(listStartDate).AddDays(42));
				
				foreach(DateTime key in newTimeslots.Keys)
				{
					if(!timeslotDict.ContainsKey(key))
						timeslotDict.Add(key, new List<Timeslot>());
					
					timeslotDict[key].AddRange(newTimeslots[key]);
				}
			}
		}
		
		public static void RenameSchedule(long id, string name)
		{
			Schedule schedule = GetSchedule(id);
			PlannerIO.DeleteSchedule(schedule);
			
			schedule.SetName(name);
			schedule.SetFileName(PlannerIO.GenerateScheduleFileName(name));
		}
		
		public static Schedule CreateNewSchedule(string name = null)
		{
			Schedule schedule = new Schedule(name);
			
			scheduleDict.Add(schedule.GetID(), schedule);
			PlannerIO.Save(schedule);
			
			return schedule;
		}
		
		public static void DeleteSchedule(long id)
		{
			Schedule schedule = scheduleDict[id];
			
			PlannerIO.DeleteSchedule(schedule);
			scheduleDict.Remove(id);
			
			if(!(selectedSchedule is null) && selectedSchedule.GetID() == id)
				DeselectSchedule();
			
			if(!schedule.GetShow())
				return;
			PopulateTimeslotDict();
		}
		
		public static void DeleteTimeslot(long scheduleID, long timeslotID)
		{
			scheduleDict[scheduleID].Remove(timeslotID);
			PopulateTimeslotDict();
		}
		
		public static void DeleteTimeslot(Timeslot timeslot)
		{
			DeleteTimeslot(timeslot.GetScheduleID(), timeslot.GetID());
		}
		
		public static void SetScheduleShow(long id, bool show)
		{
			GetSchedule(id).SetShow(show);
			PopulateTimeslotDict();
		}
		
		public static void SelectSchedule(long id)
		{
			selectedSchedule = GetSchedule(id);
		}
		
		public static void DeselectSchedule()
		{
			selectedSchedule = null;
		}
		
		public static int GetDayOfWeek(DateTime date)
		{
			switch(date.DayOfWeek)
			{
				case DayOfWeek.Monday:
					return 0;
				case DayOfWeek.Tuesday:
					return 1;
				case DayOfWeek.Wednesday:
					return 2;
				case DayOfWeek.Thursday:
					return 3;
				case DayOfWeek.Friday:
					return 4;
				case DayOfWeek.Saturday:
					return 5;
				default:
					return 6;
			}
		}
	}
}
