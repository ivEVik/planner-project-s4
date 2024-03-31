using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;


namespace Planner
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class JsonIncludePrivateFieldsAttribute : Attribute { }
	
	static class PlannerIO
	{
		private static ResourceManager imageResourceManager;
		private static ResourceManager textResourceManager;
		
		private const string configPath = "./config.json";
		private static string savedSchedulePath;
		private static JsonSerializerOptions jsonOptions;
		
		public static void Init()
		{
			imageResourceManager = new ResourceManager("Planner.Resources.Images", typeof(Program).Assembly);
			textResourceManager = new ResourceManager("Planner.Resources.UIText", typeof(Program).Assembly);
			
			savedSchedulePath = "./data/schedules/";
			
			jsonOptions = new JsonSerializerOptions {
				//IncludeFields = true,
				TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { AddPrivateFieldsModifier } }
			};
			
			ValidateSavedSchedulePath();
		}
		
		public static string GetResourceString(string resourceName)
		{
			return (string)textResourceManager.GetObject(resourceName);
		}
		
		public static Icon GetResourceIcon(string resourceName)
		{
			return (Icon)imageResourceManager.GetObject(resourceName);
		}
		
		public static Image GetResourceImage(string resourceName)
		{
			return (Image)imageResourceManager.GetObject(resourceName);
		}
		
		public static PlannerConfig LoadConfig()
		{
			if(!File.Exists(configPath))
				SaveConfig(new PlannerConfig());
			return JsonSerializer.Deserialize<PlannerConfig>(File.ReadAllText(configPath), jsonOptions);
		}
		
		public static void SaveAllSchedules()
		{
			foreach(Schedule schedule in Program.GetSchedules())
				Save(schedule);
		}
		
		public static Dictionary<long, Schedule> LoadAllSchedules()
		{
			string[] filePaths = Directory.GetFiles(savedSchedulePath, "*.json");
			Dictionary<long, Schedule> result = new Dictionary<long, Schedule>();
			
			foreach(string path in filePaths)
			{
				Schedule schedule = new Schedule();
				if(TryOpen(path, ref schedule))
					result.Add(schedule.GetID(), schedule);
			}
			
			return result;
		}
		
		public static void SaveConfig(PlannerConfig config)
		{
			File.WriteAllText(configPath, JsonSerializer.Serialize(config, jsonOptions));
		}
		
		public static void Save(Schedule schedule)
		{
			ValidateSavedSchedulePath();
			
			File.WriteAllText(savedSchedulePath + schedule.GetFileName(), JsonSerializer.Serialize(schedule, jsonOptions));
		}
		
		public static bool TryOpen(string filePath, ref Schedule schedule)
		{
			if(!File.Exists(filePath) || filePath.Length < 5 || filePath.Substring(filePath.Length - 5) != ".json")
				return false;
			
			schedule = JsonSerializer.Deserialize<Schedule>(File.ReadAllText(filePath), jsonOptions);
			
			return !(schedule is null) && schedule.Validate();
		}
		
		public static bool TryLoad(string fileName, ref Schedule schedule)
		{
			bool result = TryOpen(savedSchedulePath + fileName, ref schedule);
			
			if(fileName != schedule.GetFileName())
			{
				File.Delete(fileName);
				Save(schedule);
			}
			
			return result;
		}
		
		public static void DeleteSchedule(Schedule schedule)
		{
			string path = savedSchedulePath + schedule.GetFileName();
			if(File.Exists(path))
				File.Delete(path);
		}
		
		public static string GenerateScheduleFileName(string name)
		{
			string fileName = name + ".json";
			int t = 0;
			
			while(true)
			{
				if(!File.Exists(savedSchedulePath + fileName))
					return fileName;
				
				fileName = name + "-" + (t++).ToString() + ".json";
			}
		}
		
		public static bool IsScheduleFileNameAvailable(string fileName)
		{
			return !File.Exists(savedSchedulePath + fileName);
		}
		
		private static void AddPrivateFieldsModifier(JsonTypeInfo jsonTypeInfo)
		{
			if(jsonTypeInfo.Kind != JsonTypeInfoKind.Object || !jsonTypeInfo.Type.IsDefined(typeof(JsonIncludePrivateFieldsAttribute), inherit: false))
				return;
			
			foreach(FieldInfo field in jsonTypeInfo.Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				if(Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
					continue;
				
				JsonPropertyInfo jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);
				jsonPropertyInfo.Get = field.GetValue;
				jsonPropertyInfo.Set = field.SetValue;
				
				jsonTypeInfo.Properties.Add(jsonPropertyInfo);
			}
		}
		
		private static void ValidateSavedSchedulePath()
		{
			if(!Directory.Exists(savedSchedulePath))
				Directory.CreateDirectory(savedSchedulePath);
		}
	}
}
