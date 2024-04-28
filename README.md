# Navigating Stars
![](https://i.imgur.com/AxeJT4T.png)
Navigating Stars is a client-side mod for the game Lethal Company. This mod allows players to draw a green line from their current position to the main entrance of the map by pressing the 'C' key on the keyboard.

## Features

- **Key Detection**: Detects when the configured toggle key (default: 'C') is pressed to initiate line drawing.
- **Dynamic Drawing**: Draws a line from the player's current position to the main entrance, which can be dynamically redrawn based on the player's movements if enabled.
- **Customizable Visuals and Audio**:
  - Line color and width can be customized.
  - Sound effects for drawing and clearing the line can be toggled and their volume adjusted.
  - Option to enable 3D sound for enhanced spatial audio effects.
- **Auto-Clear Mechanism**: The line is automatically cleared when the player is within a specified distance from the main entrance or when a new map is loaded.
- **Toggle Feature**: Pressing the toggle key again will clear the currently drawn line.

## Installation

1. Make sure you have BepInEx installed for Lethal Company.
2. Download the latest release of the Navigating Stars mod from the [Thunderstore,](https://thunderstore.io/c/lethal-company/p/Nilaier/NavigatingStars/) [NexusMods,](https://www.nexusmods.com/lethalcompany/mods/199/) or [Releases page.](https://github.com/NilaierMusic/NavigatingStars/releases)
3. Extract the contents of the downloaded ZIP file into the `BepInEx/plugins` directory of your Lethal Company installation.
4. Launch the game and enjoy the mod!

### Configuration

The mod provides several configuration options to customize the user experience. These settings can be adjusted in the `BepInEx/config/NavigatingStars.cfg` file:

- `ToggleKey`: Key used to toggle the drawing of the line (default: `C`).
- `LineWidth`: Width of the drawn line (default: 0.1). This is a float value that determines the thickness of the line.
- `LineColor`: Color of the line. The color is defined in Unity's `Color` format (default: `Color.green`).
- `AutoClearDistance`: Distance threshold (in meters) for automatically clearing the line when approaching the main entrance (default: 2.5).
- `DynamicLineRedraw`: Boolean value that determines whether the line should be redrawn dynamically as the player moves (default: true).
- `EnableSoundEffects`: Enables or disables sound effects associated with drawing and clearing the line (default: false).
- `SoundEffectVolume`: Volume of the sound effects, ranging from 0.0 (silent) to 1.0 (full volume) (default: 1.0).
- `Enable3DSound`: Determines whether sound effects should utilize 3D spatial audio for a more immersive experience (default: true).

### Usage

1. **Start the Game**: Launch Lethal Company with the Navigating Stars mod installed.
2. **Activate Line Drawing**: During gameplay, press the configured toggle key (`C` by default) to draw a navigable line from your current position to the main entrance.
3. **View the Line**: Observe a brief animation that demonstrates the path to follow. The line's appearance (color and width) can be adjusted via the mod's configuration file.
4. **Toggle the Line**: Press the toggle key again to make the line disappear.
5. **Automatic Clearing**: The line will also vanish automatically once you're within the auto-clear distance from the main entrance or when transitioning to a new map.

## Incompatible Modded Moons:
- Chiron TL-34
- Circus
- [Clock Town](https://thunderstore.io/c/lethal-company/p/Dafini/Zelda_Moons/)
- [Crypt](https://thunderstore.io/c/lethal-company/p/Olemyth/Crypt/)
- Dragon Roost Island
- [G7 Island](https://thunderstore.io/c/lethal-company/p/G7Exid/G7_Island/)
- [Gm_Construct](https://thunderstore.io/c/lethal-company/p/babyherc/Gm_Construct/)
- [H410 Array](https://thunderstore.io/c/lethal-company/p/SpookyFingas/ArrayMoon/)
- [Halo](https://thunderstore.io/c/lethal-company/p/Ryudious/Halo/)
- [Harloth](https://thunderstore.io/c/lethal-company/p/Tolian/Harloth/)
- [Kamino](https://thunderstore.io/c/lethal-company/p/Puremask/KaminoMoon/)
- [Lidl](https://thunderstore.io/c/lethal-company/p/Passiert/Lidl_Moon/)
- [Los Santos](https://thunderstore.io/c/lethal-company/p/G7Exid/Los_Santos/)
- Luke 78
- [Main Street (South Park)](https://thunderstore.io/c/lethal-company/p/G7Exid/SouthParkCompany/)
- Mayhem Temple
- [Nostromo](https://thunderstore.io/c/lethal-company/p/Pareware/Nostromo/)
- [Nutandila](https://thunderstore.io/c/lethal-company/p/ZaphonGaming/Nutandila_MOON/)
- [Peach's Castle](https://thunderstore.io/c/lethal-company/p/TeamBridget/Peaches_Castle/)
- [PengCity](https://thunderstore.io/c/lethal-company/p/Apeng/PengCity/)
- [RMS Titanic](https://thunderstore.io/c/lethal-company/p/G7Exid/RMS_Titanic/)
- [SPES (South Park Elementary)](https://thunderstore.io/c/lethal-company/p/G7Exid/SouthParkCompany/)
- [Sargus-4](https://thunderstore.io/c/lethal-company/p/G7Exid/Sargus/)
- [Spacestation](https://thunderstore.io/c/lethal-company/p/sfDesat/Spacestation/)
- [The Moon](https://thunderstore.io/c/lethal-company/p/SpazzJr/LunarMoon/)
- [[SCRAPED] Wasteland](https://thunderstore.io/c/lethal-company/p/SpookyFingas/WastelandMoon/)

## Known Issues

- Pathfinding might break if there are any closed doors on the way to the main entrance.
- Sometimes the line would go through objects (scrap, pipes, hills, corners, carpets, ladders, etc.).
- The line might not choose the fastest paths.
- The line might draw itself over some inaccessible paths (sharp slopes that players can't climb).
- The line might not be seen from some angles due to it being too flat and bending in weird ways.
- Sometimes, the line doesn't draw a path.
- Pathfinding breaks on the railings and shelves.
- **If you encounter any more issues, please report them on the [issues page](https://github.com/NilaierMusic/NavigatingStars/issues).**

## Planned Features

- Different lines that would point to different things.
- The Stanley Parable Adventure Line™ skin.
- Compass HUD that would, instead, point to a direction.
- Pretty effects.

## Contributing

Contributions to the Navigating Stars mod are welcome! If you have any ideas, bug fixes, or improvements, please submit a [pull request](https://github.com/NilaierMusic/NavigatingStars/pulls) or [open an issue](https://github.com/NilaierMusic/NavigatingStars/issues) on the [GitHub repository](https://github.com/NilaierMusic/NavigatingStars).

## License

This mod is released under the [GNU General Public License version 3](https://opensource.org/license/gpl-3-0). See the `LICENSE` file for more details.

## Credits

- Developed by Nilaier
- Special thanks to the Lethal Company modding community.

## Contact

If you have any questions, suggestions, or feedback, you can reach out to me via:

- Discord: nilaier
- GitHub: [NilaierMusic](https://github.com/NilaierMusic)

Enjoy the Navigating Stars mod and have fun playing Lethal Company!