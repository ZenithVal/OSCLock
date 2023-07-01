# OSCLock
This app can unlock a bluetooth lock that uses the ESmartLock app. It's been well tested it with [this lock.](https://amzn.to/3JAGxmm) <br> A simple use case might be throwing regular keys into a lockbox secured by the bluetooth lock. <br> We take no responsibility for unsafe usage and provide no warranty. Always a backup plan!

<br>

# Modes
OSCLock's parameters & timings are fully configurable. <br> Check down in the config section or the config.toml file.

### Timer:
- Starts a timer for a specified duration. 
- While the timer is running, unlock functions of the app are disabled.
- Specific avatar parameters that can add or remove some time 
- Will readout a parameter so you can reflect the current timer on your avatar.

### Basic:
 - By default, unlock functions of the app are disabled
 - When a specific parameter is recieved, the app will enable unlock functions.

### Testing: 
 - Unlock functionality is always available. Good for testing your lock.

### Counter:
 - TBD

Although the app is built for An ESmartLock in mind, **it doesn't actually require one to function.** If you'd like to entirely skip over the physical device and just use it's timer control functions and readouts, feel free to skip 3-8 in setup.


<br>
<br>

# First Time Setup
You can get the latest zip [from releases](https://github.com/ZenithVal/OSCLock/releases) (On the right side panel) <br> Unzip the entire folder wherever. You can also clone the git and build it yourself. 

1. Start OSCLock.exe once to generate a config.toml
2. Exit the application and open the config.toml next to the executable
3. Add your eSmartLock credentials to the config.toml
4. Start OSCLock
5. Unlock your lock (Press U)
6. Once successfully completed, exit the application
7. In the config.toml, device_password should now be filled in
8. You can remove your esmart username and password if you'd like.
9. Configure everything else in the confg.toml to your heart's content.

##### *Skip 3-8 if you don't actually have a physical lock*


<br>
<br>

# Avatar Setup
Avatar setup is decided by the user. You can use any of the readout modes to fit your avatar setup. 
A simple digital timer using readout mode 3 can be found on at https://zenithval.booth.pm/items/4892327


<br>
<br>

# Config - Main
| Value           | Info                                                        | Default     |
|:--------------- | ----------------------------------------------------------- |:-----------:|
| ip              | Address to send OSC data to                                 | "127.0.0.1" |
| listener_port   | Port to listen for OSC data on                              | 9001        |
| write_port      | Port to send OSC data to                                    | 9000        |
| mode            | Testing, Basic, Or Timer                                    | "Timer"     |
| debugging       | Extra console readouts, mainly for OSC debugging            | false       |
| lock_type       | Not used yet, maybe for different bluetooth locks later.    | ESmartLock  |
| esmart_username | Account username for login                                  | ""          |
| esmart_password | Account password for login                                  | ""          |
| device_password | Lock passcode will be written here after a successful login | ""          |


<br>

## Config - Timer
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
| dec_parameter      | When this Bool is true via OSC, it should decrease the timer.    | ""      |
| dec_step           | Time in seconds to subtract (int)                                | 300     |
| input_delay        | Minimum cooldown between allowed inputs.                         | 1500    |
|                    |                                                                  |         |
| readout_mode       | Method of translating time remaining via OSC. Chart below        | 0       |
| readout_parameter  | Readout parameter 1                                              | ""      |
| readout_parameter2 | Readout parameter 2 (optional)                                   | ""      |
| readout_interval   | Time in miliseconds between parameter updates.                   | 500     |
|                    |                                                                  |         |

<br>
<br>

# Readout modes
readout_mode determines how data is output from OSCLock. Choose a method that works for you and your avatar. <br> P1 = readout_parameter and P2 = readout_parameter2

| readout_mode | Use of Readout parameters                                      |
|:------------ | -------------------------------------------------------------- |
| 0            | No readout parameter will be used                              |
| 1            | P1, float 0 to +1                                              |
| 2            | P1, float -1 to +1                                             |
| 3            | P1, float -1 to +1 for minutes. P2, float -1 to +1 for seconds |
| 4            | P1, float -1 to +1 for minutes. P2, int 1:1 with seconds       |
| 5            | P1 & P2, ints 1:1 minutes and seconds respectively             |
| 6            | P1, int 1:1 mins/seconds. P2, bool determines min/sec data     |


<br>
<br>

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


<br>
<br>

# Encryption
Encryption uses very basic encryption to obfuscate the config.toml and timer files. After pressing { in the app, you'll be prompted with a password prompt. You can paste from your clipboard if you wish. If encryption is enabled, the timer can not simply be ended early by deleting the timer files. Decryption will force end the current time. 

It's only "effective" if OSCLock is the only method easily available to open the lock. You can do this by not keeping the eSmartLock app or not memorizing the login and logging out. Encrypt OSCLock with a code impossible to remember, maybe print it or DM it to someone you trust to send back to you if needed.

Goes without saying, only use this if you're confident and have confirmed it can open your lock!


# FAQ

### Q: My newly added parameters aren't working! <br>

- Reset OSC or delete the OSC folder at `C:\Users\(Username)\AppData\LocalLow\VRChat\VRChat` <br>
- Did you include `/avatar/parameters/` at the start? EG: `/avatar/parameters/headpat_sensor` <br>
- If your VRC parameter has spaces, replace the spaces with underscores, EG: `headpat_sensor` 

<br>

### Q: What locks does this work with?

Theoretically, this should work with ANY bleueooth Lock that uses the ESmartLock App. <br>
Look for the white/green color laytout with the ESmartLock Icon. If a specific brand doesnt work please let us know. 
- [EseeSmart](https://amzn.to/3PuaTuo) 
- [ELinkSmart](https://amzn.to/3ra1NsM)
- [Pothunder](https://amzn.to/3r1EJfv) 
- [Dhiedas](https://amzn.to/46t4xBC)

<br>

# Roadmap
The code as it is right now was only ever really meant to be a draft but we all know how that goes. <br> At some point it'll probably be refactored, optimized, and, modularized to make it easier to add features.

Want to:
 - The above
 - Add more modes
   - Extensions of the basic timer mode.
   - Some "Gamified" elements.
 - Automate a simple avatar setup
 - Unique avatar add ons/integrations
 - Have Zeni actually know how to code
 - Support difficent brands of bluetooth locks (VERY painful without an API)
 - Make the encryption feature not security theater (Probably never, might be a safety hazard.)

<br>

# Credits & Liscense 

- GitLab Mirror: https://gitlab.com/osclock/osclock
- Original programming before git by @NeetCode. 08/2022
- SharpOSC [MIT Liscense](https://github.com/tecartlab/SharpOSC/blob/master/License.txt)
- App Icon  [Game-icons.net](https://game-icons.net/1x1/delapouite/locked-heart.html) under [CC by 3.0](https://creativecommons.org/licenses/by/3.0/)