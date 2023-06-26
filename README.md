VRChat OSC Tool that can put control an eseesmart bluetooth lock.

This app can control an eseesmart Bluetooth lock. I've personally tested it with this one https://www.amazon.com/gp/product/B096S7PTS1

When you're done with initial setup you can unlink your fingerprint and logout of the app. (Let someone you trust change the password maybe?)

<br>

# Config

| Value                 | Info                                                        | Default     |
|:--------------------- | ----------------------------------------------------------- |:-----------:|
| ip                    | Address to send OSC data to                                 | "127.0.0.1" |
| listener_port         | Port to listen for OSC data on                              | 9001        |
| write_port            | Port to send OSC data to                                    | 9000        |
| mode                  | Timer is the only mode atm                                             | "Timer"     |
| esmart_username       | Account username for login                                  | ""          |
| esmart_password       | Account password for login                                  | ""          |
| device_password | Lock passcode will be written here after a successful login | ""          |

### Mode - Timer

You won't be able to unlock the Bluetooth lock until this timer finishes.

| Value              | Info                                                          | Default         |
|:------------------ | ------------------------------------------------------------- |:---------------:|
| max                | Max minutes at a given moment. How much sand can the hourglass hold?       | 60              |
| absolute_min       | Miniumum time that must pass before the system can unlock.    | 0               |
| absolute_max       | If the total time has reached this, it can not increase       | 0               |
| starting_value     | Time in minutes the timer should start at. Random if -1       | -1              |
| random_min         | Random minimum time                                           | 40              |
| random_min         | Random maximum time                                           | 60              |
| inc_parameter      | When this Bool is true via OSC, it should increase the timer. | "timer_inc"     |
| inc_step           | Time in minutes to add (int)                                  | 1               |
| dec_parameter      | When this Bool is true via OSC, it should decrease the timer. | "timer_dec"     |
| dec_step           | Time in minutes to subtract (int)                             | 1               |
| readout_mode       | Method of translating time remaining via OSC                  | 1               |
| readout_parameter  | Readout parameter 1                                           | "timer_readout" |
| readout_parameter2 | Readout parameter 2 (optional)                                | ""              |


| readout_mode | Use of Readout parameters                            |
|:------------ | ---------------------------------------------------- |
| 0            | No readout parameter will be used                    |
| 1            | parameter1 is a float (0 to +1)                      |
| 2            | parameter1 is a float (-1 to +1) for extra precision |
| 3            | parameter1 and 2 are ints for minutes and seconds    |

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
| F7    | Encrypts the application config with a password |
| F8    | Encrypts the application config with a password |

SyncMode

"Timer" mode is the feature complete part of this app atm. You won't be able to unlock the Bluetooth lock until this timer finishes. 

The Timer is limited to displaying 126 minutes due to integer limitations.

A float (0-1) is set and updated every second to reflect the percent of time remaining from your maximum timer value. You can use this to put the timer on your avatar like the example image below.
A parameter from VRC can be read to increase the time by x amount. EG: Headpats increase the timer by 1 minute


# Credits

- @FrostbyteVR is responsible for the original code of this tool.