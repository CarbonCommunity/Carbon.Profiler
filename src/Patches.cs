using HarmonyLib;

namespace Carbon;

public class Patches
{
	[HarmonyPatch(typeof(Bootstrap), nameof(Bootstrap.Init_Tier0))]
	public static class Bootstrap_Init_Tier0
	{
		public static void Postfix()
		{
			if (HarmonyProfiler.IsCarbonInstalled)
			{
				return;
			}
			HarmonyProfiler.InstallCommands();
		}
	}
}
