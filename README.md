![Carbon Light Logo](https://raw.githubusercontent.com/CarbonCommunity/.github/refs/heads/main/profile/press/carbonlogo_w.png#gh-dark-mode-only)
![Carbon Dark Logo](https://raw.githubusercontent.com/CarbonCommunity/.github/refs/heads/main/profile/press/carbonlogo_b.png#gh-light-mode-only)

<p align="center">
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
- `carbon.export_profile( -c=CSV, -j=JSON, -t=Table, -p=ProtoBuf [default] )` Exports to disk the most recent profile
