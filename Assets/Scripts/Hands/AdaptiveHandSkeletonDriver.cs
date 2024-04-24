using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace Project.Hands
{
    public class AdaptiveHandSkeletonDriver : XRHandSkeletonDriver
    {
        [SerializeField] private float m_ForFunFactor = 0.5f;



        protected override void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
        {
            UpdateJointLocalPosesOculus(args);

            ApplyUpdatedTransformPoses();
        }

        private void UpdateJointLocalPosesOculus(XRHandJointsUpdatedEventArgs args)
        {
            CalculateJointTransformLocalPosesOculus(ref args.hand, ref m_JointLocalPoses);
        }

        private void CalculateJointTransformLocalPosesOculus(ref XRHand hand, ref NativeArray<Pose> jointLocalPoses)
        {
            var wristIndex = XRHandJointID.Wrist.ToIndex();

            if (hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristJointPose))
            {
                jointLocalPoses[wristIndex] = wristJointPose;
                var palmIndex = XRHandJointID.Palm.ToIndex();

                if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palmJointPose))
                {
                    CalculateLocalTransformPoseOculus(wristJointPose, palmJointPose, 1f, out var palmPose);
                    jointLocalPoses[palmIndex] = palmPose;
                }

                for (var fingerIndex = (int)XRHandFingerID.Thumb;
                     fingerIndex <= (int)XRHandFingerID.Little;
                     ++fingerIndex)
                {
                    var parentPose = wristJointPose;
                    var fingerId = (XRHandFingerID)fingerIndex;

                    var jointIndexBack = fingerId.GetBackJointID().ToIndex();
                    var jointIndexFront = fingerId.GetFrontJointID().ToIndex();
                    for (var jointIndex = jointIndexFront;
                         jointIndex <= jointIndexBack;
                         ++jointIndex)
                    {
                        var jointId = XRHandJointIDUtility.FromIndex(jointIndex);
                        var fingerJointPose = Pose.identity;
                        var jointLocalPose = Pose.identity;

                        if(jointId == XRHandJointID.ThumbMetacarpal)
                        {
                            if (hand.GetJoint(jointId).TryGetPose(out fingerJointPose))
                            {
                                CalculateLocalTransformPoseOculus(parentPose, fingerJointPose, m_ForFunFactor, out jointLocalPose);
                            }
                        }
                        else
                        {
                            if (hand.GetJoint(jointId).TryGetPose(out fingerJointPose))
                            {
                                CalculateLocalTransformPoseOculus(parentPose, fingerJointPose, 1f, out jointLocalPose);
                            }
                        }

                        parentPose = fingerJointPose;
                        jointLocalPoses[jointIndex] = jointLocalPose;
                    }
                }
            }
        }

        private void CalculateLocalTransformPoseOculus(in Pose parentPose, in Pose jointPose, float scaleFactor, out Pose jointLocalPose)
        {
            var inverseParentRotation = Quaternion.Inverse(parentPose.rotation);
            jointLocalPose.position = inverseParentRotation * ((jointPose.position - parentPose.position) * scaleFactor);
            jointLocalPose.rotation = inverseParentRotation * jointPose.rotation;
        }
    }
}