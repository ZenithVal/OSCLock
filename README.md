# OSCLock

This app can control any bluetooth lock that uses the ESmartLock app. It's been tested it with [this lock.](https://amzn.to/3JAGxmm) <br> A simple use case might be throwing regular keys into a lockbox secured by the bluetooth lock. <br> We take no responsibility for unsafe usage and provide no warranty. Have a backup plan!

<br>

## Getting the app

1. **Via the executable**
   - Download latest zip [from releases](https://gitlab.com/osclock/osclock/-/releases)
   - Extract wherever.
   - Run the Executable.
   - Follow first time setup.
2. **From the source**
   - Clone the git.
   - Open the sln.
   - Install required nuget packages.

# First Time Setup

This guide assumes you have an ESmartLock bluetooth lock and have set it up to be opened with the app. <br> If not, go ahead and do that that first. You probably don't need to assign fingerprints to the lock. <br> This app can be used without a physical lock in a more pretend manner if you want as well.

1. Start the application, it will generate a config.toml file.
2. Exit the application and open the config.toml file.
3. Add your esmart credetials and save the file.
4. Start OSCLock.
5. Unlock your lock. (U)
7. Once successful, exit the application.
8. In config.toml, device_password should now be filled in. 
9. You can remove your esmart username and password if you'd like.
9. Configure everything else in the confg.toml to your heart's content.
10. Encryption works well if you have a friend you trust to hold onto the code.

--- 

<br>

# Config

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

### Mode - Timer

You won't be able to unlock the Bluetooth lock until this timer finishes.
A parameter from VRC can be read to increase the time by x amount. EG: Headpats increase the timer by 1 minute

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

Readout mode determines how data is output from OSCLock. Choose a method that works for you and your avatar. <br> P1 = readout_parameter and P2 = readout_parameter2

| readout_mode | Use of Readout parameters                                      |
|:------------ | -------------------------------------------------------------- |
| 0            | No readout parameter will be used                              |
| 1            | P1, float 0 to +1                                              |
| 2            | P1, float -1 to +1                                             |
| 3            | P1, float -1 to +1 for minutes. P2, float -1 to +1 for seconds |
| 4            | P1, float -1 to +1 for minutes. P2, int 1:1 with seconds       |
| 5            | P1 & P2, ints 1:1 minutes and seconds respectively             |
| 6            | P1, int 1:1 mins/seconds. P2, bool determines min/sec data     |

--- 

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
| }     | Encrypts the application config with a password |

--- 

<br>

# Avatar Setup
Avatar setup is decided by the user. You can use any of the readout modes to fit your avatar setup. <br>
A simple digital timer using readout mode 3 can be found on at https://zenithval.booth.pm/items/4892327


--- 

<br>

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
 - Make the encryption feature not security theater
 - Have Zeni actually know how to code
 - Support difficent brands of bluetooth locks (VERY painful without an API)
 - PiShock?

<br>

# Credits & Liscense 

- SharpOSC [MIT Liscense](https://github.com/tecartlab/SharpOSC/blob/master/License.txt)
- App Icon  [Game-icons.net](https://game-icons.net/1x1/delapouite/locked-heart.html) under [CC by 3.0](https://creativecommons.org/licenses/by/3.0/)

