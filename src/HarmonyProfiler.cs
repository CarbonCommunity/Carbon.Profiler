using System;
using System.IO;
using System.Runtime.InteropServices;
using Carbon.Components;
using Carbon.Profiler;
using UnityEngine;

namespace Carbon;

public sealed class HarmonyProfiler : IHarmonyModHooks
{
	public static readonly string configPath = Path.Combine(HarmonyLoader.modPath, "config.profiler.json");

	public void OnLoaded(OnHarmonyModLoadedArgs args)
	{
		MonoProfilerConfig.Load(configPath);
		InitNative();

		if (MonoProfiler.Crashed)
		{
			Debug.LogError($"Carbon.Profiler crashed! (NATIVE_PROTOCOL:{MonoProfiler.NATIVE_PROTOCOL} MANAGED_PROTOCOL:{MonoProfiler.MANAGED_PROTOCOL})");
		}
		else
		{
			Debug.Log($"Carbon.Profiler initialized! (NATIVE_PROTOCOL:{MonoProfiler.NATIVE_PROTOCOL} MANAGED_PROTOCOL:{MonoProfiler.MANAGED_PROTOCOL})");
		}
	}

	public void OnUnloaded(OnHarmonyModUnloadedArgs args)
	{
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
}
