## More information on what is installed
There are three main components that are installed:
* A set of config files
* A hitsound file
* Minor HUD improvements

The config files will always be installed, but the rest may be opted out by choosing "Advanced
Install" during setup. Some additional detail on each component has been added below.

### Config files
The config, `neodefaults.cfg`, is a script file that configures in-game settings like the FOV. Many
of these options are available to be edited in the "Advanced Options" menu inside of TF2. Every
command in the file is provided with an explanation of why the setting is being changed.

The config files will be stored under `<your TF2 install path>/tf/cfg/NeoDefaults/`, and can be
opened with a text editor like Notepad. `neodefaults.cfg` cannot be edited, however. See
[Customizing NeoDefaults](#customizing-neodefaults) for more information.

### Hitsound
The hitsound provided (as well as the pitch settings) come from
[b4nny](https://www.twitch.tv/b4nny)'s config, but is originally from Quake. The low damage values
have a pleasant sound, and the high damage values have a very satisfying bass.

This was added as part of the installer because a hitsound is a very reliable way of judging damage
dealt, and can help with decision-making while playing. For example, hearing only high-pitch damage
sounds during a fight would mean an opponent has taken very little damage, which would make
retreating a wise choice.

The hitsound is installed under `tf/custom/` in the `neodefaults-quake-hitsound.vpk` file, but the
pitch settings are configured in `neodefaults.cfg`.

### HUD
While the default HUD is not really a good choice compared to all the other custom HUDs available
(just ask [woolen](https://youtu.be/gW6YXCfGgdQ?t=228) 🙂), deciding on a custom HUD mostly revolves
around personal preference. Instead of replacing the HUD entirely, some minor modifications are
placed in the `neodefaults-hud-tweaks.vpk ` file, which is stored in the `tf/custom` directory.
Currently, the only change made is to increase the size of damage numbers to make them more
readable, as discussed in the [README](/README.md#what-gets-changed).

This VPK file is only intended for use with the default HUD, and may not work if you are using a custom HUD.

## Customizing NeoDefaults
If NeoDefaults changes a game setting that you don't like, you can fix it by overriding that
setting in the `custom.cfg` file that NeoDefaults creates for you.

For example, let's say you wanted to disable the hitsound. First, open the `neodefaults.cfg` file.
This is located in the `<your TF2 install path>/tf/cfg/NeoDefaults/` folder. You can use Notepad or
a similar program to open this.

Next, find the setting that you want to change. Every setting in the file has a description, so you
can read through it to see which setting is the one you want to change. In this example, the setting
that turns on/off the hitsound is called `tf_dingalingaling`. To reset this value, add the following
line to `custom.cfg`:
```
tf_dingalingaling 0
```
Now, after a restart (or if you run `exec autoexec` in your console), doing damage will no longer
play a sound.

## Uninstalling NeoDefaults
An uninstall tool is planned for a future release. Currently, this has to be done manually, but it
is a short process.

Open your `Team Fortress 2/tf/` folder. This folder will have a `cfg/` and a `custom/` folder inside
it.

Open `cfg/`, and delete the folder called `NeoDefaults`. Then, in `custom/`, there will be two
`.vpk` files with "neodefaults" in the name. Delete these two files. There will likely also be two
".cache" files with "neodefaults" in the name, and these can be deleted too.

Unfortunately, due to the way TF2 works, the settings NeoDefaults changed (e.g., FOV value, damage
numbers, etc.) will stay the same, so you will need to manually set these back to what they were
before in the Advanced Options menu in the game. This is annoying, so hopefully, this problem will
be solved when the uninstall tool is released.

## The log file
Computers are complex machines, and it's possible that you could run into problems while trying to
run the installer. To help diagnose these, the installer writes its operations to a log file. This
is stored under the `NeoDefaults` folder in `ProgramData`. You can get to the ProgramData folder by
opening the start menu and typing `%ProgramData%`. If you open the log file, it will look something
like:
```
Logfile initialized on: 5/18/2020 11:26:44 PM
Version 1.0.0

---------------------------------------------------------------------------------
Beginning automatic filepath check...

Checking if the path exists: C:\Program Files (x86)\Steam\SteamApps\common\Team Fortress 2\tf
...
```
You can refer to this file for more detail on what happened the last time the program was run,
especially if you're trying to find the cause of an error. Any failures that occur will have the
associated stacktraces printed here.

The log file is rotated to keep the two most recent runs (if the installer is run multiple times),
which are named `log.txt` and `log_prev.txt`. All older logs will be deleted.
