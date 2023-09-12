import socket
import time
import json

from scipy.spatial.transform import Rotation as R
from robodk.robolink import *
from robodk.robomath import *
import numpy as np

import UdpComms as U

global RUN_MODE
rot_order_unity = "yxz"
rot_order_robodk = "XYZ"
dist_std = 5.650644918807993
rot_std = 3.2454893109983565


def normalize(v: np.ndarray) -> np.ndarray:
    v_norm = np.linalg.norm(v)
    if v_norm == 0:
        return v_norm
    return v / v_norm


def quatWAvgMarkley(Q: np.ndarray) -> np.ndarray:
    # Number of quaternions to average
    M = Q.shape[0]
    A = np.zeros(shape=(4, 4))

    for i in range(0, M):
        q = Q[i, :]
        # multiply q with its transposed version q' and add A
        A = np.outer(q, q) + A

    # scale
    A = (1.0 / M) * A
    # compute eigenvalues and -vectors
    eigenValues, eigenVectors = np.linalg.eig(A)
    # Sort by largest eigenvalue
    eigenVectors = eigenVectors[:, eigenValues.argsort()[::-1]]
    # return the real part of the largest eigenvector (has only real part)
    return np.real(eigenVectors[:, 0].ravel())


def average_transformations(postures: np.ndarray) -> list:
    position = np.zeros((3))
    q = np.zeros((postures.shape[0], 4))

    for i in range(postures.shape[0]):
        posture = postures[i, :]
        position += posture[:3]

        rotation = posture[3:]

        rot_1 = R.from_euler(
            seq=rot_order_unity,
            angles=[-rotation[0], -rotation[1], -rotation[2]],
            degrees=True,
        )
        rot_2 = rot_1.as_quat(canonical=True)

        q[i, :] = np.roll(rot_2, 1)

    quat = quatWAvgMarkley(q)
    rotation = R.from_quat(np.roll(quat, -1))

    pos = position / postures.shape[0]
    rot = rotation.as_euler(rot_order_unity, degrees=True)

    return [pos[0], pos[1], pos[2], -rot[0], -rot[1], -rot[2]]


def pose_by_2_markers(init_posture: list, final_posture: list) -> list[list, bool]:
    init_posture_rot = init_posture[3:]
    rotation_1 = R.from_euler(
        seq=rot_order_unity,
        angles=[-init_posture_rot[0], -init_posture_rot[1], -init_posture_rot[2]],
        degrees=True,
    )

    final_posture_rot = final_posture[3:]
    rotation_2 = R.from_euler(
        seq=rot_order_unity,
        angles=[-final_posture_rot[0], -final_posture_rot[1], -final_posture_rot[2]],
        degrees=True,
    )

    s1 = np.zeros((3, 3))
    s2 = np.zeros((3, 3))

    s1[:, 1] = normalize(np.subtract(final_posture[:3], init_posture[:3]))
    s1[:, 2] = normalize(rotation_1.apply([0, 0, 1]))

    s2[:, 1] = normalize(np.subtract(final_posture[:3], init_posture[:3]))
    s2[:, 2] = normalize(rotation_2.apply([0, 0, 1]))

    angle = np.arccos(np.clip(np.dot(s1[:, 1], s1[:, 2]), -1, 1))
    if angle == np.pi:
        return [0, 0, 0, 0, 0, 0], False
    axis = normalize(np.cross(s1[:, 1], s1[:, 2]))
    z_rot = R.from_rotvec(-np.sign(angle) * np.pi / 2 * axis)
    s1[:, 1] = normalize(z_rot.apply(s1[:, 2]))
    s1[:, 0] = normalize(axis)
    if np.linalg.det(s1) == -1:
        s1[:, 0] *= -1

    angle = np.arccos(np.clip(np.dot(s2[:, 1], s2[:, 2]), -1, 1))
    if angle == np.pi:
        return [0, 0, 0, 0, 0, 0], False
    axis = normalize(np.cross(s2[:, 1], s2[:, 2]))
    z_rot = R.from_rotvec(-np.sign(angle) * np.pi / 2 * axis)
    s2[:, 1] = normalize(z_rot.apply(s2[:, 2]))
    s2[:, 0] = normalize(axis)
    if np.linalg.det(s2) == -1:
        s2[:, 0] *= -1

    rotation_1 = R.from_matrix(s1)
    rotation_2 = R.from_matrix(s2)

    q = np.zeros((2, 4))

    q[0, :] = rotation_1.as_quat(canonical=True)
    q[1, :] = rotation_2.as_quat(canonical=True)

    quat = quatWAvgMarkley(np.roll(q, 1))
    rotation = R.from_quat(np.roll(quat, -1))

    rot = rotation.as_euler(rot_order_unity, degrees=True)

    z_dif = rotation.apply(np.subtract(final_posture[:3], init_posture[:3]))[2]

    if np.abs(z_dif) > 1.5 * np.sqrt(3) * dist_std:
        return [0, 0, 0, 0, 0, 0], False
    pos = init_posture[:3]

    return [pos[0], pos[1], pos[2], -rot[0], -rot[1], -rot[2]], True


def check_postures_valid(init_postures: np.ndarray, final_postures: np.ndarray) -> list:
    init_dist = True
    init_rot = True
    final_dist = True
    final_rot = True
    distance_poses = True
    parallel = True
    coplanar = True
    posture = []

    init_angles = np.abs(init_postures[:, 3:] - init_postures[:, 3:][:, None])

    init_positions = init_postures[:, :3]
    init_distances = np.linalg.norm(init_positions - init_positions[:, None], axis=-1)

    final_angles = np.abs(final_postures[:, 3:] - final_postures[:, 3:][:, None])

    final_positions = final_postures[:, :3]
    final_distances = np.linalg.norm(
        final_positions - final_positions[:, None], axis=-1
    )

    for i in range(init_postures.shape[0]):
        if (
            init_angles[i, :, 0][init_angles[i, :, 0] != 0].min() > 1.5 * rot_std
            or init_angles[i, :, 1][init_angles[i, :, 1] != 0].min() > 1.5 * rot_std
            or init_angles[i, :, 2][init_angles[i, :, 2] != 0].min() > 1.5 * rot_std
        ):
            init_rot = False

        if (
            final_angles[i, :, 0][final_angles[i, :, 0] != 0].min() > 1.5 * rot_std
            or final_angles[i, :, 1][final_angles[i, :, 1] != 0].min() > 1.5 * rot_std
            or final_angles[i, :, 2][final_angles[i, :, 2] != 0].min() > 1.5 * rot_std
        ):
            final_rot = False

        if init_distances[i, :][init_distances[i, :] != 0].min() > 1.5 * dist_std:
            init_dist = False

        if final_distances[i, :][final_distances[i, :] != 0].min() > 1.5 * dist_std:
            final_dist = False

    if init_dist and init_rot and final_dist and final_rot:
        init_averaged_posture = average_transformations(init_postures)

        final_averaged_posture = average_transformations(final_postures)

        distance_markers = np.linalg.norm(
            np.subtract(init_averaged_posture[:3], final_averaged_posture[:3])
        )
        if distance_markers < 6 * dist_std:
            distance_poses = False

        init_averaged_rot = init_averaged_posture[3:]
        final_averaged_rot = final_averaged_posture[3:]

        rotation_1 = R.from_euler(
            rot_order_unity,
            [-init_averaged_rot[1], -init_averaged_rot[0], -init_averaged_rot[2]],
        )
        rotation_2 = R.from_euler(
            rot_order_unity,
            [-final_averaged_rot[1], -final_averaged_rot[0], -final_averaged_rot[2]],
        )

        angle = np.arccos(
            np.clip(
                np.dot(rotation_1.apply([0, 0, 1]), rotation_2.apply([0, 0, 1])), -1, 1
            )
        )

        if angle > 1.5 * rot_std:
            parallel = False

        if distance_poses and parallel:
            posture, coplanar = pose_by_2_markers(
                init_averaged_posture, final_averaged_posture
            )
        else:
            coplanar = False

    else:
        distance_poses = False
        parallel = False
        coplanar = False

    return (
        init_dist,
        init_rot,
        final_dist,
        final_rot,
        distance_poses,
        parallel,
        coplanar,
        posture,
    )


def trajectory(position: list, rotation: list, points: list[list]) -> None:
    RDK = Robolink()

    ROBOT_NAME = "UR3e"
    HOME_ITEM = "Home"
    TOOL = "TCP_Marker"
    WORKOBJECT = "WorkObject"
    ROBOT_ROUNDING = 2.5
    ROBOT_VELOCITY = 100
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

    rot_1 = R.from_euler(
        seq=rot_order_unity,
        angles=[-rotation[1], -rotation[0], -rotation[2]],
        degrees=True,
    )
    rot_2 = rot_1.as_euler(seq=rot_order_robodk, degrees=False)

    x, y, z = position
    rx, ry, rz = list(rot_2)

    workObject.setPose(TxyzRxyz_2_Pose([x, y, z, rx, ry, rz]))

    # Specific posture to place the UR3e robot in an specific configuration and avoid singularities
    angle = np.arctan2(y, x) * 180 / np.pi + 25.08821348

    robot.setSpeed(ROBOT_VELOCITY)
    robot.setSpeedJoints(ROBOT_JOIN_VELOCITY)

    robot.setJoints([0, -90, 0, -90, 0, 0])

    approach = workObject.Pose()

    robot.MoveJ([angle, -90, -90, -90, 90, 0])

    robot.MoveL(approach * transl(points[0]) * transl(0, 0, 50))

    for point in points:
        robot.MoveL(workObject.Pose() * transl(point))

    robot.MoveL(approach * transl(points[-1]) * transl(0, 0, 50))


def main():
    s1 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s1.bind(("127.0.0.1", 0))
    portTX = s1.getsockname()[1]

    s2 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s2.bind(("127.0.0.1", 0))
    portRX = s2.getsockname()[1]

    s1.shutdown(socket.SHUT_RDWR)
    s1.close()
    s2.shutdown(socket.SHUT_RDWR)
    s2.close()

    hostname = socket.gethostname()
    ip_adress = socket.gethostbyname(hostname)

    print("On the mobile app enter the following IP and ports (enter as it is):")
    print(f"IP: {ip_adress}")
    print(f"portRX: {portTX}")
    print(f"portTX: {portRX}")

    sock = U.UdpComms(
        udpIP=ip_adress,
        portTX=portTX,
        portRX=portRX,
        enableRX=True,
        suppressWarnings=False,
    )

    while True:
        data = sock.ReadReceivedData()

        if data is not None:
            data = data.replace("(", "[").replace(")", "]")
            print(data)
            try:
                response = json.loads(data)
                print(response)
            except:
                print("Data parsing failed")
                continue

            if "function" in response:
                function = response["function"]

                if function == "trajectory":
                    try:
                        pose = response["pose"]

                        position = pose[:3]
                        rotation = pose[3:]

                        points = response["points"]
                        points = map(list, points)

                        trajectory(position, rotation, points)

                        print("Program created")

                        data = {
                            "function": "trajectory",
                            "status": True,
                            "valid": {
                                "init_dist": False,
                                "init_rot": False,
                                "final_dist": False,
                                "final_rot": False,
                                "distance": False,
                                "parallel": False,
                                "coplanar": False,
                            },
                            "posture": [0, 0, 0, 0, 0, 0],
                        }
                        print(data)

                        data_out = json.dumps(data)
                        sock.SendData(data_out)
                    except:
                        print("Failed at creating program")

                        data = {
                            "function": "trajectory",
                            "status": False,
                            "valid": {
                                "init_dist": False,
                                "init_rot": False,
                                "final_dist": False,
                                "final_rot": False,
                                "distance": False,
                                "parallel": False,
                                "coplanar": False,
                            },
                            "posture": [0, 0, 0, 0, 0, 0],
                        }
                        print(data)

                        data_out = json.dumps(data)

                        sock.SendData(data_out)
                elif function == "check_postures_valid":
                    try:
                        init_postures = response["init_postures"]
                        init_postures = np.array(init_postures)

                        final_postures = response["final_postures"]
                        final_postures = np.array(final_postures)

                        (
                            init_dist,
                            init_rot,
                            final_dist,
                            final_rot,
                            distance_poses,
                            parallel,
                            coplanar,
                            posture,
                        ) = check_postures_valid(init_postures, final_postures)

                        data = {
                            "function": "check_postures_valid",
                            "status": True,
                            "valid": {
                                "init_dist": init_dist,
                                "init_rot": init_rot,
                                "final_dist": final_dist,
                                "final_rot": final_rot,
                                "distance": distance_poses,
                                "parallel": parallel,
                                "coplanar": coplanar,
                            },
                            "posture": posture,
                        }
                        print(data)

                        data_out = json.dumps(data)
                        sock.SendData(data_out)
                    except:
                        data = {
                            "function": "check_postures_valid",
                            "status": False,
                            "valid": {
                                "init_dist": False,
                                "init_rot": False,
                                "final_dist": False,
                                "final_rot": False,
                                "distance": False,
                                "parallel": False,
                                "coplanar": False,
                            },
                            "posture": [0, 0, 0, 0, 0, 0],
                        }
                        print(data)

                        data_out = json.dumps(data)
                        sock.SendData(data_out)

        time.sleep(0.1)


if __name__ == "__main__":
    main()
