using Project.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace Project.Hands
{
    public abstract class HandPoseProvider : PoseProvider
    {
        [SerializeField] protected XRHandSkeletonDriver m_HandSkeletonDriver;



        public bool TryGetJoint(TrackedHandJoint joint, out HandJointPose jointPose)
        {
            var joints = m_HandSkeletonDriver.jointTransformReferences;
            var jointIndex = TrackedHandJointUtil.TrackedHandToUnityHand(joint);
            var foundJoint = joints.FirstOrDefault(j => j.xrHandJointID == jointIndex);

            if (foundJoint.jointTransform != null)
            {
                jointPose = new HandJointPose(foundJoint.jointTransform.position, foundJoint.jointTransform.rotation, 0.1f);

                return true;
            }
            else
            {
                jointPose = new HandJointPose();
                return false;
            }
        }
    }
}