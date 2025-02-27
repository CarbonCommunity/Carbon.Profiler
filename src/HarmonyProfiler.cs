using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Carbon.Components;
using Carbon.Profiler;
using Steamworks;
using UnityEngine;

namespace Carbon;

public sealed class HarmonyProfiler : IHarmonyModHooks
{
	public static readonly string configPath = Path.Combine(HarmonyLoader.modPath, "config.profiler.json");

	public static string profilesFolderPath
	{
		get
		{
			var path = Path.Combine(HarmonyLoader.modPath, "profiles");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			return path;
		}
	}

	public static bool IsCarbonInstalled = Type.GetType("Carbon.Community,Carbon.Common") != null;

	public static bool IsOxideInstalled = Type.GetType("Oxide.Core.Interface,Oxide.Core") != null;

	private static ProfilerRunner _runner;

	public static ProfilerRunner Runner => _runner ??= (_runner = new GameObject("Profiler Runner").AddComponent<ProfilerRunner>());

	public static MonoProfiler.Sample ProfileSample = MonoProfiler.Sample.Create();

	private static readonly List<ConsoleSystem.Command> commands = new();

	private static ConsoleSystem.Command[] originalCommands;

	public void OnLoaded(OnHarmonyModLoadedArgs args)
	{
		if (IsCarbonInstalled)
		{
			Debug.LogError($"Carbon is installed! Remove the Carbon.Profiler HarmonyMod since the profiler is already built in.");
			return;
		}

		MonoProfilerConfig.Load(configPath);
		InitNative();

		Debug.LogError($"Carbon.Profiler {(MonoProfiler.Crashed ? "crashed" : "initialized")}! (NATIVE_PROTOCOL:{MonoProfiler.NATIVE_PROTOCOL} MANAGED_PROTOCOL:{MonoProfiler.MANAGED_PROTOCOL})");

		if (SteamServer.IsValid)
		{
			Patches.Bootstrap_Init_Tier0.Postfix();
		}
	}

	public void OnUnloaded(OnHarmonyModUnloadedArgs args)
	{
		UninstallCommands();
	}

	public static void InstallCommands()
	{
		originalCommands ??= ConsoleSystem.Index.All;

		commands.Clear();
		commands.AddRange(originalCommands);
		AddCommand("carbon", "profile", arg =>
		{
			if (!MonoProfiler.Enabled)
			{
				arg.ReplyWith("Mono profiler is disabled. Enable it in the 'config.profiler.json' config file. Must restart the server for changes to apply.");
				return;
			}

			var duration = arg.GetFloat(0);
			var flags = MonoProfiler.ProfilerArgs.None;

			if (arg.HasArg("-cm")) flags |= MonoProfiler.ProfilerArgs.CallMemory;
			if (arg.HasArg("-am")) flags |= MonoProfiler.ProfilerArgs.AdvancedMemory;
			if (arg.HasArg("-t")) flags |= MonoProfiler.ProfilerArgs.Timings;
			if (arg.HasArg("-c")) flags |= MonoProfiler.ProfilerArgs.Calls;
			if (arg.HasArg("-gc")) flags |= MonoProfiler.ProfilerArgs.GCEvents;

			if (flags == MonoProfiler.ProfilerArgs.None) flags = MonoProfiler.AllFlags;

			if (MonoProfiler.IsRecording)
			{
				MonoProfiler.ToggleProfiling(flags);
				ProfileSample.Resample();
				MonoProfiler.Clear();
				return;
			}

			if (duration <= 0)
			{
				MonoProfiler.ToggleProfiling(flags);
			}
			else
			{
				MonoProfiler.ToggleProfilingTimed(duration, flags, args =>
				{
					ProfileSample.Resample();
					MonoProfiler.Clear();
				});
			}
		}, description: "Toggles the current state of the Carbon.Profiler", arguments: "[duration] [-cm] [-am] [-t] [-c] [-gc]");
		AddCommand("carbon", "abort_profile", arg =>
		{
			if (!MonoProfiler.IsRecording)
			{
				arg.ReplyWith("No profiling process active");
				return;
			}

			MonoProfiler.ToggleProfiling(MonoProfiler.ProfilerArgs.Abort);
			ProfileSample.Clear();
		}, description: "Stops a current profile from running");
		AddCommand("carbon", "export_profile", arg =>
		{
			if (MonoProfiler.IsRecording)
			{
				arg.ReplyWith("Profiler is actively recording");
				return;
			}

			var mode = arg.GetString(0);

			switch (mode)
			{
				case "-c":
					arg.ReplyWith(WriteFileString("csv", ProfileSample.ToCSV()));
					break;

				case "-j":
					arg.ReplyWith(WriteFileString("json", ProfileSample.ToJson(true)));
					break;

				case "-t":
					arg.ReplyWith(WriteFileString("txt", ProfileSample.ToTable()));
					break;

				default:
				case "-p":
					arg.ReplyWith(WriteFileBytes(MonoProfiler.ProfileExtension, ProfileSample.ToProto()));
					break;

			}

			static string WriteFileString(string extension, string data)
			{
				var date = DateTime.Now;
				var file = Path.Combine(profilesFolderPath, $"profile-{date.Year}_{date.Month}_{date.Day}_{date.Hour}{date.Minute}{date.Second}.{extension}");
				File.WriteAllText(file, data);

				return $"Exported profile output at '{file}'";
			}
			static string WriteFileBytes(string extension, byte[] data)
			{
				var date = DateTime.Now;
				var file = Path.Combine(profilesFolderPath, $"profile-{date.Year}_{date.Month}_{date.Day}_{date.Hour}{date.Minute}{date.Second}.{extension}");
				File.WriteAllBytes(file, data);

				return $"Exported profile output at '{file}'";
			}
		}, description: "Exports to disk the most recent profile", arguments: "-c=CSV, -j=JSON, -t=Table, -p=ProtoBuf [default]");
		ConsoleSystem.Index.All = commands.ToArray();

		static void AddCommand(string parent, string name, Action<ConsoleSystem.Arg> callback, string description = null, string arguments = null)
		{
			var command = new ConsoleSystem.Command
			{
				Name = name,
				Parent = parent,
				FullName = parent + "." + name,
				Call = callback,
				ServerAdmin = true,
				Description = description,
				Arguments = arguments
			};
			commands.Add(command);
			ConsoleSystem.Index.Server.Dict[command.FullName] = command;
			Debug.LogWarning($"Carbon.Profiler: Installed '{command.FullName}'");
		}
	}

	public static void UninstallCommands()
	{
		ConsoleSystem.Index.All = originalCommands;
	}

	#region Native MonoProfiler

	[DllImport("CarbonNative")]
	public static unsafe extern void init_profiler(char* ptr, int length);

	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	public static extern void mono_dllmap_insert(ModuleHandle assembly, string dll, string func, string tdll, string tfunc);

	public static unsafe void InitNative()
	{
#if UNIX
        mono_dllmap_insert(ModuleHandle.EmptyHandle, "CarbonNative", null, Path.Combine(HarmonyLoader.modPath, "native", "libCarbonNative.so"), null);
#elif WIN
		mono_dllmap_insert(ModuleHandle.EmptyHandle, "CarbonNative", null, Path.Combine(HarmonyLoader.modPath, "native", "CarbonNative.dll"), null);
#endif

		fixed (char* ptr = configPath)
		{
			init_profiler(ptr, configPath.Length);
		}
	}

	#endregion

	public class ProfilerRunner : FacepunchBehaviour;
}
