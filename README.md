# OSCLock
An OSC timer app that can optionally unlock a bluetooth lock that uses the ESmartLock phone app. 
<br> A simple use case might be throwing regular keys into a lockbox secured by the bluetooth lock. 

![Preview](https://raw.githubusercontent.com/ZenithVal/OSCLock/main/preview.png)
<p align="center">
⚠ Keep in mind bluetooth can be unreliable. BE SAFE  ⚠
</p>

[[_TOC_]]

# Modes
OSCLock's parameters & timings are fully configurable. <br> Check down in the config section or the config.toml file.

### Timer
- Starts a timer for a specified duration. 
- While the timer is running, unlock functions of the app are disabled.
- Specific avatar parameters that can add or remove some time 
- Will readout a parameter so you can reflect the current timer on your avatar.

### Basic
 - By default, unlock functions of the app are disabled
 - When a specific parameter is recieved, the app will enable unlock functions.

### Testing
 - Unlock functionality is always available. Good for testing your lock.

### Counter
 - TBD

Although the app is built for An ESmartLock in mind, **it doesn't actually require one to function.** If you'd like to entirely skip over the physical device and just use it's timer control functions and readouts, feel free to skip 3-8 in setup.

<br>

# Config

Tables containing information on all available settings you can change in the config.toml files. <br> Click a > to expand the section.

<details><summary>Main Settings</summary>

| Value           | Info                                                        | Default     |
|:--------------- | ----------------------------------------------------------- |:-----------:|
| OSCQuery        | Enable [OSCQuery](https://github.com/vrchat-community/vrc-oscquery-lib) (IP & Listening port will be ignored)| false|
||||
| ip              | Address to send OSC data to                                 | "127.0.0.1" |
| listener_port   | Port to listen for OSC data on                              | 9001        |
| write_port      | Port to send OSC data to                                    | 9000        |
||||
| mode            | Testing, Basic, Or Timer                                    | "Timer"     |
| debugging       | Extra console readouts, mainly for OSC debugging            | false       |
||||
| lock_type       | Not used yet, maybe for different bluetooth locks later.    | ESmartLock  |
| esmart_username | Account username for login                                  | ""          |
| esmart_password | Account password for login                                  | ""          |
| device_password | Lock passcode will be written here after a successful login | ""          |
</details>

<details><summary>Timer Settings</summary>

| Value              | Info                                                             | Default |
|:------------------ | ---------------------------------------------------------------- |:-------:|
| max                | Maximum time. How much sand can the hourglass hold at a time?    | 60      |
| absolute_min       | Time will be added if it total time is below this. 0 disables.   | 0       |
| absolute_max       | If overall time reaches this, inc_step wont work. 0 disables.    | 120     |
|                    |                                                                  |         |
| starting_value     | Time in minutes the timer should start at. Random if -1          | -1      |
| random_min         | Random minimum time                                              | 40      |
| random_min         | Random maximum time                                              | 60      |
|                    |                                                                  |         |
| inc_parameter      | When this Bool is true via OSC, it should increase the timer.    | ""      |
| inc_step           | Time in seconds to add (int)                                     | 60      |
| manual_parameter   | When this Bool is true via OSC, it should decrease the timer.    | ""      |
| input_cooldown     | Minimum cooldown between allowed inputs.                         | 0       |
|                    |                                                                  |         |
| readout_mode       | Method of translating time remaining via OSC. Chart below        | 4       |
| readout_parameter1 | Readout parameter 1                                              | ""      |
| readout_parameter2 | Readout parameter 2 (optional)                                   | ""      |
|                    |                                                                  |         |
| cooldown_param     | True while cooldown is active                                    | ""      |
| capacity_max_param | True while capacity of timer is maxed.                           |         |
| absolute_max_param | True while timer has reached it's absolute max.                  |         |
| readout_interval   | Time in miliseconds between parameter updates.                   | 500     |
</details>


<details><summary>Readout Modes</summary>

readout_mode determines how data is output from OSCLock. <br> Choose a method that works for you and your avatar. 

> P1 = readout_parameter1 and P2 = readout_parameter2

| readout_mode | Use of Readout parameters                                      |
|:------------ | -------------------------------------------------------------- |
| 0            | No readout parameter will be used                              |
| 1            | P1, float 0 to +1                                              |
| 2            | P1, float -1 to +1                                             |
| 3            | P1, float -1 to +1 for minutes. P2, float -1 to +1 for seconds |
| 4            | P1, int, P2 int. Automatically formatted for MM:SS or HH:MM    |

In v1.02, some readout modes were removed for sanity.
<br> If you used them please let me know.
</details>


<br>

# Setup
You can get the latest zip [from releases](https://gitlab.com/osclock/osclock/-/releases) (On the right side panel) <br> Unzip the entire folder wherever. You can also clone the git and build it yourself. 

1. Start OSCLock.exe once to generate a config.toml
2. Exit the application and open the config.toml next to the executable
3. Add your eSmartLock credentials to the config.toml
4. Start OSCLock
5. Unlock your lock (Press U)
6. Once successfully completed, exit the application
7. In the config.toml, device_password should now be filled in
8. You can remove your esmart username and password if you'd like.
9. Configure everything else in the confg.toml to your heart's content.
10. **HAVE A BACKUP PLAN AND BE SAFE. Bluetooth CAN'T BE TRUSTED.**

*Skip 3-8 if you don't actually have a physical lock*


# Avatar Setup
Avatar setup is decided by the user. You can use any of the readout modes to fit your avatar setup. A simple digital timer using readout mode 3 can be found on at https://zenithval.booth.pm/items/4892327


# In app Controls
| Value | Info                                            |
|:----- | ----------------------------------------------- |
| H     | Prints the help screen                          |
| T     | Starts a new timer (If in timer mode)           |
| S     | Prints the status of the app and lock           |
| U     | Begins unlock process if available              |
| Q     | Quits the application                           |
| {     | Encrypts the application config with a password |
| }     | Decrypts the application config with a password |

<details><summary>Encryption</summary>

This uses very basic encryption to obfuscate the config.toml and timer files. After pressing { in the app, you'll be prompted to enter a password. If encryption is enabled, the timer can not simply be ended early by deleting the timer files. Decryption will force end the current time. 

A fun way to use this might be encrypting the app with a code you don't remember and giving it to someone you trust. Goes without saying, **only use this if you're confident and have confirmed it can open your lock and you're BEING SAFE.**

It's hidden in the app interface but the { and } buttons still function. <br>

</details>


<br>

# FAQ
### How safe is this?
- **IT'S NOT.** PLEASE exercise caution.
- **Do not put the lock on anything that could put YOU in danger.** 
- Bluetooth is unreliable as heck. It'll drop randomly sometimes.
- Remember you can't open the lock without clicking the unlock button!
- You charged the lock, right?
- **HAVE. A. BACKUP. PLAN.**


### What locks does it work with?
Theoretically, this should work with ANY bleueooth Lock that uses the ESmartLock App. <br>
Look for the white/green color laytout with the ESmartLock Icon. If a specific brand doesnt work please let us know. 
- It's been well tested it with [this lock.](https://amzn.to/3JAGxmm) 
- [EseeSmart](https://amzn.to/3PuaTuo) 
- [ELinkSmart](https://amzn.to/3ra1NsM)
- [Pothunder](https://amzn.to/3r1EJfv)
- [Dhiedas](https://amzn.to/46t4xBC)

### Why arent my newly added parameters working? <br>
- Reset OSC or delete the OSC folder at `C:\Users\(Username)\AppData\LocalLow\VRChat\VRChat` <br>
- Did you include `/avatar/parameters/` at the start? EG: `/avatar/parameters/headpat_sensor` <br>
- If your VRC parameter has spaces, replace the spaces with underscores, EG: `headpat_sensor` 

### Why isn't my timer accute for others?
 - Keep in mind VRchat synced Ints/Floats can only represent 128 values. *(Only two accurate 2 decimal places on a -1 to +1 float)*
 - Readout mode 0 and 1 (0 to +1 or -1 to +1) are only good for reading out minutes. No seconds
 - You'll need a second parameter for the seconds (other readout modes) or a special avatar setup to make pseudo seconds. IMO, not worth the pain.


<br>

# Roadmap
The code as it is right now was only ever really meant to be a draft but we all know how that goes. At some point it'll probably be refactored, optimized, and, modularized to make it easier to add features.

Want to:
 - The above.
 - New Modes:
   - Extensions of the basic timer mode.
   - Some "Gamified" elements.
   - Feel free to suggest.
 - Unique avatar add ons/integrations.
 - Support difficent brands of bluetooth locks. (VERY painful without an API, unlikely)
 - Make the encryption feature not security theater. (Probably never, would be a safety hazard.)


<br>

# Credits & Licenses

- Prior programming before git history by @NeetCode 08/2022
- SharpOSC | [MIT Liscense](https://github.com/tecartlab/SharpOSC/blob/master/License.txt)
- OSCQuery | [MIT Liscense](https://github.com/vrchat-community/vrc-oscquery-lib/blob/main/License.md)
- Tomlet | [MIT Liscense](https://github.com/SamboyCoding/Tomlet/blob/master/LICENSE)
- FluentColorConsole | [MIT Liscense](https://github.com/developer82/FluentColorConsole/blob/master/LICENSE)
- App Heart Icon | [Game-icons.net](https://game-icons.net/1x1/delapouite/locked-heart.html) under [CC by 3.0](https://creativecommons.org/licenses/by/3.0/)