# godot-hog (Sonic Throwback)
Abandoned project. Feel free to fork and build off of it. I suggest you swap out the assets for your own.
 Momentum platformer in Godot 4.2.
 Built off [Sonic-Onset code](https://github.com/cuckydev/SonicOnset-Source), adding features such as hurt state and tweaking the online code.
 
## Features
- Sonic
    - A modified version of Sonic from Onset Adventure, adding a LightDash, Death and Hurt state.
- Modular stage select
    - stages are stored in their own pck file and loaded at runtime.
- Online
    - Supports upnp and peer-to-peer multiplayer. Only works on TestStage.
- Options menu
    - Menu features video and audio settings, allowing you to tweak and save resolution and volume
    - Stage select dynamically loads tiles based on available stage pcks.

## Installation
 1. Download the repo as a zip or clone the repo
 2. Open Godot 4.2 or later and import the project.
 3. To build, go to project/export and use the Windows Desktop (Runnable)
 4. To build a stage, use PCK instead. Go to Resources and only tick the stage folder contents.

 ## Credits
 - Roy (Me) - coding additional features, UI elements, and model porting.
 - [cuckydev](https://github.com/cuckydev) - Original Sonic-Onset code used as a foundation
 - [KamauKianjahe](https://ko-fi.com/kamaukianjahe/) - Sonic edits and animations
 - LandyRS - Sonic Voice
 - Sonic Team - Sounds and models from the Sonic series
