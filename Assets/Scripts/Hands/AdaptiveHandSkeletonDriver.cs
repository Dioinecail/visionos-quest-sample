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
        [SerializeField] private Transform m_CameraTransform;
        [SerializeField] private TMPro.TMP_Text m_Debug;



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
                            CalculateLocalTransformPose(parentPose, fingerJointPose, out jointLocalPose);
                        }

                        parentPose = fingerJointPose;
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

        private void Update()
        {
            var joints = m_JointTransformReferences.Where(j => j.xrHandJointID == XRHandJointID.LittleMetacarpal
            || j.xrHandJointID == XRHandJointID.RingMetacarpal
            || j.xrHandJointID == XRHandJointID.MiddleMetacarpal
            || j.xrHandJointID == XRHandJointID.IndexMetacarpal);

            var sb = new System.Text.StringBuilder();

            foreach (var joint in joints)
            {
                var p = joint.jointTransform.position;
                var lp = joint.jointTransform.localPosition;

                sb.AppendLine($"index: '{joint.xrHandJointID}' | pos: '{p}' | local(cm): '{lp * 10f}'");
            }

            m_Debug.text = sb.ToString();
        }
    }
}