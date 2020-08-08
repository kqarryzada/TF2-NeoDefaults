# The NeoDefaults Installer

NeoDefaults is a config installer geared towards new players of Team Fortress 2. It is designed to
modify game settings to values that everyone should use, and in a way that is as fast, seamless,
and as easy to use as possible. It can be used as a quick "set and forget" for those who don't want
to bother customizing their game manually, but still want the essentials.

## The Problem
Team Fortress 2's default settings are horribly out-of-date, and terrible for new players. The good
thing about TF2 is that it is extremely customizable, so these bad settings can be (and are) easily
changed by seasoned players. However, this can make things worse for new players, because they are
now using completely different settings than their opponents, putting them at a disadvantage.

## The Solution
This program installs plugins and script files to provide a small list of changes that all players
need, while avoiding changing anything that could infringe on personal preference (as much as
possible, at least). Some of the changes include:
* Disabling mouse acceleration to help with aim
* Improving network settings to make the game more responsive
* Updating the FOV settings so that the screen is less obscured

Instead of providing a step-by-step list of instructions on how to install a set of files, this
installer takes those files and does that work for you. It aims to make it so easy that there's no
reason to avoid using it. All changes that NeoDefaults makes are fully supported by Valve and are
not considered exploits.

## What Gets Changed
Probably the most noticeable differences are the settings related to the FOV and viewmodel:

![Soldier FOV changes](
https://raw.githubusercontent.com/kqarryzada/TF2-NeoDefaults/master/resource/readme-images/soldier_fov.png)
In the image on the left, the Sniper is nearly invisible because his entire body is hidden by the
rocket launcher's viewmodel. Also notice that on the right image, much more of the surrounding
area can be seen (e.g., the +/- on the left, the windows at the top, etc.). This image makes it
clear that the default FOV settings give a very tunnel-visioned perspective.

Another noticeable change is the way damage numbers appear in the game. Ordinarily, these numbers
are tiny and difficult to read. With some modifications made to the HUD, these numbers can be made
much bigger:

![Damage Numbers](
https://raw.githubusercontent.com/kqarryzada/TF2-NeoDefaults/master/resource/readme-images/pyro_damagenumbers.png)
Notice that NeoDefaults enables `hud_combattext_batching` in the image on the right, so the
individual damage numbers are automatically summed together.

## How It Works
Every game option in TF2 has an associated console command. For example, the "turn on autoheal"
setting in the Advanced Options is called `tf_medigun_autoheal`. If you're familiar with the
developer console, you can open it and type `tf_medigun_autoheal 1` to enable this, or
`tf_medigun_autoheal 0` to disable it.

If you write a bunch of these commands into a single file, you can have TF2 open this file and apply
all of the settings inside it. These script files always end in `.cfg`.

NeoDefaults has a file called `neodefaults.cfg`, which contains a list of commands that it considers
ideal. When you run the installer, it creates the `neodefaults.cfg` file on your computer. Then, it
tells TF2 to always run this file when you start your game.

## Minimum Requirements
The installer is currently only supported for systems that use Windows 8 or 10.

## Instructions (Windows only)
1. Download the installer here (link to be provided at a later date).
1. If you're using Windows 8, you may need to right-click the installer and select "Run as
Administrator".
1. Launch the installer and walk through the setup.

## mastercomfig Support
TF2 installs that use [mastercomfig](https://mastercomfig.com/) are supported by NeoDefaults. This
means that the NeoDefaults installer will always check for mastercomfig before installing its own
config files (as mastercomfig expects that `autoexec.cfg` is placed under `cfg/user/` instead of
`cfg/`).

Using mastercomfig with TF2 is highly encouraged, as it is by far the best FPS config for the game.
If you are planning to install mastercomfig, make sure to do that before running this installer.

## FAQ
The FAQ is available [here](docs/FAQ.md).

## Contact Me
Before reaching out, first check the [FAQ](docs/FAQ.md) to make sure your question hasn't already
been answered. If there's nothing there related to your question, then you can do one of the
following:
* Send an e-mail to neodefaults.tf2@gmail.com.
* DM me on Twitter: [@0x0ACE_](https://twitter.com/0x0ACE_)
* Open [a new issue](https://github.com/kqarryzada/TF2-NeoDefaults/issues) here on GitHub.

For a bug report or for help diagnosing issues, you must include the log file that was generated
when you ran into the problem. You can find more info about the log file
[here](docs/More-Info.md/#the-log-file).

## More Information
In the interest of shortening this page, additional information is available on the
[More Info](docs/More-Info.md) page, which discusses:
* An explanation of what each component is and why it's added
* How to customize what NeoDefaults changes
* How to diagnose and report issues

## Special Thanks
In no particular order, a special thank you to:
* Eniere for their work on the [Improved Default HUD](https://huds.tf/forum/showthread.php?tid=276),
which inspired this project to improve the appearance of damage numbers.
* The mastercomfig team for their thorough [documentation](https://docs.mastercomfig.com/en/latest/).
* [b4nny](https://www.twitch.tv/b4nny) for sharing his hitsound and the associated pitch settings.

## License
The conditions of using this software may be viewed in the [LICENSE](LICENSE) file.

Valve, the Valve logo, Steam, the Steam logo, Team Fortress, and the Team Fortress logo are
trademarks and/or registered trademarks of Valve Corporation. The NeoDefaults project is not
affiliated with or endorsed by Valve Corporation.
