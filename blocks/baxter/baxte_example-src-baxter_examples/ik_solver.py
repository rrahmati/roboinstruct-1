#!/usr/bin/env python

# Copyright (c) 2013, University Of Massachusetts Lowell
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
# 3. Neither the name of the University of Massachusetts Lowell nor the names
#    from of its contributors may be used to endorse or promote products
#    derived this software without specific prior written permission.
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

import roslib
import rospy

from geometry_msgs.msg import (
    PoseStamped,
    Pose,
    Point,
    Quaternion,
)
from sensor_msgs.msg import (
    JointState,
)
from std_msgs.msg import Header
from baxter_core_msgs.srv import SolvePositionIK
from baxter_core_msgs.srv import SolvePositionIKRequest


class IKSolver(object):
    def __init__(self, limb):
        self.limb = limb
        ns = "ExternalTools/" + limb + "/PositionKinematicsNode/IKService"
        rospy.wait_for_service(ns)
        self.iksvc = rospy.ServiceProxy(ns, SolvePositionIK)
        self.solution = dict()
        self.foundSolution = False
        self.cooledDown = True
        self.last_solve_request_time = rospy.Time.now()
        rospy.Subscriber("/robot/joint_states", PoseStamped, self.joint_states_callback)

    def cooled_down(self):
        return rospy.Time.now() - self.last_solve_request_time > rospy.Duration(0.01)

    def joint_states_callback(self, joint_states):
        self.current_joint_states = joint_states

    def solve(self, pos, ori):
        self.last_solve_request_time = rospy.Time.now()
        ikreq = SolvePositionIKRequest()
        hdr = Header(
            stamp=rospy.Time.now(), frame_id='base')
        pose = PoseStamped(
            header=hdr,
            pose=Pose(
                position=Point(x=pos[0], y=pos[1], z=pos[2]),
                orientation=Quaternion(x=ori[0], y=ori[1], z=ori[2], w=ori[3])
            ),
        )
        ikreq.pose_stamp.append(pose)
        # ikreq.seed_angles.append(self.current_joint_states)
        try:
            resp = self.iksvc(ikreq)
            if (resp.isValid[0]):
                self.solution = resp.joints[0].position
                self.foundSolution = True
                # rospy.loginfo("Solution Found, %s" % self.limb)
                return True

            else:
                self.foundSolution = False
                rospy.logwarn("INVALID POSE for %s" % self.limb)
                return False
        except rospy.ServiceException, e:
            self.foundSolution = False
            rospy.loginfo("Service call failed: %s" % (e,))

        
