using UnityEngine;

namespace Project.Hands
{
    public class HandAimProvider : HandPoseProvider
    {
        [SerializeField] private float m_HandFacingAwayTolerance;

        private HandRay handRay = new HandRay();
        private HandJointPose knuckle;
        private HandJointPose palm;



        public override bool TryGetPose(out Pose pose)
        {
            bool poseRetrieved = true;
            poseRetrieved &= TryGetJoint(TrackedHandJoint.IndexProximal, out knuckle);
            poseRetrieved &= TryGetJoint(TrackedHandJoint.Palm, out palm);

            // Tick the hand ray generator function. Uses index knuckle for position.
            if (poseRetrieved)
            {
                handRay.Update(knuckle.Position, -palm.Up, Camera.main.transform, m_TargetHand);

                pose = new Pose(
                    handRay.Ray.origin,
                    Quaternion.LookRotation(handRay.Ray.direction, palm.Up));
            }
            else
            {
                pose = Pose.identity;
            }

            return poseRetrieved;
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

        private bool IsPalmFacingAway(HandJointPose palmJoint)
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