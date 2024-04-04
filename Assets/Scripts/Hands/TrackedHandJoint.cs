using UnityEngine.XR.Hands;

namespace Project.Hands
{
    /// <summary>
    /// The supported tracked hand joints.
    /// </summary>
    /// <remarks>See https://en.wikipedia.org/wiki/Interphalangeal_joints_of_the_hand#/media/File:Scheme_human_hand_bones-en.svg for joint name definitions.</remarks>
    public enum TrackedHandJoint
    {
        /// <summary>
        /// The palm.
        /// </summary>
        Palm,

        /// <summary>
        /// The wrist.
        /// </summary>
        Wrist,

        /// <summary>
        /// The lowest joint in the thumb (down in your palm).
        /// </summary>
        ThumbMetacarpal,

        /// <summary>
        /// The thumb's second (middle-ish) joint.
        /// </summary>
        ThumbProximal,

        /// <summary>
        /// The thumb's first (furthest) joint.
        /// </summary>
        ThumbDistal,

        /// <summary>
        /// The tip of the thumb.
        /// </summary>
        ThumbTip,

        /// <summary>
        /// The lowest joint of the index finger.
        /// </summary>
        IndexMetacarpal,

        /// <summary>
        /// The knuckle joint of the index finger.
        /// </summary>
        IndexProximal,

        /// <summary>
        /// The middle joint of the index finger.
        /// </summary>
        IndexIntermediate,

        /// <summary>
        /// The joint nearest the tip of the index finger.
        /// </summary>
        IndexDistal,

        /// <summary>
        /// The tip of the index finger.
        /// </summary>
        IndexTip,

        /// <summary>
        /// The lowest joint of the middle finger.
        /// </summary>
        MiddleMetacarpal,

        /// <summary>
        /// The knuckle joint of the middle finger. 
        /// </summary>
        MiddleProximal,

        /// <summary>
        /// The middle joint of the middle finger.
        /// </summary>
        MiddleIntermediate,

        /// <summary>
        /// The joint nearest the tip of the finger.
        /// </summary>
        MiddleDistal,

        /// <summary>
        /// The tip of the middle finger.
        /// </summary>
        MiddleTip,

        /// <summary>
        /// The lowest joint of the ring finger.
        /// </summary>
        RingMetacarpal,

        /// <summary>
        /// The knuckle of the ring finger.
        /// </summary>
        RingProximal,

        /// <summary>
        /// The middle joint of the ring finger.
        /// </summary>
        RingIntermediate,

        /// <summary>
        /// The joint nearest the tip of the ring finger.
        /// </summary>
        RingDistal,

        /// <summary>
        /// The tip of the ring finger.
        /// </summary>
        RingTip,

        /// <summary>
        /// The lowest joint of the little finger.
        /// </summary>
        LittleMetacarpal,

        /// <summary>
        /// The knuckle joint of the little finger.
        /// </summary>
        LittleProximal,

        /// <summary>
        /// The middle joint of the little finger.
        /// </summary>
        LittleIntermediate,

        /// <summary>
        /// The joint nearest the tip of the little finger.
        /// </summary>
        LittleDistal,

        /// <summary>
        /// The tip of the little finger.
        /// </summary>
        LittleTip,

        /// <summary>
        /// Number of joints total.
        /// </summary>
        TotalJoints = LittleTip + 1
    }

    public static class TrackedHandJointUtil
    {
        public static XRHandJointID TrackedHandToUnityHand(TrackedHandJoint index)
        {
            switch (index)
            {
                case TrackedHandJoint.Palm:
                    return XRHandJointID.Palm;
                case TrackedHandJoint.Wrist:
                    return XRHandJointID.Wrist;
                case TrackedHandJoint.ThumbMetacarpal:
                    return XRHandJointID.ThumbMetacarpal;
                case TrackedHandJoint.ThumbProximal:
                    return XRHandJointID.ThumbProximal;
                case TrackedHandJoint.ThumbDistal:
                    return XRHandJointID.ThumbDistal;
                case TrackedHandJoint.ThumbTip:
                    return XRHandJointID.ThumbTip;
                case TrackedHandJoint.IndexMetacarpal:
                    return XRHandJointID.IndexMetacarpal;
                case TrackedHandJoint.IndexProximal:
                    return XRHandJointID.IndexProximal;
                case TrackedHandJoint.IndexIntermediate:
                    return XRHandJointID.IndexIntermediate;
                case TrackedHandJoint.IndexDistal:
                    return XRHandJointID.IndexDistal;
                case TrackedHandJoint.IndexTip:
                    return XRHandJointID.IndexTip;
                case TrackedHandJoint.MiddleMetacarpal:
                    return XRHandJointID.MiddleMetacarpal;
                case TrackedHandJoint.MiddleProximal:
                    return XRHandJointID.MiddleProximal;
                case TrackedHandJoint.MiddleIntermediate:
                    return XRHandJointID.MiddleIntermediate;
                case TrackedHandJoint.MiddleDistal:
                    return XRHandJointID.MiddleDistal;
                case TrackedHandJoint.MiddleTip:
                    return XRHandJointID.MiddleTip;
                case TrackedHandJoint.RingMetacarpal:
                    return XRHandJointID.RingMetacarpal;
                case TrackedHandJoint.RingProximal:
                    return XRHandJointID.RingProximal;
                case TrackedHandJoint.RingIntermediate:
                    return XRHandJointID.RingIntermediate;
                case TrackedHandJoint.RingDistal:
                    return XRHandJointID.RingDistal;
                case TrackedHandJoint.RingTip:
                    return XRHandJointID.RingTip;
                case TrackedHandJoint.LittleMetacarpal:
                    return XRHandJointID.LittleMetacarpal;
                case TrackedHandJoint.LittleProximal:
                    return XRHandJointID.LittleProximal;
                case TrackedHandJoint.LittleIntermediate:
                    return XRHandJointID.LittleIntermediate;
                case TrackedHandJoint.LittleDistal:
                    return XRHandJointID.LittleDistal;
                case TrackedHandJoint.LittleTip:
                    return XRHandJointID.LittleTip;
                case TrackedHandJoint.TotalJoints:
                    return XRHandJointID.LittleTip + 1;
                default:
                    return (XRHandJointID)0;
            }
        }

        public static TrackedHandJoint UnityHandToTrackedHand(XRHandJointID index)
        {
            switch (index)
            {
                case XRHandJointID.Palm:
                    return TrackedHandJoint.Palm;
                case XRHandJointID.Wrist:
                    return TrackedHandJoint.Wrist;
                case XRHandJointID.ThumbMetacarpal:
                    return TrackedHandJoint.ThumbMetacarpal;
                case XRHandJointID.ThumbProximal:
                    return TrackedHandJoint.ThumbProximal;
                case XRHandJointID.ThumbDistal:
                    return TrackedHandJoint.ThumbDistal;
                case XRHandJointID.ThumbTip:
                    return TrackedHandJoint.ThumbTip;
                case XRHandJointID.IndexMetacarpal:
                    return TrackedHandJoint.IndexMetacarpal;
                case XRHandJointID.IndexProximal:
                    return TrackedHandJoint.IndexProximal;
                case XRHandJointID.IndexIntermediate:
                    return TrackedHandJoint.IndexIntermediate;
                case XRHandJointID.IndexDistal:
                    return TrackedHandJoint.IndexDistal;
                case XRHandJointID.IndexTip:
                    return TrackedHandJoint.IndexTip;
                case XRHandJointID.MiddleMetacarpal:
                    return TrackedHandJoint.MiddleMetacarpal;
                case XRHandJointID.MiddleProximal:
                    return TrackedHandJoint.MiddleProximal;
                case XRHandJointID.MiddleIntermediate:
                    return TrackedHandJoint.MiddleIntermediate;
                case XRHandJointID.MiddleDistal:
                    return TrackedHandJoint.MiddleDistal;
                case XRHandJointID.MiddleTip:
                    return TrackedHandJoint.MiddleTip;
                case XRHandJointID.RingMetacarpal:
                    return TrackedHandJoint.RingMetacarpal;
                case XRHandJointID.RingProximal:
                    return TrackedHandJoint.RingProximal;
                case XRHandJointID.RingIntermediate:
                    return TrackedHandJoint.RingIntermediate;
                case XRHandJointID.RingDistal:
                    return TrackedHandJoint.RingDistal;
                case XRHandJointID.RingTip:
                    return TrackedHandJoint.RingTip;
                case XRHandJointID.LittleMetacarpal:
                    return TrackedHandJoint.LittleMetacarpal;
                case XRHandJointID.LittleProximal:
                    return TrackedHandJoint.LittleProximal;
                case XRHandJointID.LittleIntermediate:
                    return TrackedHandJoint.LittleIntermediate;
                case XRHandJointID.LittleDistal:
                    return TrackedHandJoint.LittleDistal;
                case XRHandJointID.LittleTip:
                    return TrackedHandJoint.LittleTip;
                default:
                    return (TrackedHandJoint)0;
            }
        }
    }
}