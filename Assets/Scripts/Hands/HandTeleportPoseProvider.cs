using Project.Core;
using UnityEngine;

namespace Project.Hands
{
    public class HandTeleportPoseProvider : HandPoseProvider
    {
        [SerializeField] private float m_RayHalfLife = 0.05f;

        private HandJointPose m_IndexFingerPose;
        private StabilizedRay m_TeleportRay;
        private Ray m_OriginRay = new Ray();



        public override bool TryGetPose(out Pose pose)
        {
            bool poseRetrieved = TryGetJoint(TrackedHandJoint.IndexTip, out m_IndexFingerPose);

            // Tick the hand ray generator function. Uses index knuckle for position.
            if (poseRetrieved)
            {
                m_OriginRay.origin = m_IndexFingerPose.Position;
                m_OriginRay.direction = m_IndexFingerPose.Forward;

                m_TeleportRay.AddSample(m_OriginRay);

                pose = new Pose(m_TeleportRay.StabilizedPosition, Quaternion.LookRotation(m_TeleportRay.StabilizedDirection, Vector3.up));
            }
            else
            {
                pose = Pose.identity;
            }

            return poseRetrieved;
        }

        private void OnEnable()
        {
            m_TeleportRay = new StabilizedRay(m_RayHalfLife);
        }
    }
}