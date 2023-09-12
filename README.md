# Augmented Reality Robot Programming

This is the repository of the software from the application of a paper to SoftwareX. This contains software that allows to define trajectories for an Universal Robots UR3e robot in relation to two markers using the
Augmented Reality framework Vuforia and Unity.

    
## Installation

It is needed to have a PC and an Android device connected to the same local network.

On the PC install the Android Platforms tools for the corresponding OS from:
https://developer.android.com/tools/releases/platform-tools. If needed add the adb binary to PATH.

Install RoboDK from:
https://robodk.com/download

Install RoboDK Python API from:
https://pypi.org/project/robodk/

Install pose-estimation.apk on a device running Android 9.0 or higher.

Enable developer options and inside them enable USB Debbuging and Wirelees Debugging. For Xiaomi devices it is needed to also enable USB Debugging (Security Settings).

A copy of the fixed marker used by the Android application is found on fixed_target.pdf.


## Usage/Example

Open trajectories_robolink.rdk with RoboDK.  

Allow Wireless Debuging on the local network.

Connect the Android device via USB with USB debugging enabled.

Run read_poses.py.

If a windows pops up asking for a post-processor select the Universal Robots post-processor.

Accept the permissions for USB Debugging. Press enter.

On the consoles enter the IP address shown on Wireless Debuging options. Press enter.

Press enter if the device is listed as connected. Otherwise remove USB Debuging and Wireless Debugging permissions and try again from the start of this section.

Use the Android app to define trajectories with respect to the virtual robot base (represented by the cube).

Send the defined trajectories to the PC.

A text file with the script for the robot should pop up. You can save it and run it on a UR3e robot.

Example of the expected output on the console:

```console
$ python read_poses.py

* daemon not running; starting now at tcp:5037
* daemon started successfully
Connect the phone via USB with USB debugging enabled and accept the permissions for USB Debugging
Press Enter to continue...
List of devices attached
c2ee2599        device

restarting in TCP mode port: 5555
List of devices attached
c2ee2599        device

Enter IP address shown inside Wireless Debuging options: 192.168.1.237
connected to 192.168.1.237:5555
List of devices attached
c2ee2599        device
192.168.1.237:5555      device

Disconnect the USB cable from the phone
Press Enter to continue...
Use the Android app to define trajectories with respect to the virtual robot base (represented by the cube)
Waiting...
Data read successfully
Program created successfully
```
