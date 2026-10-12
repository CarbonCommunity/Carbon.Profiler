<p align="center">
  <img src="https://cdn.carbonmod.gg/carbon_profiler_sq.jpg" height="30%" width="30%" />
  <br /><br />
  <a href="https://github.com/CarbonCommunity/Carbon/releases/tag/profiler_build"><img src="https://github.com/CarbonCommunity/Carbon/actions/workflows/profiler-build.yml/badge.svg" /></a>
  <hr />
</p>

This is an out-of-the-box build of the Carbon.Profiler dedicated to be working on vanilla and/or Oxide servers.
Run `find carbon` upon server boot to get further command instructions.

# How to install
- Download the `Carbon.[Windows|Linux].Profiler` archive from the attachments below.
- Unzip the archive to the `HarmonyMods` directory of your Rust Dedicated Server.
- Start the server and enjoy, or run `harmony.load` Carbon.Profiler.

# Commands

- `carbon.profile( [duration] [-cm] [-am] [-t] [-c] [-gc] )` Toggles the current state of the Carbon.Profiler
- `carbon.abort_profile(  )` Stops a current profile from running
- `carbon.profile_mod( <name> <duration> [-cm] [-am] [-t] [-c] [-gc] )` Profiles one HarmonyMod: reloads it with profiler hooks, records for the duration, then reloads it without them
- `carbon.profile_plugin( <name> <duration> [-cm] [-am] [-t] [-c] [-gc] )` Profiles one Oxide plugin: recompiles it with profiler hooks, records for the duration, then recompiles it without them
- `carbon.export_profile( -c=CSV, -j=JSON, -t=Table, -p=ProtoBuf [default] )` Exports to disk the most recent profile
- `carbon.tracked(  )` All tracking lists present in the config which are used by the Mono profiler for tracking
- `carbon.track( [assembly|plugin|module|ext|harmony] [value] )` Adds an object to be tracked. Reload a tracked plugin or HarmonyMod to profile it. Assemblies, modules and extensions need Enabled in config.profiler.json and a server restart
- `carbon.untrack( [assembly|plugin|module|ext|harmony] [value] )` Removes an object from being tracked. Reload an untracked plugin or HarmonyMod to remove its profiler hooks. Assemblies, modules and extensions need a server restart
