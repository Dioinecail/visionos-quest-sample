using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace Project.Hands
{
    public class AdaptiveHandSkeletonDriver : XRHandSkeletonDriver
    {
        //private Dictionary<XRHandJointID, Pose> m_LastParentPoses = new Dictionary<XRHandJointID, Pose>();
        //private Dictionary<XRHandJointID, Pose> m_LastFingerPoses = new Dictionary<XRHandJointID, Pose>();



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
                var wristTransform = m_JointTransformReferences.FirstOrDefault(j => j.xrHandJointID == XRHandJointID.Wrist).jointTransform;

                if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palmJointPose))
                {
                    CalculateLocalTransformPose(wristJointPose, palmJointPose, out var palmPose);
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

                        if (hand.GetJoint(jointId).TryGetPose(out fingerJointPose))
                        {
                            //                          world pos   world pos        out local pos 
                            CalculateLocalTransformPose(parentPose, fingerJointPose, out jointLocalPose);

                            var parentLocal = wristTransform.InverseTransformPoint(parentPose.position);
                            var fingerLocal = wristTransform.InverseTransformPoint(fingerJointPose.position);

                            // TODO: 
                            // change caching of world space rotation to local rotation
                            // because world space rotation will get messy 
                            //m_LastParentPoses[jointId] = new Pose(parentLocal, parentPose.rotation);
                            //m_LastFingerPoses[jointId] = new Pose(fingerLocal, fingerJointPose.rotation);
                        }
                        else
                        {
                            break;

                            // TODO: convert local position to worldspace position
                            //var parentWorld = wristTransform.TransformPoint(parentCachedPose.position);
                            //var fingerWorld = wristTransform.TransformPoint(fingerCachedPose.position);

                            //// change using world space rotation to local rotation
                            //parentPose = new Pose(parentWorld, parentCachedPose.rotation);
                            //fingerJointPose = new Pose(fingerWorld, fingerCachedPose.rotation);

                            //CalculateLocalTransformPose(parentPose, fingerJointPose, out jointLocalPose);
                        }

                        // world pos world pos
                        parentPose = fingerJointPose;
                        //                            local pos
                        jointLocalPoses[jointIndex] = jointLocalPose;
                    }
                }
            }
        }

        private static void CalculateLocalTransformPose(in Pose parentPose, in Pose jointPose, out Pose jointLocalPose)
        {
            var inverseParentRotation = Quaternion.Inverse(parentPose.rotation);
            jointLocalPose.position = inverseParentRotation * (jointPose.position - parentPose.position);
            jointLocalPose.rotation = inverseParentRotation * jointPose.rotation;
        }

        //private bool IsMetacarpal(XRHandJointID id)
        //{
        //    return id == XRHandJointID.LittleMetacarpal
        //    || id == XRHandJointID.RingMetacarpal
        //    || id == XRHandJointID.MiddleMetacarpal
        //    || id == XRHandJointID.IndexMetacarpal;
        //}
    }
}