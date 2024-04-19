using Project.Gaze;
using UnityEngine;

namespace Project.Hands
{
    public class HandAimProvider : HandPoseProvider
    {
        [SerializeField] private GazePoseProvider m_GazePoseProvider;
        [SerializeField] private Vector3 m_AimOffset;

        private HandRay m_HandRay = new HandRay(0.02f);
        private HandJointPose m_Knuckle;
        private HandJointPose m_Palm;



        public override bool TryGetPose(out Pose pose)
        {
            if(m_GazePoseProvider.IsGazeReady)
            {
                return m_GazePoseProvider.TryGetPose(out pose);
            }

            bool poseRetrieved = true;
            poseRetrieved &= TryGetJoint(TrackedHandJoint.IndexProximal, out m_Knuckle);
            poseRetrieved &= TryGetJoint(TrackedHandJoint.Palm, out m_Palm);

            // Tick the hand ray generator function. Uses index knuckle for position.
            if (poseRetrieved)
            {
                var cameraTransform = Camera.main.transform;
                var aimPosition = m_Knuckle.Position + cameraTransform.TransformDirection(m_AimOffset);

                m_HandRay.Update(aimPosition, -m_Palm.Up, Camera.main.transform, m_TargetHand);

                pose = new Pose(
                    m_HandRay.Ray.origin,
                    Quaternion.LookRotation(m_HandRay.Ray.direction, m_Palm.Up));
            }
            else
            {
                pose = Pose.identity;
            }

            return poseRetrieved;
        }
    }
}