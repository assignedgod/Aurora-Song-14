<div class="header" align="center">
<img alt="Frontier Station" height="300" src="https://github.com/AuroraSong14/Aurora-Song-14/blob/master/Resources/Textures/_AS/Logo/logo.png" />
</div>

Aurora Song is a fork of [Space Station 14](https://github.com/space-wizards/space-station-14) that runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox) engine written in C#.

This is the primary repo for Aurora Song.

If you want to host or create content for Aurora Song, this is the repo you need. It contains both RobustToolbox and the content pack for development of new content packs.

## Links

<div class="header" align="center">

[Discord](https://discord.gg/zUXmPrwbbM) | [Steam](https://store.steampowered.com/app/1255460/Space_Station_14/) | [Wiki](https://wiki.aurorasong.net/index.php)

</div>

## Documentation/Wiki

Our [wiki](https://wiki.aurorasong.net/index.php) has documentation on Aurora Song's content.

## Contributing

We are happy to accept contributions from anybody. Get in Discord if you want to help, and don't be afraid to ask for help of your own!

If you make any contributions, note that any changes made to files belonging to our upstream should be properly marked with comments (see the "Changes to upstream files" section in [CONTRIBUTING.md](https://github.com/AuroraSong14/Aurora-Song-14/blob/master/CONTRIBUTING.md)).

## Building

1. Clone this repo:
```shell
git clone https://github.com/AuroraSong14/Aurora-Song-14.git
```
2. Go to the project folder and run `RUN_THIS.py` to initialize the submodules and load the engine:
```shell
cd Aurora-Song-14
python RUN_THIS.py
```
3. Compile the solution:

Build the server using `dotnet build`.

[More detailed instructions on building the project.](https://docs.spacestation14.com/en/general-development/setup.html)

## License

Read [LEGAL.md](https://github.com/AuroraSong14/Aurora-Song-14/blob/master/LEGAL.md) for legal information regarding code licensing, including a table of attributions for each namespace within the codebase.

Most assets are licensed under CC-BY-SA 3.0 unless stated otherwise. Assets have their license and the copyright in the metadata file. Example.

Code taken from Emberfall was specifically relicensed under MIT terms with [permission from MilonPL](https://github.com/new-frontiers-14/frontier-station-14/pull/3607)

[2fca06eaba205ae6fe3aceb8ae2a0594f0effee0](https://github.com/new-frontiers-14/frontier-station-14/commit/2fca06eaba205ae6fe3aceb8ae2a0594f0effee0) was pushed on July 1, 2024 at 16:04 UTC

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and copyright specified in the metadata file. For example, see the [metadata for a crowbar](https://github.com/new-frontiers-14/frontier-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.

## Attributions

When we pull content from other forks, we organize their content to repo-specific subfolders to better track attribution and limit merge conflicts.

Content under these subdirectories originate from their respective forks and may contain modifications. These modifications are denoted by comments around the modified lines.

| Subdirectory | Fork Name | Fork Repository | License |
|--------------|-----------|-----------------|---------|
| `_AS` | Aurora Song | https://github.com/AuroraSong14/Aurora-Song-14/ | AGPL 3.0
| `_NF` | Frontier Station | https://github.com/new-frontiers-14/frontier-station-14 | AGPL 3.0 |
| `_CD` | Cosmatic Drift | https://github.com/cosmatic-drift-14/cosmatic-drift | MIT |
| `_Corvax` | Corvax | https://github.com/space-syndicate/space-station-14 | MIT |
| `_Corvax` | Corvax Frontier | https://github.com/Corvax-Frontier/Frontier | AGPL 3.0 |
| `_DV` | Delta-V | https://github.com/DeltaV-Station/Delta-v | AGPL 3.0 |
| `_EE` | Einstein Engines | https://github.com/Simple-Station/Einstein-Engines | AGPL 3.0 |
| `_Emberfall` | Emberfall | https://github.com/emberfall-14/emberfall | [MIT](https://github.com/new-frontiers-14/frontier-station-14/pull/3607) |
| `_EstacaoPirata` | Estacao Pirata | https://github.com/Day-OS/estacao-pirata-14 | AGPL 3.0 |
| `_Goobstation` | Goob Station | https://github.com/Goob-Station/Goob-Station | AGPL 3.0 |
| `_Impstation` | Impstation | https://github.com/impstation/imp-station-14 | AGPL 3.0 |
| `_NC14` | Nuclear 14 | https://github.com/Vault-Overseers/nuclear-14 | AGPL 3.0 |
| `Nyanotrasen` | Nyanotrasen | https://github.com/Nyanotrasen/Nyanotrasen | MIT |

Additional repos that we have ported features from without subdirectories are listed below.

| Fork Name | Fork Repository | License |
|-----------|-----------------|---------|
| Monolith | https://github.com/Monolith-Station/Monolith | AGPL 3.0 |
| Space Station 14 | https://github.com/space-wizards/space-station-14 | MIT |
| White Dream | https://github.com/WWhiteDreamProject/wwdpublic | AGPL 3.0 |
