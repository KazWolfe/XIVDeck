![XIVDeck Icon](SDPlugin/assets/images/icon@2x.png)

# XIVDeck

[![Download count](https://img.shields.io/endpoint?url=https://vz32sgcoal.execute-api.us-east-1.amazonaws.com/XIVDeck.FFXIVPlugin)](https://github.com/KazWolfe/XIVDeck)
[![Crowdin](https://badges.crowdin.net/xivdeck/localized.svg)](https://crowdin.com/project/xivdeck)
[![Latest Release](https://img.shields.io/github/v/release/KazWolfe/XIVDeck)](https://github.com/KazWolfe/XIVDeck/releases/latest)
[![Testing Release](https://img.shields.io/github/v/release/KazWolfe/XIVDeck?color=orange&include_prereleases&label=testing)](https://github.com/KazWolfe/XIVDeck/releases)


XIVDeck is a project that attempts to bridge the gap between [Final Fantasy XIV][ffxiv] 
and the [Elgato Stream Deck][streamdeck]. XIVDeck uses the Dalamud plugin library to 
create an interactive and pleasant human interface experience.

> ⚠️ **Hic Svnt Leones!**
> 
> This plugin, alongside all of Dalamud, is against the Terms of Service for Final Fantasy XIV.
> While this plugin has gone to great lengths to ensure that it is as safe as possible and adheres
> to the spirit of the game, there is still a small risk. Please ensure you're only using official
> builds and releases of all relevant tools.
> 
> By using this plugin (and Dalamud itself), you understand that you are risking receiving a
> ban for client modifications. For more information, please see [Dalamud's FAQ][dalamudfaq-tos].

## Plugin Components

This plugin has two primary components:

* The [XIVDeck Game Plugin](FFXIVPlugin), which is a Dalamud plugin responsible for interacting
with Final Fantasy XIV and hosting a local webserver that allows other systems to communicate 
with the game.
* The [XIVDeck Stream Deck Plugin](SDPlugin), which is a JavaScript plugin written using the 
Elgato Stream Deck SDK. It exposes a number of actions that will allow the game to deeply 
interact with the Stream Deck.

For those using hardware other than the Elgato Stream Deck, community-made plugins are available:

* [Loupedeck/Razer Stream Controller Plugin](https://github.com/bendobos/LoupeXIVDeck) by bendobos

Please note that the above plugins are _not_ endorsed and may be missing features or otherwise
have problems. Please contact the relevant maintainer if there are issues using non-Stream Deck
plugins. If you are a maintainer of a plugin using my API, please reach out via Discord!

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
* **Change Volume**: This action allows you to quickly mute or unmute independent audio channels in
game at any time. Stream Deck + users can additionally use Dials to adjust volume more precisely.

More action types may come later, depending on user demand, what can actually go on a hotbar, and what
Dalamud ultimately supports or makes accessible. For actions that either cannot be put on a hotbar 
or are not supported, the **Text Command** action will allow calling a command directly.

### Installing the Plugin

The XIVDeck plugins are available for download from [this repository's Releases page][releases].

To install the Stream Deck plugin, simply open the `XIVDeck.streamDeckPlugin` file. The Elgato 
Stream Deck software will take care of all installation steps.

To install the FFXIV plugin, simply add it through the Dalamud Plugin Installer. To opt in to
testing versions (when available), right-click XIVDeck's entry in the Plugin Installer and select
`Receive plugin testing versions`. 

### Getting Help

Technical support for release and testing versions of XIVDeck is available in the
[Goat Place Discord][goatplace]'s [XIVDeck help thread][support-thread]. 

Please report bugs and submit feature requests through GitHub Issues when possible. I also will
accept bugs and feature requests on Discord, but may ask you to open an Issue anyways. 

I cannot offer support for self-built versions of XIVDeck, nor versions installed from unofficial
repositories. Please ensure that you are using the official release(s) of all relevant tools.

### Building the Plugin

If for some reason you'd rather be on the bleeding edge, you may also manually build the plugins.

The XIVDeck Game Plugin is more or less self-contained and only needs to go through your IDE's
normal build processes. Build the plugin normally and add the output folder as a dev plugin 
path in Dalamud's settings.

The XIVDeck Stream Deck Plugin can be built by running `yarn build`. Symlink or copy the 
resulting`dist/dev.wolf.xivdeck.sdPlugin` folder to 
`%APPDATA%\Elgato\StreamDeck\Plugins\dev.wolf.xivdeck.sdPlugin`. Note that after creating this 
link, you need to *fully restart* the Stream Deck software for the plugin to be detected.

[ffxiv]: https://www.finalfantasyxiv.com
[streamdeck]: https://www.elgato.com/en/stream-deck
[releases]: https://github.com/KazWolfe/XIVDeck/releases
[dalamudfaq-test]: https://goatcorp.github.io/faq/dalamud_troubleshooting.html#q-how-do-i-enable-plugin-test-builds
[dalamudfaq-tos]: https://goatcorp.github.io/faq/xl_troubleshooting#q-are-xivlauncher-dalamud-and-dalamud-plugins-safe-to-use
[goatplace]: https://discord.gg/holdshift
[support-thread]: https://discord.com/channels/581875019861328007/1019648519226806323