KSPReloaded
===========

KSPReloaded is an add-on for the popular space program simulator Kerbal Space Program (https://kerbalspaceprogram.com)

Installation
============

This is a stand-alone add-on for KSP, you do not need any part files or otherwise. You simply need to compile the source into it's .dll target and drop it in the Plugins folder of KSP.

What it does
============

KSPReloaded aims to provide you with an in-game add-on manager. Due to limitations with the Unity Engine, KSP cannot reload assemblies (add-ons) as it does other resources. This skirts those limitations and gives you an interface accessed anywhere in game (F10) to allow you to load, unload, and reload add-ons without exiting the game. There are severe limitations currently, and it is currently most useful for other mod developers who are testing and need to frequently reload a mod.

Things to note
==============

The loading and unloading features are, in a word, missing. They don't work, and even if they did they wouldn't work correctly. They'll be coming along shortly.

KSPReloaded renames your add-on files (.dlls) to allow it to perform the trickery that it does, so you won't recognize the files after I have at them. I intend this to be a complete add-on manager soon enough, with the idea that you never even need to look into the Plugins folder again.

Limitations
===========

Loading/Unloading -- don't have em'. Reloading works like a champion though.

Contributing
============

If you're a player who finds this mod, feel free to submit bugs and feature requests directly via github. If you're a fellow modder, I fully encourage you to fork and submit a pull request with any changes you make.
