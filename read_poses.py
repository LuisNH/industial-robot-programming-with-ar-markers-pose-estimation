from robodk.robolink import *
from robodk.robomath import *
import numpy as np
import subprocess
import time

###################################################################################
# First, execute the command (with the phone connected via USB): 'adb tcpip 5555'
# The script runs: adb connect 'ip'
# The script runs: adb devices
###################################################################################

global RUN_MODE

RDK = Robolink()


class Parametrization:
    def __init__(self, subprocess):
        self.subprocess = subprocess

    # Connect to the Android device
    def pairToDevice(self):
        # Restart the adb server
        self.subprocess.run(['adb', 'kill-server'], shell=True)
        self.subprocess.run(['adb', 'start-server'], shell=True)
        
        print('Connect the phone via USB with USB debugging enabled and accept the permissions for USB Debugging')
        input('Press Enter to continue...')

        self.subprocess.run(['adb', 'devices' ], shell=True) # Show devices connected via adb
        self.subprocess.run(['adb', 'tcpip', '5555'], shell=True) # Open port 5555 on the phone
        self.subprocess.run(['adb', 'devices' ], shell=True) # Show devices connected via adb

        self.ip = input('Enter IP address shown inside Wireless Debuging options: ')
        self.subprocess.run(['adb', 'connect' , self.ip], shell=True) # Connect adb over wifi to 'ip'

        time.sleep(4)

        self.subprocess.run('adb devices', shell=True) # Show devices connected via adb
        print('Disconnect the USB cable from the phone')
        input('Press Enter to continue...')

    # Get position and rotation of the WorkObject and return pose
    def getLogcat(self):
        print('Use the Android app to define trajectories with respect to the virtual robot base (represented by the cube)\nWaiting...')
        while True:
            # Retrieve the Unity application log and save it to 'logcat.txt'
            self.subprocess.run('adb logcat -d | findstr -i unity > logcat.txt', shell=True)

            file_log = open('logcat.txt', 'r', encoding='utf-8') # Open the 'logcat.txt' file
            Lines = file_log.readlines()
            
            for i, line in enumerate(Lines):
                # Check if any line contains 'Seam:'
                if (line.find('Seam:') > -1):
                    i_pos = Lines[i+1].find('(mm) = ')
                    i_rot = Lines[i+2].find('(degrees) = ')

                    position = list(map(int, Lines[i+1][i_pos+7:].split(', '))) # WorkObject position
                    rotation = list(map(int, Lines[i+2][i_rot+11:].split(', '))) # WorkObject orientation
                    pose = position + rotation

                    i_num = Lines[i+3].find('Number of points: ')
                    number_of_points = int(Lines[i+3][i_num+18:])
                    points = np.zeros((number_of_points, 3))
                    for j in range(number_of_points):
                        i_point = Lines[i+j+4].find('(')
                        point = list(map(float, Lines[i+j+4][i_point+1:-2].split(', ')))
                        points[j,:] = np.asarray(point)
                    print('Data read successfully')
                    return (*pose, points)

            self.subprocess.run('adb logcat -c', shell=True) # Clear the log
            self.subprocess.run('break>logcat.txt', shell=True) # Empty 'logcat.txt'


if __name__ == '__main__':
    ROBOT_NAME = 'UR3e'
    HOME_ITEM = 'Home'
    TOOL = 'TCP_Marker'
    WORKOBJECT = 'WorkObject'
    ROBOT_ROUNDING = 2.5
    ROBOT_VELOCITY = 50
    ROBOT_JOIN_VELOCITY = 200
    RUN_MODE = RUNMODE_MAKE_ROBOTPROG
    X_TOOL, Y_TOOL, Z_TOOL = 16.91, -17.48, 47.5
    RX_TOOL, RY_TOOL, RZ_TOOL = np.pi, 0, 0

    RDK.setRunMode(RUN_MODE)
    robot = RDK.Item(ROBOT_NAME, ITEM_TYPE_ROBOT)
    workObject = RDK.Item(WORKOBJECT, ITEM_TYPE_FRAME)
    toolPose = TxyzRxyz_2_Pose([X_TOOL, Y_TOOL, Z_TOOL, RX_TOOL, RY_TOOL, RZ_TOOL])

    robot.setRounding(ROBOT_ROUNDING)

    robot.setPoseTool(toolPose)

    param = Parametrization(subprocess)
    param.pairToDevice()

    x, y, z, rx, ry, rz, points = param.getLogcat()

    workObject.setPose(TxyzRxyz_2_Pose([x, y, z, rx * np.pi/180, ry * np.pi/180, rz * np.pi/180]))

    # Specific posture to place the UR3e robot in an specific configuration and avoid singularities
    angle = np.arctan2(y, x) * 180 / np.pi + 25.08821348

    robot.setSpeed(ROBOT_VELOCITY)
    robot.setSpeedJoints(ROBOT_JOIN_VELOCITY)

    robot.setJoints([0, -90, 0, -90, 0, 0])

    approach = workObject.Pose()

    robot.MoveJ([angle, -90, -90, -90, 90, 0])

    robot.MoveL(approach * transl(points[0,:]) * transl(0, 0, 50))

    for i in range(points.shape[0]):
        robot.MoveL(workObject.Pose() * transl(points[i,:]))

    robot.MoveL(approach * transl(points[-1,:]) * transl(0, 0, 50))

    print('Program created')
