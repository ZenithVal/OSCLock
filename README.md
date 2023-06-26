VRChat OSC Tool that can put control an eseesmart bluetooth lock.

This app can control an eseesmart Bluetooth lock. I've personally tested it with [this lock.](https://www.amazon.com/gp/product/B096S7PTS1) <br>
I take no responsibility for unsafe usage of this app and provide no warranty. Have a backup plan!

<br>

# First Time Setup

This guide assumes you have an Esseesmart bluetooth lock and have set it up to be opened with the app. If not, go ahead and do that that first. You can remove all fingerprints from the lock.

1. Start the application, it will generate a config.toml file
2. Exit the application and open the config.toml file
3. Add your esmart credetials and save the file
4. Start OSCLock
5. Start a new timer
6. Unlock the lock
7. if successful, device_password should be filled in within the config
8. You can remove your esmart login info and just leave the device password
9. Configure everything to your hearts content.
10. You can change your esmart login or remove the app from your phone if you'd like.
11. Encryption works well if you have a friend you trust to hold onto a backup code

I throw my regular keys into a lockbox locked by the bluetooth lock.

# Config

| Value           | Info                                                        | Default     |
|:--------------- | ----------------------------------------------------------- |:-----------:|
| ip              | Address to send OSC data to                                 | "127.0.0.1" |
| listener_port   | Port to listen for OSC data on                              | 9001        |
| write_port      | Port to send OSC data to                                    | 9000        |
| mode            | Timer is the only mode atm                                  | ""          |
| esmart_username | Account username for login                                  | ""          |
| esmart_password | Account password for login                                  | ""          |
| device_password | Lock passcode will be written here after a successful login | ""          |

### Mode - Timer

You won't be able to unlock the Bluetooth lock until this timer finishes.
A parameter from VRC can be read to increase the time by x amount. EG: Headpats increase the timer by 1 minute

| Value              | Info                                                          | Default |
|:------------------ | ------------------------------------------------------------- |:-------:|
| max                | Max held minutes. How much sand can the hourglass hold?       | 60      |
| absolute_min       | Miniumum time that must pass before the system can unlock.    | 0       |
| absolute_max       | If the total time has reached this, it can not increase       | 0       |
|                    |                                                               |         |
| starting_value     | Time in minutes the timer should start at. Random if -1       | 0       |
| random_min         | Random minimum time                                           | 40      |
| random_min         | Random maximum time                                           | 60      |
|                    |                                                               |         |
| inc_parameter      | When this Bool is true via OSC, it should increase the timer. | ""      |
| inc_step           | Time in minutes to add (int)                                  | 1       |
| dec_parameter      | When this Bool is true via OSC, it should decrease the timer. | ""      |
| dec_step           | Time in minutes to subtract (int)                             | 1       |
|                    |                                                               |         |
| readout_mode       | Method of translating time remaining via OSC. Chart below     | 1       |
| readout_parameter  | Readout parameter 1                                           | ""      |
| readout_parameter2 | Readout parameter 2 (optional)                                | ""      |
| readout_interval   | Time in miliseconds between parameter updates.                | 500     |

| readout_mode | Use of Readout parameters                                   |
|:------------ | ----------------------------------------------------------- |
| 0            | No readout parameter will be used                           |
| 1            | param1 is a float (0 to +1)                                 |
| 2            | param1 is a float (-1 to +1) for higher precision           |
| 3            | param1 and 2 are ints for minutes and seconds               |
| 4            | Param1 is an int, param 2 is a bool for switching mins/secs |

VRChat clamps synced floats, which is why so many readout mods are offered. Use whatever works for your setup.

### TBD Mode

| Value | Info | Default |
|:----- | ---- |:-------:|
|       |      |         |

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

# Credits

- @FrostbyteVR is responsible for all the original code of this tool prior to it reaching github.