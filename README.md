# Children Of Morta Archipelago Mod
This is an [Archipelago](https://archipelago.gg) implementation enabling Children of Morta to be played in the multiworld randomizer.

# What is Archipelago?
Many answers are [here](https://archipelago.gg/faq/en/) but in the nutshell: Items you normally collect are replaced with random items. 

# Installation Instructions
1. Install [BepInEx 5](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_mono.html) - download latest 5.X.XX version [here](https://github.com/BepInEx/BepInEx/releases) and drop into folder with game
2. Download latest version of .zip file from [release page](https://github.com/BocikPG/Children-Of-Morta-Archipelago-Randomizer-Mod/releases)
3. Unzip folder inside 'ChildrenOfMorta\BepInEx\plugins' so it becomes 'ChildrenOfMorta\BepInEx\plugins\ArchipelagoRandomizer\ArchipelagoRandomizer.dll' (and other files)
4. Setup archipelago multiworld:
- Download latest version of .apworld file from the same [release page earlier](https://github.com/BocikPG/Children-Of-Morta-Archipelago-Randomizer-Mod/releases)
- Double click the downloaded apworld to automatically install it to Archipelago, or manually move the file into the custom_worlds folder of your Archipelago installation
- For game setup read [Archipelago guide](https://archipelago.gg/tutorial/Archipelago/setup_en) on that topic
5. Once you have your server running you can run the game.
6. Inside the boxes in left-top corner you can enter link, player name and password (optionally)
- if you see the boxes that means you are not connected to the archipelago, if you can't that means you are connected.
- message log shows up in left-bottom corner on received message if connected. During character select or in game pause it's always visible.

# About Implementation
Currently ONLY Endless mode (Zyklus/Family trials) is supported. Things may behave unexpectedly if run in campaign mode. 
Endless mode is unlocked once the connection is made (may have to restart the game), if not unlocked already.

Co-op is currently not supported - solo play only.

# Goal
There are 2 goals currently you can choose from:
- Defeat end boss with every family member
- Defeat end boss specified number of times

# Locations / "Checks"
Locations (also known as "Checks" in Archipelago terminology) are specific points in a game that is considered something important, and it will release an associated item into the multiworld

For Morta it's Divine Relics, Talents and end boss kills. 

In options you can specify in what ratio you want to split items between Relics and Talents. 
# Items
Depending on options set you can decide what you can expect to receive from multiworld (from yourself, or other players):
- Characters
- Divine Relics
- Talents depending on type

Characters are always in. Only unlocked characters can check locations.
I recommend using all available items, but you can make game more challenging by skipping some.

# Options
## Deathlink
This is an opt-in system where when a player dies, all other players will die also.

# Known Issues
Game (client) can become unstable in later stages of the game - it can crash or stuck on the loding of the level.

If playing using gamepad locked characters can be seen as playable. If you don't remember what characters are unlocked you can try to restart the game.

# Found issue or have questions?
Use Issues tab above to check for your problem or create new one.
Alternatively you can join Archipelago Discord server, find future-game-designs forum and inside it Children Of Morta thread.

# AI Disclosure
I used AI to generate parts of apworld code.

All client mod code is handwritten tho. None of original creators' code was send to LLM's.

I did used it to search the web in both cases.
