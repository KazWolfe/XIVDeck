![XIVDeck Icon](SDPlugin/assets/images/icon@2x.png)

# XIVDeck

XIVDeck is a project that attempts to bridge the gap between [Final Fantasy XIV][ffxiv] 
and the [Elgato Stream Deck][streamdeck]. XIVDeck uses the Dalamud plugin library to 
create an interactive and pleasant human interface experience.

## Plugin Components

This plugin has two primary components:

* The [XIVDeck Game Plugin](FFXIVPlugin), which is a Dalamud plugin responsible for interacting
with Final Fantasy XIV and hosting a local WebSocket server that allows other systems to
communicate with the game.
* The [XIVDeck Stream Deck Plugin](SDPlugin), which is a JavaScript plugin written
using the Elgato Stream Deck SDK. It exposes a number of actions that will allow the game to
deeply interact with the Stream Deck.

## Using the Plugin

The XIVDeck Game Plugin has very few configuration parameters; only really requiring a port
be chosen if (for whatever reason) TCP port 37984 is already used on your host.

The XIVDeck Stream Deck Plugin is a little bit more involved and does expose some extra options
for discerning players. Currently, the following command types can be placed on a Stream
Deck button:

* **Text Command**: This action allows you to send a slash command directly to the game upon 
pressing a button on your Stream Deck.
* **Execute Hotbar Slot**: This action allows you to trigger a specific hotbar slot at any
time, regardless of whether the hotbar is visible or not. The current icon present in that
hotbar slot will display on the Stream Deck.
* **Execute Action**: This (not at all confusingly named) action allows you to trigger a subset 
of actions that can normally be placed on a hotbar. Currently-supported actions are Collections,
Emotes, Extra Commands, Gearsets, General Actions, Performance Instruments, Macros, Main Commands,
Markers, Minions, Mounts, Fashion Accessories, and Waymarks.
* **Run In-Game Macro**: This action allows you to trigger any macro by ID number (zero-indexed) on
either the Individual or Shared tab of the Macro interface. *This feature does not allow you to
create external macros.*
* **Switch Class**: This action allows you to switch to a specific class directly. It will automatically
trigger the first gearset for each class that it finds in your active Gearset list.

More action types may come later, depending on user demand, what can actually go on a hotbar, and what
Dalamud ultimately supports or makes accessible. For actions that either cannot be put on a hotbar 
or are not supported, the **Text Command** action will allow calling a command directly.

### Installing the Plugin

The XIVDeck plugins are available for download from [this repository's Releases page][releases].

To install the Stream Deck plugin, simply open the `XIVDeck.streamDeckPlugin` file. The Elgato 
Stream Deck software will take care of all installation steps.

To install the FFXIV plugin, simply add it through the Dalamud Plugin Installer. Note that testing
versions are also available (and may at times be the only live version), but this requires you to 
[enable Dalamud Testing plugins][dalamudfaq-test] first.

### Building the Plugin

If for some reason you'd rather be on the bleeding edge, you may also manually build the plugins.
Note that support is *not* provided for self-built version of the plugin.

The XIVDeck Game Plugin is more or less self-contained and only needs to go through your IDE's
normal build processes. Currently, the only version that's guaranteed to build (and work) is
the Debug build. Place the output files (or symlink them) to 
`%APPDATA%\XIVLauncher\devPlugins\XIVDeck` to get started.

The XIVDeck Stream Deck Plugin can be installed by copying or symlinking the 
`StreamDeckPlugin` folder to `%APPDATA%\Elgato\StreamDeck\Plugins\dev.wolf.xivdeck.sdPlugin`.
Note that after creating this link, you need to *fully restart* the Stream Deck software for
the plugin to be detected.

[ffxiv]: https://www.finalfantasyxiv.com
[streamdeck]: https://www.elgato.com/en/stream-deck
[releases]: https://github.com/KazWolfe/XIVDeck/releases
[dalamudfaq-test]: https://goatcorp.github.io/faq/dalamud_troubleshooting.html#q-how-do-i-enable-plugin-test-builds