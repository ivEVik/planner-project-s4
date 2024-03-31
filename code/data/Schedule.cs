using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Planner
{
	[JsonIncludePrivateFields]
	public class Schedule : IEquatable<Schedule>
	{
		private static long nextID = long.MinValue;
		
		[JsonIgnore]
		private long id;
		private string name;
		private string fileName;
		
		private bool show;
		
		private List<Timeslot> timeslots;
		
		[JsonIgnore]
		private Dictionary<DateTime, List<Timeslot>> timeslotsByDate;
		
		[JsonIgnore]
		private Dictionary<long, Timeslot> timeslotsByID;
		
		[JsonIgnore]
		private List<Timeslot> repeatTimeslots;
		[JsonIgnore]
		private List<Timeslot> repeatTimeslotsElapsed;
		
		
		public Schedule()
		{
			this.id = GenerateID();
			this.name = null;
			this.timeslots = null;
		}
		
		public Schedule(string name) : this(name, new List<Timeslot>()) { }
		
		//[JsonConstructor]
		public Schedule(string name, List<Timeslot> timeslots, string fileName = null, bool show = true)
		{
			this.id = GenerateID();
			this.name = name;
			this.timeslots = timeslots;
			this.fileName = fileName is null ? PlannerIO.GenerateScheduleFileName(name) : fileName;
			this.show = show;
			
			InitSecondaries();
		}
		
		
		private void SetID(long id) { this.id = id; }
		public long GetID() { return this.id; }
		
		public void SetName(string name) { this.name = name; }
		public string GetName() { return this.name; }
		
		public void SetFileName(string fileName) { this.fileName = fileName; }
		public string GetFileName() { return this.fileName; }
		
		public void SetShow(bool show) { this.show = show; }
		public bool GetShow() { return this.show; }
		
		public Timeslot GetTimeslot(long timeslotID) { return timeslotsByID[timeslotID]; }
		
		
		public Dictionary<DateTime, List<Timeslot>> GetTimeslotsInRange(DateTimeOffset startTime, DateTimeOffset endTime)
		{
			startTime = startTime.ToUniversalTime();
			endTime = endTime.ToUniversalTime();
			
			DateTime endDate = endTime.Date;
			
			Dictionary<DateTime, List<Timeslot>> result = new Dictionary<DateTime, List<Timeslot>>();
			
			for(DateTime date = startTime.Date; date <= endDate; date = date.AddDays(1.0))
				if(timeslotsByDate.ContainsKey(date))
					result.Add(date, timeslotsByDate[date]);
			
			foreach(Timeslot t in repeatTimeslots)
				foreach(DateTime date in t.GetRepeatsInRange(startTime, endTime))
				{
					if(!result.ContainsKey(date))
						result.Add(date, new List<Timeslot>());
					
					result[date].Add(t);
				}
			
			if(startTime > DateTimeOffset.UtcNow)
				return result;
			
			foreach(Timeslot t in repeatTimeslotsElapsed)
				foreach(DateTime date in t.GetRepeatsInRange(startTime, endTime))
				{
					if(!result.ContainsKey(date))
						result.Add(date, new List<Timeslot>());
					
					result[date].Add(t);
				}
			
			return result;
		}
		
		public void Add(Timeslot timeslot)
		{
			timeslots.Add(timeslot);
			
			timeslot.SetScheduleID(id);
			
			AddToSecondaries(timeslot);
		}
		
		public void Remove(long timeslotID)
		{
			Timeslot timeslotToRemove = null;
			
			foreach(Timeslot timeslot in timeslots)
				if(timeslot.GetID() == timeslotID)
				{
					timeslotToRemove = timeslot;
					break;
				}
			
			if(timeslotToRemove is null)
				return;
			
			timeslots.Remove(timeslotToRemove);
			timeslotsByDate[timeslotToRemove.GetStartTimeGMT().Date].Remove(timeslotToRemove);
			timeslotsByID.Remove(timeslotID);
			
			if(!timeslotToRemove.GetRepeat())
				return;
			
			if(timeslotToRemove.GetFinalRepeatGMT() < DateTimeOffset.UtcNow)
			{
				repeatTimeslotsElapsed.Remove(timeslotToRemove);
				return;
			}
			
			repeatTimeslots.Remove(timeslotToRemove);
		}
		
		public bool Validate()
		{
			if(timeslots is null)
				return false;
			
			foreach(Timeslot t in timeslots)
			{
				if(!t.Validate())
					return false;
				t.SetScheduleID(id);
			}
			
			if(timeslotsByDate is null || repeatTimeslots is null || repeatTimeslotsElapsed is null)
				InitSecondaries();
			
			return true;
		}
		
		private void AddToSecondaries(Timeslot timeslot)
		{
			DateTime date = timeslot.GetStartTimeGMT().Date;
			
			if(!timeslotsByDate.ContainsKey(date))
				timeslotsByDate.Add(date, new List<Timeslot>());
			timeslotsByDate[date].Add(timeslot);
			
			if(timeslot.GetRepeat())
			{
				if(timeslot.GetRepeatCount() == -1 || timeslot.GetFinalRepeatGMT() > DateTimeOffset.UtcNow)
					repeatTimeslots.Add(timeslot);
				else
					repeatTimeslotsElapsed.Add(timeslot);
			}
			
			timeslotsByID.Add(timeslot.GetID(), timeslot);
		}
		
		private void InitSecondaries()
		{
			if(timeslots is null)
				return;
			
			timeslotsByDate = new Dictionary<DateTime, List<Timeslot>>();
			timeslotsByID = new Dictionary<long, Timeslot>();
			repeatTimeslots = new List<Timeslot>();
			repeatTimeslotsElapsed = new List<Timeslot>();
			
			foreach(Timeslot t in timeslots)
			{
				AddToSecondaries(t);
			}
		}
		
		private static long GenerateID()
		{
			return nextID++;
		}
		
		
		public bool Equals(Schedule schedule)
		{
			if(schedule is null)
				return false;
			
			return this.id == schedule.id;
		}
		
		public override bool Equals(object obj)
		{
			if(obj is null)
				return false;
			
			return Equals(obj as Schedule);
		}
		
		public override int GetHashCode()
		{
			return this.id.GetHashCode();
		}
		
		public static bool operator ==(Schedule sc1, Schedule sc2)
		{
			return sc1.Equals(sc2);
		}
		
		public static bool operator !=(Schedule sc1, Schedule sc2)
		{
			return !sc1.Equals(sc2);
		}
	}
}
