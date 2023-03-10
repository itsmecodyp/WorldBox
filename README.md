# WorldBox
My collection of BepInEx projects for WorldBox

Mod statuses
Active: Regularly being improved and worked on

Maintained: Only bug fixes and compatibility changes

Deprecated: Merged or replaced by separate project

Outdated: Old, probably broken, will update eventually

Abandoned: No intention to add or fix anything



CustomAssetLoader: Abandoned

Before NCMS, I needed a way to load custom assets easily without writing code for each one
This loaded pictures and text files containing data, then replaced or added everything ingame. I used it for buildings, actor textures and animations, and traits
Also had some integration with 3d, allowing assets loaded to have some custom data like rotation and height



CustomBlackJack: Active


Gambling mod to scratch the itch without spending money or playing something with unknown mechanics or algorithms in the background

Recreations of classic blackjack and some other random stuff



Formations: Deprecated


Standalone project to control units. Eventually was turned into a mechanic in other RPG mods



GeneralPurposeOnlineAddon: Abandoned

GeneralPurposeOnlineClientWorldBox: Abandoned

GeneralPurposeOnlineServer: Abandoned


A brotherhood that made up a basic networking mod that could be applied to any Unity game supporting BepInEx

The idea was to have 1 client-mod per game (ClientWorldBox for example) which handles connecting to and listening to a server

Then you could use the server mod to host and facilitate connections made from any number of games

I personally tested TABS, WorldBox, and Streets of Rogue, and the results were promising

Abandoned after Unity removed the networking system I was using



LuBu: Deprecated


First of the standalone RPG mods, this was a commission requested by a user of the same name in WorldBox discord

This was an amalgamation of previous mods, with improvements and customization based on the price paid

SimpleGUI had features for unit control which were eventually reused in other stuff, including this

Using the CustomAssetLoader, I added a few units and a set of buildings, all chinese-themed

LuBu and his horse had a mounting mechanic, which basically just combined them into a third unique unit




MapSizes: Active


Allows making maps of any size and with a bit more customization

Used to facilitate converting images into maps, but just a simple edit to something Maxim already made



PhotonMod: Abandoned (still works)


A test to see if PUN could be used in a mod. It can, but required some editing of the PUN code

Confirmed you can connect to rooms and communicate, but wasn't sure how to proceed from there

PUN commands are usually created in unity editor by placing attributes on methods

Dunno how to replicate that on the mod side



SimpleAI: Abandoned (still works)


Another test mod, using OpenAI and it's chat preset.

Could generate a "personality" to chat with using data the game generates and a few extra lists

Config option for OpenAI api key so users can input their own



SimpleAdditions: Outdated


Small project meant to be used as an example for adding various assets



SimpleGUI: Active


Basically a big toolbox, a collection of random features

Everything is contained in simple floating menus

Started as a "gotcha" to someone who said making a GUI would be hard

Became a medium to experiment and learn everything about WorldBox



SimpleLib: Active


A project meant to be used by other modders, but without much to offer

Has "actorSay" which allows displaying messages above units in WorldBox



SuperRPG: Active


The current standalone RPG that absorbed all the previous ones I made

Added manual construction, an inventory system and resource collection, and more

Has many extra assets that improve the added features, including 200 "buildings"

Actual code can be found in the zipped mod, I converted this one to NCMS



UnitClipBoard: Active


Simple copy paste system for units, even working across worlds



WorldBox3D: Maintained


Funniest mod to show off, uses a collection of tricks to force the game into various modes of 3d

The camera can be rotated and even repositioned in the world

Paper3D: The simplest, main mode, repositions and rotates everything but leaves them flat

Thick3D: Same as paper except some stuff has been duplicated and layered close together to simulate a thickness

Line3D: A terrain visualizer which uses Unity's LineRenderer component to simulate tiles at the correct heights

World3D: Similar to paper but the tiles and everything on them are at the correct heights

Snapshot3D: Similar to world and thick combined. Looks the best visually but offers zero gameplay



WorldBoxOnline: Abandoned


Used the same networking system as the GeneralPurpose series, but this was made first and only supported WorldBox

Synced a few things like power usage, but overall didn't have enough interest to even test properly

Was too much work to focus on making one whole game networked, so I moved on to the "general purpose" versions



z_com_TWrecksMod: Deprecated


A commissioned RPG mod, this was made after LuBu but before SuperRPG

It added customization options to exp, and a way to give traits as a unit levels
