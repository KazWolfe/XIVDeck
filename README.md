# XIVDeck

XIVDeck is a project that attempts to bridge the gap between 
[Final Fantasy XIV](https://www.finalfantasyxiv.com/) and the 
[Elgato Stream Deck](https://www.elgato.com/en/stream-deck). XIVDeck uses the Dalamud plugin
library to create an interactive and pleasant human interface experience.

> **Caution:** This plugin, in its current state, is nowhere near done. While certain core
> functionality *appears* to work, there are currently very few safety checks and even fewer
> considerations to "doing things right." While this plugin can be compiled and used, extreme
> caution is recommended until a full release has been published.
> 
> By using this plugin (especially in its current state) you understand that you are at risk
> of being banned from the game by Square Enix, or worse. I am not responsible if your Miqo'te
> turns into a Lalafell through use of this plugin.

## Plugin Components

This plugin has two primary components:

* The [XIVDeck Game Plugin](FFXIVPlugin), which is a Dalamud plugin responsible for interacting
with Final Fantasy XIV and hosting a local WebSocket server that allows other systems to
communicate with the game.
* The [XIVDeck Stream Deck Plugin](StreamDeckPlugin), which is a JavaScript plugin written
using the Elgato Stream Deck SDK. It exposes a number of actions that will allow the game to
deeply interact with the Stream Deck.

## Using the Plugin

The XIVDeck Game Plugin has very few configuration parameters; only really requiring a port
be chosen if (for whatever reason) TCP port 37984 is already used on your host.

The XIVDeck Stream Deck Plugin is a little bit more involved and does expose some extra options
for discerning players. Currently, the following command types can be placed on a Stream
Deck button:

* **Text Command / Message**: This action allows you to send a chat message (or slash
command) directly to the game upon pressing a button on your Stream Deck. Note that only
single-line messages can be used, and anything after a newline is ignored by the game.
* **Execute Hotbar Slot**: This action allows you to trigger a specific hotbar slot at any
time, regardless of whether the hotbar is visible or not. The current icon present in that
hotbar slot will display on the Stream Deck.
* **Execute Action**: This (not at all confusingly named) action allows you to trigger a limited
subset of actions that can normally be placed on a hotbar. Currently-supported actions are Minions,
Emotes, Mounts, General Actions, Main Commands, Waymarks, Markers, and Gear Sets.
* **Run In-Game Macro**: This action allows you to trigger any macro by ID number (zero-indexed) on
either the Individual or Shared tab of the Macro interface. *This feature does not allow you to
create external macros.*
* **Switch Class**: This action allows you to switch to a specific class directly. It will automatically
trigger the first gearset for each class that it finds in your active Gearset list.

More action types may come later, depending on user demand, what can actually go on a hotbar, and what
Dalamud ultimately supports or makes easily accessible. For actions that either cannot be put on a
hotbar or are not supported, the **Text Command** action will allow calling a command directly.

### Installing the Plugin

The XIVDeck plugins are available for download from [this repository's Releases 
page](https://github.com/KazWolfe/xivdeck/releases).

To install the Stream Deck plugin, simply open the `XIVDeck-StreamDeckPlugin.streamDeckPlugin`
file. The Elgato Stream Deck software will take care of all installation steps.

To install the FFXIV plugin, extract `XIVDeck-FFXIVPlugin.zip` to 
`%APPDATA%\XIVLauncher\devPlugins\XIVDeck`. If this folder does not already exist, please create it.

### Building the Plugin

If all this sounds like a good idea to you and the big scary warning at the top of this README
did not do enough to get you to not want to use it just yet, you can build and use this plugin
right now.

The XIVDeck Game Plugin is more or less self-contained and only needs to go through your IDE's
normal build processes. Currently, the only version that's guaranteed to build (and work) is
the Debug build. Place the output files (or symlink them) to 
`%APPDATA%\XIVLauncher\devPlugins\XIVDeck` to get started.

The XIVDeck Stream Deck Plugin can be installed by copying or symlinking the 
`StreamDeckPlugin` folder to `%APPDATA%\Elgato\StreamDeck\Plugins\dev.wolf.xivdeck.sdPlugin`.
Note that after creating this link, you need to *fully restart* the Stream Deck software for
the plugin to be detected.
