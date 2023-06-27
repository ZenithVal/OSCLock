This app can control an eseesmart Bluetooth lock. Personally tested it with [this lock.](https://www.amazon.com/gp/product/B096S7PTS1) <br> We take no responsibility for unsafe usage and provide no warranty. Have a backup plan!

<br>

## Getting the app

1. **Via the executable**
   - Download latest zip [from releases](Link.goes.here)
   - Extract wherever.
   - Run the Executable.
   - Follow first time setup.
2. **From the source**
   - Clone the git
   - Open the sln
   - Install required nuget packages.

# First Time Setup

This guide assumes you have an Esseesmart bluetooth lock and have set it up to be opened with the app. <br> If not, go ahead and do that that first. You probably don't need to assign fingerprints to the lock. <br> This app can be used without a physical lock in a more pretend manner if you want as well.

1. Start the application, it will generate a config.toml file.
2. Exit the application and open the config.toml file.
3. Add your esmart credetials and save the file.
4. Start OSCLock.
5. Start a new timer. (T) 
6. Unlock yoru lock. (U)
7. If successful, device_password should be filled in within the config.
8. Configure everything else in the confg.toml to your heart's content.
9. You can change your esmart login or remove the app from your phone if you'd like.
10. Encryption works well if you have a friend you trust to hold onto the code.

A simple use case might be throwing regular keys into a lockbox secured by the bluetooth lock. <br> For setup questions you can ask in #OSC-Talkin in [Zeni's Discord](https://discord.gg/7VAm3twDyy)

--- 

<br>

# Config

| Value           | Info                                                        | Default     |
|:--------------- | ----------------------------------------------------------- |:-----------:|
| ip              | Address to send OSC data to                                 | "127.0.0.1" |
| listener_port   | Port to listen for OSC data on                              | 9001        |
| write_port      | Port to send OSC data to                                    | 9000        |
| mode            | Timer is the only mode atm                                  | ""          |
| lock_type       | Not used yet, maybe for different bluetooth locks later.    | eseesmart   |
| esmart_username | Account username for login                                  | ""          |
| esmart_password | Account password for login                                  | ""          |
| device_password | Lock passcode will be written here after a successful login | ""          |

<br>

### Mode - Timer

You won't be able to unlock the Bluetooth lock until this timer finishes.
A parameter from VRC can be read to increase the time by x amount. EG: Headpats increase the timer by 1 minute

| Value              | Info                                                             | Default |
|:------------------ | ---------------------------------------------------------------- |:-------:|
| max                | Max held minutes. How much sand can the hourglass hold?          | 60      |
| absolute_min       | Will force inc_step until overall time reaches this. 0 disables. | 0       |
| absolute_max       | If overall time reaches this, inc_step wont work. 0 disables.    | 0       |
|                    |                                                                  |         |
| starting_value     | Time in minutes the timer should start at. Random if -1          | -1      |
| random_min         | Random minimum time                                              | 40      |
| random_min         | Random maximum time                                              | 60      |
|                    |                                                                  |         |
| inc_parameter      | When this Bool is true via OSC, it should increase the timer.    | ""      |
| inc_step           | Time in minutes to add (int)                                     | 1       |
| dec_parameter      | When this Bool is true via OSC, it should decrease the timer.    | ""      |
| dec_step           | Time in minutes to subtract (int)                                | 1       |
|                    |                                                                  |         |
| readout_mode       | Method of translating time remaining via OSC. Chart below        | 1       |
| readout_parameter  | Readout parameter 1                                              | ""      |
| readout_parameter2 | Readout parameter 2 (optional)                                   | ""      |
| readout_interval   | Time in miliseconds between parameter updates.                   | 500     |
| <br>               |                                                                  |         |

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

# FAQ

### Q: My Parameters aren't working! <br>

> A1: Reset OSC or delete the OSC folder at `C:\Users\(Username)\AppData\LocalLow\VRChat\VRChat` <br>
> A2: Did you include `/avatar/parameters/` EG: `/avatar/parameters/` <br>
> A3: If your VRC parameter has spaces, replace the spaces with underscores, EG: `head_pat_sensor` 

### Q:

> A: 

### Q:

> A: 

# Credits

- @FrostbyteVR is responsible for all the original code of this tool prior to it reaching github.