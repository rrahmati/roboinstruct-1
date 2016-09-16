#!/usr/bin/env python

# Copyright (c) 2013-2015, Rethink Robotics
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions are met:
#
# 1. Redistributions of source code must retain the above copyright notice,
#    this list of conditions and the following disclaimer.
# 2. Redistributions in binary form must reproduce the above copyright
#    notice, this list of conditions and the following disclaimer in the
#    documentation and/or other materials provided with the distribution.
# 3. Neither the name of the Rethink Robotics nor the names of its
#    contributors may be used to endorse or promote products derived from
#    this software without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
# AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
# IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
# ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
# LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
# CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
# SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
# INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
# CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
# ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
# POSSIBILITY OF SUCH DAMAGE.

import argparse
import roslib
import rospy
import threading
import numpy
import baxter_interface
import sys
import signal
import numpy as np
import copy
import math

from geometry_msgs.msg import (
    PoseStamped,
    Pose,
    Point,
    Quaternion,
)

from ar_track_alvar_msgs.msg import (
    AlvarMarkers)

from tf.transformations import *
from baxter_examples import IKSolver
from baxter_interface import CHECK_VERSION


def signal_handler(signal, frame):
    poseToJoints._end_thread = True
    sys.exit(0)
signal.signal(signal.SIGINT, signal_handler)

class PoseToJoints(object):

    def __init__(self, filename, limb):
        print("Initializing node... ")
        rospy.init_node("rsdkPoseToJoints")
        print("Getting robot state... ")
        rs = baxter_interface.RobotEnable(CHECK_VERSION)
        print("Enabling robot... ")
        rs.enable()
        self._filename = filename
        self._start_time = rospy.get_time()
        self._limb = limb

        self._limb_left = baxter_interface.Limb("left")
        self._limb_right = baxter_interface.Limb("right")
        self._left_names = self._limb_left.joint_names()
        self._right_names = self._limb_right.joint_names()
        self._gripper_left = baxter_interface.Gripper('left', CHECK_VERSION)
        self._gripper_right = baxter_interface.Gripper('right', CHECK_VERSION)
        self._gripper_left_status = 1
        self._gripper_right_status = 1
        self._rate = rospy.Rate(200)
        self._execution_timeout = 1000 # miliseconds
        self._execution_start = rospy.get_time() # miliseconds

        self._left_ik_solver = IKSolver('left')
        self._right_ik_solver = IKSolver('right')

        self._initial_right_pose = Pose()
        self._initial_left_pose = Pose()

        self._end_thread = False
        self.thread = threading.Thread(target=self._update_thread)
        self.thread.start()

        if(limb == 'left' or limb == 'both'):
            rospy.Subscriber("/robot/limb/left/endpoint_state", PoseStamped, self.left_end_callback)
        if(limb == 'right' or limb == 'both'):
            rospy.Subscriber("/robot/limb/right/endpoint_state", PoseStamped, self.right_end_callback)
        rospy.Subscriber("/ar_pose_marker", AlvarMarkers, self.objects_pose_callback)
        self._executed = True
        print 'initialized'

    def _time_stamp(self):
        return rospy.get_time() - self._start_time

    def try_float(self, x):
        try:
            return float(x)
        except ValueError:
            return None

    def execute(self, limb, pose):
#         print pose[1:8]
        pose[1:8] = self.transform_unity_to_baxter(pose[1:8])
        
#         print self.transform_baxter_to_unity(pose[1:8])
        if limb == 'left':
            self._left_ik_solver.solve(pose[1:4], pose[4:8])
            self._gripper_left_status = pose[0]
            if self._gripper_left_status > .5 and pose[0] < .5:
                self._gripper_left.close()
            elif self._gripper_left_status < .5 and pose[0] > .5:
                self._gripper_left.open()
        if limb == 'right':
            self._right_ik_solver.solve(pose[1:4], pose[4:8])
            self._gripper_right_status = pose[0]
            if self._gripper_right_status > .5 and pose[0] < .5:
                self._gripper_right.close()
            elif self._gripper_right_status < .5 and pose[0] > .5:
                self._gripper_left.open()
        self._executed = False
        self._execution_start = rospy.get_time()

    def pose_to_pos_and_ori(self, pose):
        pos = numpy.array([pose.position.x, pose.position.y, pose.position.z])
        al, be, ga = euler_from_quaternion(numpy.array([pose.orientation.x, pose.orientation.y, pose.orientation.z, pose.orientation.w]))
        ori = numpy.array([al, be, ga])
        return (pos, ori)

    def pos_and_ori_to_pose(self, pos, ori):
        pose = Pose()
        pose.position.x = pos[0]; pose.position.y = pos[1]; pose.position.z = pos[2]
        pose.orientation.x = ori[0]; pose.orientation.y = ori[1]; pose.orientation.z = ori[2]; pose.orientation.w = ori[3]
        return pose

    def right_end_callback(self, state):
        self._current_right_pose = state.pose
        if(self._initial_right_pose.position.x == 0):
            self._initial_right_pose = state.pose
    def left_end_callback(self, state):
        self._current_left_pose = state.pose
        if(self._initial_left_pose.position.x == 0):
            self._initial_left_pose = state.pose
    
    def objects_pose_callback(self, state):
        if len(state.markers) > 0:
            self._objects_pose = [None] * len(state.markers)
            for i in range(len(state.markers)):
                self._objects_pose[i] = state.markers[i].pose.pose
        
    def pose_to_array(self, pose):
        return np.array([pose.position.x, pose.position.y, pose.position.z, pose.orientation.x, pose.orientation.y, pose.orientation.z, pose.orientation.w])

    def transform_unity_to_baxter(self, unity_pose):
        # transorm end-effector position
        # unity: x = backward, y = up, z = right
        # baxter: x = forward, y = left, z = up
        baxter_pose = copy.copy(unity_pose)
        unity_pose[0:3] /= 1.8
        baxter_pose[0] = -unity_pose[0] + 1.3
        baxter_pose[1] = -unity_pose[2] - .15
        baxter_pose[2] = unity_pose[1] - .98
        
#         transform end-effector orientation, it is calculated using trial and error, so, don't expect a understandable code here
        baxter_pose[3] = unity_pose[6]
        baxter_pose[4] = unity_pose[3]
        baxter_pose[5] = unity_pose[4]
        baxter_pose[6] = unity_pose[5]
        ori_matrix = quaternion_matrix(baxter_pose[3:7])
        angle, axis, point = rotation_from_matrix(ori_matrix)
        axis[2] = -axis[2]
        ori_matrix = rotation_matrix(-angle, axis)
        rot_matrix = euler_matrix(1.57,0,0)
        rot_matrix = euler_matrix(0,0,3.14).dot(rot_matrix)
        rot_matrix = rot_matrix.dot(ori_matrix)
        angle, axis, point = rotation_from_matrix(rot_matrix)
        axis_0 = axis[0]
        axis[0] = -axis[1]
        axis[1] = axis_0
        rot_matrix = rotation_matrix(angle, axis)
        ori_quat = quaternion_from_matrix(rot_matrix)
        baxter_pose[3] = ori_quat[1]
        baxter_pose[4] = ori_quat[2]
        baxter_pose[5] = ori_quat[3]
        baxter_pose[6] = ori_quat[0]
        
        return baxter_pose
    def transform_baxter_to_unity(self, baxter_pose):
        # unity: x = backward, y = up, z = right
        # baxter: x = forward, y = left, z = up
        unity_pose = copy.copy(baxter_pose)
        unity_pose[0] = -baxter_pose[0] + 1.3
        unity_pose[1] = baxter_pose[2] + .98
        unity_pose[2] = -baxter_pose[1] - .15
        unity_pose[0:3] *= 1.8
#         transform end-effector orientation, it is calculated using trial and error, so, don't expect an understandable code here
        unity_pose[3] = baxter_pose[4]
        unity_pose[4] = baxter_pose[5]
        unity_pose[5] = baxter_pose[6]
        unity_pose[6] = baxter_pose[3]
        ori_matrix = quaternion_matrix(unity_pose[3:7])
        angle, axis, point = rotation_from_matrix(ori_matrix)
        axis_0 = axis[0]
        axis[0] = axis[1]
        axis[1] = -axis_0
        ori_matrix = rotation_matrix(angle, axis)
        rot_matrix = euler_matrix(0,0,-3.14)
        rot_matrix = euler_matrix(-1.57,0,0).dot(rot_matrix)
        rot_matrix = ori_matrix.dot(rot_matrix)
        angle, axis, point = rotation_from_matrix(rot_matrix)
        axis[2] = -axis[2]
        rot_matrix = rotation_matrix(-angle, axis)
        ori_quat = quaternion_from_matrix(rot_matrix)
        unity_pose[3] = ori_quat[1]
        unity_pose[4] = ori_quat[2]
        unity_pose[5] = ori_quat[3]
        unity_pose[6] = ori_quat[0]
        return unity_pose
    def transform_kinect_to_unity(self, kinect_pose):
        # unity: x = backward, y = up, z = right
        # kinect: x = forward, y = left, z = up
        unity_pose = copy.copy(kinect_pose)
        unity_pose[0] = -kinect_pose[0] + 1.5
        unity_pose[1] = kinect_pose[2] + .60
        unity_pose[2] = -kinect_pose[1] + .05
        unity_pose[0:3] *= 1.8
#         transform end-effector orientation, it is calculated using trial and error, so, don't expect an understandable code here
        unity_pose[3] = kinect_pose[4]
        unity_pose[4] = kinect_pose[5]
        unity_pose[5] = kinect_pose[6]
        unity_pose[6] = kinect_pose[3]
        ori_matrix = quaternion_matrix(unity_pose[3:7])
        angle, axis, point = rotation_from_matrix(ori_matrix)
        axis_0 = axis[0]
        axis[0] = axis[1]
        axis[1] = -axis_0
        ori_matrix = rotation_matrix(angle, axis)
        rot_matrix = euler_matrix(3.14,0,-3.14)
        rot_matrix = euler_matrix(-1.57,0,0).dot(rot_matrix)
        rot_matrix = ori_matrix.dot(rot_matrix)
        angle, axis, point = rotation_from_matrix(rot_matrix)
        axis_1 = axis[1]
        axis[1] = axis[2]
        axis[2] = axis_1
        rot_matrix = rotation_matrix(-angle, axis)
        ori_quat = quaternion_from_matrix(rot_matrix)
        unity_pose[3] = ori_quat[1]
        unity_pose[4] = ori_quat[2]
        unity_pose[5] = ori_quat[3]
        unity_pose[6] = ori_quat[0]
        return unity_pose
    def write_env_state(self, limb):
        if hasattr(self, '_objects_pose'):
            with open(self._filename, 'w') as f:
                f.write('time,gripper,gripper_center_p_x,gripper_center_p_y,gripper_center_p_z,gripper_center_r_x,gripper_center_r_y,gripper_center_r_z,gripper_center_r_w,')
                f.write('box (1)_p_x,box (1)_p_y,box (1)_p_z,box (1)_r_x,box (1)_r_y,box (1)_r_z,box (1)_r_w,')
                f.write('task')
                f.write('\n' + str(rospy.get_time()) + ',' + str(self._gripper_left_status if limb == 'left' else self._gripper_right_status) + ',')
                if limb == 'left':
                    limb_pose_left = self.transform_baxter_to_unity(self.pose_to_array(self._current_left_pose))
                    f.write(','.join([str(x) for x in limb_pose_left]) + ',')
                elif limb == 'right':
                    limb_pose_right = self.transform_baxter_to_unity(self.pose_to_array(self._current_right_pose))
                    f.write(','.join([str(x) for x in limb_pose_right]) + ',')
                object_pose = self.transform_kinect_to_unity(self.pose_to_array(self._objects_pose[0]))
#                 print object_pose
#                 print np.array([1.439304,0.9475655,0.4852195, 0, 0, 0, 1])
                f.write(','.join([str(x) for x in object_pose]) + ',')
    #             f.write('1.066,1,0.450393,0,0,0,1,')
                f.write('18,')
        else:
            print 'No information about position of the objects.'

    def difference_of_joints(self):
        self._right_ik_solver.solution_to_execute = self._right_ik_solver.solution
        self._left_ik_solver.solution_to_execute = self._left_ik_solver.solution
        right_diff = 0; left_diff = 0
        if self._right_ik_solver.foundSolution:
            angles_right_nums = numpy.array([self._limb_right.joint_angle(j) for j in self._right_names])
            solution_right = numpy.array(self._right_ik_solver.solution)
            right_diff = numpy.linalg.norm(solution_right - angles_right_nums)*10.0
            if right_diff < 10:
                self._right_ik_solver.solution_to_execute = (angles_right_nums + (solution_right - angles_right_nums)/10.0).tolist()
            else:
                self._right_ik_solver.solution_to_execute = (angles_right_nums + (solution_right - angles_right_nums)/right_diff).tolist()
        if self._left_ik_solver.foundSolution:
            angles_left_nums = numpy.array([self._limb_left.joint_angle(j) for j in self._left_names])
            solution_left = numpy.array(self._left_ik_solver.solution)
#             print angles_left_nums, solution_left
            left_diff = numpy.linalg.norm(solution_left - angles_left_nums)*10.0
            if left_diff < 10:
                self._left_ik_solver.solution_to_execute = (angles_left_nums + (solution_left - angles_left_nums)/10.0).tolist()
            else:
                self._left_ik_solver.solution_to_execute = (angles_left_nums + (solution_left - angles_left_nums)/left_diff).tolist()
#         print right_diff, left_diff
        if ( right_diff < 10 and left_diff < 10 ) or rospy.get_time() - self._execution_start > self._execution_timeout:
            # print right_diff, left_diff
            self._executed = True
#             self.write_env_state()


    def _update_thread(self):
        rospy.loginfo("Starting Joint Update Thread:")
        print 'Baxter execution thread started. '
        while not rospy.is_shutdown() and not self._end_thread:
            self.difference_of_joints()
            if self._right_ik_solver.foundSolution:
                right_angles = dict(zip(self._right_names[0:], self._right_ik_solver.solution_to_execute[0:]))
                self._limb_right.set_joint_positions(right_angles)
            if self._left_ik_solver.foundSolution:
                left_angles = dict(zip(self._left_names[0:], self._left_ik_solver.solution_to_execute[0:]))
                self._limb_left.set_joint_positions(left_angles)
            # if self._right_ik_solver.foundSolution == False and  self._left_ik_solver.foundSolution == False:
            #     print self._right_ik_solver.foundSolution, self._left_ik_solver.foundSolution
            # if not self._right_ik_solver.foundSolution or not self._left_ik_solver.foundSolution:
            #     self._executed = True
            self._rate.sleep()
        rospy.loginfo("Stopped")


def main():
    epilog = """
    Related examples:
      joint_position_file_playback.py; joint_trajectory_file_playback.py.
    """
    arg_fmt = argparse.RawDescriptionHelpFormatter
    parser = argparse.ArgumentParser(formatter_class=arg_fmt,
                                     description=main.__doc__,
                                     epilog=epilog)
    parser.add_argument(
        '-l', '--limb', default='right',
        help='limb to move.'
    )
    parser.add_argument(
        '-f', '--file', default='nofile', dest='filename',
        help='the file name to read from'
    )
    args = parser.parse_args(rospy.myargv()[1:])

    poseTojoints = PoseToJoints(args.filename, args.limb)

    rospy.spin()

if __name__ == '__main__':
    main()

