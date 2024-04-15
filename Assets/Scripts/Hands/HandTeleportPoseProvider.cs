using Project.Core;
using UnityEngine;

namespace Project.Hands
{
    public class HandTeleportPoseProvider : HandPoseProvider
    {
        [SerializeField] private float m_RayHalfLife = 0.05f;
        [SerializeField] private Vector3 m_RayAngleOffset = new Vector3(0.0f, -0.05f, 0.0f);

        private HandJointPose m_TargetPose;
        private StabilizedRay m_TeleportRay;
        private Ray m_OriginRay = new Ray();



        public override bool TryGetPose(out Pose pose)
        {
            bool poseRetrieved = TryGetJoint(TrackedHandJoint.Palm, out m_TargetPose);

            // Tick the hand ray generator function. Uses index knuckle for position.
            if (poseRetrieved)
            {
                m_OriginRay.origin = m_TargetPose.Position;
                m_OriginRay.direction = (m_TargetPose.Forward + m_RayAngleOffset).normalized;

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