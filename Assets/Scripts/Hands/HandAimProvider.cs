using UnityEngine;

namespace Project.Hands
{
    public class HandAimProvider : HandPoseProvider
    {
        private HandRay m_HandRay = new HandRay();
        private HandJointPose m_Knuckle;
        private HandJointPose m_Palm;



        public override bool TryGetPose(out Pose pose)
        {
            bool poseRetrieved = true;
            poseRetrieved &= TryGetJoint(TrackedHandJoint.IndexProximal, out m_Knuckle);
            poseRetrieved &= TryGetJoint(TrackedHandJoint.Palm, out m_Palm);

            // Tick the hand ray generator function. Uses index knuckle for position.
            if (poseRetrieved)
            {
                m_HandRay.Update(m_Knuckle.Position, -m_Palm.Up, Camera.main.transform, m_TargetHand);

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