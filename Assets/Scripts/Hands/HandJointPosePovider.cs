using Project.Core;
using UnityEngine;

namespace Project.Hands
{
    public class HandJointPosePovider : HandPoseProvider
    {
        [SerializeField]
        [Tooltip("The specific joint whose pose we are retrieving.")]
        private TrackedHandJoint joint;

        /// <summary>
        /// The specific joint whose pose we are retrieving.
        /// </summary>
        public TrackedHandJoint Joint { get => joint; set => joint = value; }

        /// <summary>
        /// Tries to get the pose of a specific hand joint on a specific hand in worldspace.
        /// </summary>
        public override bool TryGetPose(out Pose pose)
        {
            if (TryGetJoint(Joint, out HandJointPose handJointPose))
            {
                // Hand Joints are already returned by the subsystem in worldspace, we don't have to do any transformations
                pose.position = handJointPose.Position;
                pose.rotation = handJointPose.Rotation;
                return true;
            }

            pose = Pose.identity;
            return false;
        }
    }
}