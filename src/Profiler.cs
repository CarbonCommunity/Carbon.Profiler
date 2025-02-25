using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Carbon;

public sealed class Profiler : IHarmonyModHooks
{
	public void OnLoaded(OnHarmonyModLoadedArgs args)
	{
		Config.Init();
		InitNative();
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
        mono_dllmap_insert(ModuleHandle.EmptyHandle, "CarbonNative", null, Path.Combine(HarmonyLoader.modPath, "libCarbonNative.so"), null);
#elif WIN
		mono_dllmap_insert(ModuleHandle.EmptyHandle, "CarbonNative", null, Path.Combine(HarmonyLoader.modPath, "CarbonNative.dll"), null);
#endif

		var path = Config.FilePath;

		fixed (char* ptr = path)
		{
			init_profiler(ptr, path.Length);
		}
	}

	#endregion
}
