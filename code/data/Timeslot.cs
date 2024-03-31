using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;


namespace Planner
{
	[JsonIncludePrivateFields]
	public class Timeslot : IEquatable<Timeslot>
	{
		private static long nextID = long.MinValue;
		
		[JsonIgnore]
		private long id;
		[JsonIgnore]
		private long scheduleID;
		private string name;
		private DateTimeOffset startTime;
		private DateTimeOffset endTime;
		
		private bool repeat;
		private TimeSpan repeatInterval;
		private int repeatCount;
		
		private bool activated;
		
		
		public Timeslot(string name, DateTimeOffset startTime) : this(name, startTime, startTime, false) { }
		
		[JsonConstructor]
		private Timeslot(string name, DateTimeOffset startTime, DateTimeOffset endTime, bool activated = false)
		{
			this.id = GenerateID();
			this.name = name;
			this.startTime = startTime.ToUniversalTime();
			this.endTime = endTime.ToUniversalTime();
			this.activated = activated;
		}
		/*
		private DateTimeOffset GetStartTime()
		{
			return startTime;
		}
		
		private DateTimeOffset GetEndTime()
		{
			return endTime;
		}
		*/
		
		private void SetID(long id) { this.id = id; }
		public long GetID() { return id; }
		
		public void SetScheduleID(long scheduleID) { this.scheduleID = scheduleID; }
		public long GetScheduleID() { return this.scheduleID; }
		
		public void SetName(string name) { this.name = name; }
		public string GetName() { return name; }
		
		public void SetStartTime(DateTimeOffset startTime) { this.startTime = startTime.ToUniversalTime(); }
		public DateTimeOffset GetStartTime(TimeZoneInfo timeZone) { return TimeZoneInfo.ConvertTime(startTime, timeZone); }
		public DateTimeOffset GetStartTimeLocal() { return GetStartTime(TimeZoneInfo.Local); }
		public DateTimeOffset GetStartTimeGMT() { return startTime; }
		
		public void SetEndTime(DateTimeOffset endTime) { this.endTime = endTime.ToUniversalTime(); }
		public DateTimeOffset GetEndTime(TimeZoneInfo timeZone) { return TimeZoneInfo.ConvertTime(endTime, timeZone); }
		public DateTimeOffset GetEndTimeLocal() { return GetEndTime(TimeZoneInfo.Local); }
		public DateTimeOffset GetEndTimeGMT() { return endTime; }
		
		public void SetRepeat(bool repeat) { this.repeat = repeat; }
		public bool GetRepeat() { return repeat; }
		
		public void SetRepeatInterval(TimeSpan repeatInterval) { this.repeatInterval = repeatInterval; }
		public TimeSpan GetRepeatInterval() { return repeatInterval; }
		
		public void SetRepeatCount(int repeatCount) { this.repeatCount = repeatCount; }
		public int GetRepeatCount() { return repeatCount; }
		
		public DateTimeOffset GetFinalRepeatGMT() { return repeat && (repeatCount != -1) ? GetStartTimeGMT().Add(repeatInterval * repeatCount) : GetStartTimeGMT(); }
		
		
		public bool IsActive() { return DateTimeOffset.UtcNow > GetStartTimeGMT() && DateTimeOffset.UtcNow < GetEndTimeGMT(); }
		public bool IsNewlyActive()
		{
			if(!IsActive())
				return false;
			
			if(activated)
				return false;
			
			activated = true;
			return true;
		}
		
		
		public List<DateTime> GetRepeatsInRange(DateTimeOffset intervalStart, DateTimeOffset intervalEnd)
		{
			List<DateTime> result = new List<DateTime>();
			
			if(GetStartTimeGMT() > intervalEnd || GetFinalRepeatGMT() < intervalStart)
				return result;
			
			int elapsedRepeatCount = (int)((intervalStart - GetStartTimeGMT()).TotalSeconds) / (int)repeatInterval.TotalSeconds;
			
			for(DateTimeOffset time = GetStartTimeGMT().AddSeconds(elapsedRepeatCount * repeatInterval.TotalSeconds) + repeatInterval; time < intervalEnd; time = time + repeatInterval)
				result.Add(time.Date);
			
			return result;
		}
		
		public bool Validate()
		{
			return !(name is null);
		}
		
		
		public bool Equals(Timeslot timeslot)
		{
			if(timeslot is null)
				return false;
			
			return this.id == timeslot.id;
		}
		
		public override bool Equals(object obj)
		{
			if(obj is null)
				return false;
			
			return Equals(obj as Timeslot);
		}
		
		public override int GetHashCode()
		{
			return this.id.GetHashCode();
		}
		
		public static bool operator ==(Timeslot ts1, Timeslot ts2)
		{
			return ts1.Equals(ts2);
		}
		
		public static bool operator !=(Timeslot ts1, Timeslot ts2)
		{
			return !ts1.Equals(ts2);
		}
		
		
		private static long GenerateID()
		{
			return nextID++;
		}
	}
}
