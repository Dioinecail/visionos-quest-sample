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
        [SerializeField] private float m_HandFacingAwayTolerance;

        private float? m_IndexFingerLength = null;



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

        public bool TryGetIndexFingerLength(out float length)
        {
            bool gotData = true;

            gotData &= TryGetJoint(TrackedHandJoint.IndexTip, out HandJointPose indexTip);

            // If our first query fails, we've lost tracking, and we reset the cached finger length
            // to be recomputed when the hand is visible again.
            if (!gotData)
            {
                m_IndexFingerLength = null;
                length = 0.0f;
                return false;
            }

            // If we are tracked and also have a cached finger length, return that.
            if (m_IndexFingerLength.HasValue && m_IndexFingerLength.Value != 0.0f)
            {
                length = m_IndexFingerLength.Value;
                return true;
            }
            else
            {
                // Otherwise, we compute a fresh finger length.
                gotData &= TryGetJoint(TrackedHandJoint.IndexProximal, out HandJointPose indexKnuckle);
                gotData &= TryGetJoint(TrackedHandJoint.IndexIntermediate, out HandJointPose indexMiddle);
                gotData &= TryGetJoint(TrackedHandJoint.IndexDistal, out HandJointPose indexDistal);

                if (gotData)
                {
                    m_IndexFingerLength = Vector3.Distance(indexKnuckle.Position, indexMiddle.Position) +
                                        Vector3.Distance(indexMiddle.Position, indexDistal.Position) +
                                        Vector3.Distance(indexDistal.Position, indexTip.Position);

                    length = m_IndexFingerLength.Value;
                    return true;
                }
                else
                {
                    m_IndexFingerLength = null;
                    length = 0;
                    return false;
                }
            }
        }

        public bool TryGetPalmFacingAway(out bool palmFacingAway)
        {
            bool gotData = TryGetJoint(TrackedHandJoint.Palm, out HandJointPose palm);

            if (!gotData)
            {
                palmFacingAway = false;
                return false;
            }

            palmFacingAway = IsPalmFacingAway(palm);
            return gotData;
        }

        protected bool IsPalmFacingAway(HandJointPose palmJoint)
        {
            if (Camera.main == null)
            {
                return false;
            }

            Vector3 palmDown = palmJoint.Rotation * -Vector3.up;

            // The original palm orientation is based on a horizontal palm facing down.
            // So, if you bring your hand up and face it away from you, the palm.up is the forward vector.
            if (Mathf.Abs(Vector3.Angle(palmDown, Camera.main.transform.forward)) > m_HandFacingAwayTolerance)
            {
                return false;
            }

            return true;
        }
    }
}