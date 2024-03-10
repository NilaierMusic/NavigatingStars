# Navigating Stars
![](https://t.ly/fljJF)
Navigating Stars is a client-side mod for the game Lethal Company. This mod allows players to draw a green line from their current position to the main entrance of the map by pressing the 'T' key on the keyboard.

## Features

- Detects when the 'T' key is pressed on the keyboard.
- Draws a green line from the player's current position to the main entrance when the key is pressed.
- Provides configuration options for customizing your experience.
- Clears the line when the 'T' key is pressed again or when a new map is loaded.

## Installation

1. Make sure you have BepInEx installed for Lethal Company.
2. Download the latest release of the Navigating Stars mod from the [Thunderstore,](https://thunderstore.io/c/lethal-company/p/Nilaier/NavigatingStars/) NexusMods, or [Releases page](https://github.com/NilaierMusic/NavigatingStars/releases).
3. Extract the contents of the downloaded ZIP file into the `BepInEx/plugins` directory of your Lethal Company installation.
4. Launch the game and enjoy the mod!

## Configuration

The mod provides the following configuration options:

- `ToggleKey`: The key to press for drawing the line (default: 'T').
- `LineWidth`: The width of the drawn line as a float (default: 0.1).
- `LineColor`: The color of the drawn line in HEX (default: green).
- `AutoClearDistance`: The distance threshold for auto-clearing the line when the player is near the main entrance. (default: 5)
- `DynamicLineRedraw`: Determines whether the line should be redrawn dynamically based on the player's position.

You can modify these values in the configuration file located at `BepInEx/config/NavigatingStars.cfg`.

## Usage

1. Launch Lethal Company with the Navigating Stars mod installed.
2. During gameplay, press the configured toggle key (default: 'T') to draw the line from your current position to the main entrance.
3. A brief animation of a line being drawn will play, indicating the navigable path.
4. Press the toggle key again to clear the line.
5. The line will also be cleared automatically when a new map is loaded or you get close to the main entrance.

## Known Issues

- Pathfinding might break if there are any closed doors on the way to the main entrance.
- Sometimes the line would go through objects (scrap, pipes, hills, corners, carpets, ladders, etc.).
- The line might not choose the fastest paths.
- The line might draw itself over some inaccessible paths (sharp slopes that players can't climb).
- The line might not be seen from some angles due to it being too flat and bending in weird ways.
- Sometimes, the line doesn't draw a path.
- **If you encounter any more issues, please report them on the [issues page](https://github.com/NilaierMusic/NavigatingStars/issues).**

## Planned Features

- Different lines that would point to different things.
- Directional sound effect for the line being drawn, so you can know where to look.
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