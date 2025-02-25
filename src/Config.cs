using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Carbon;

[Serializable]
public class Config
{
	public static Config Singleton;

	public bool Enabled = false;
	public bool TrackCalls = false;
	public bool SourceViewer = false;
	public List<string> Assemblies = new();
	public List<string> Plugins = new();
	public List<string> Modules = new();
	public List<string> Extensions = new();
	public List<string> Harmony = new();

	public static string FilePath => Path.Combine(HarmonyLoader.modPath, "Carbon.Profiler.json");

	public static void Init()
	{
		if (Singleton != null)
		{
			return;
		}

		if (!File.Exists(FilePath))
		{
			Singleton = new();
			File.WriteAllText(FilePath, JsonConvert.SerializeObject(Singleton, Formatting.Indented));
			return;
		}

		Singleton = JsonConvert.DeserializeObject<Config>(File.ReadAllText(FilePath));
	}
}
