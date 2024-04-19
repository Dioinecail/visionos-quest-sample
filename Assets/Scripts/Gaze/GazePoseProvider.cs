using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Core;

#if UNITY_EDITOR || UNITY_VISIONOS
using UnityEngine.XR.VisionOS;
using UnityEngine.XR.VisionOS.InputDevices;
#endif

namespace Project.Gaze
{
    public class GazePoseProvider : PoseProvider
    {
        public bool IsGazeReady => m_IsActive;

        private bool m_IsActive;

#if UNITY_EDITOR || UNITY_VISIONOS
        PointerInput m_PointerInput;

        public override bool TryGetPose(out Pose aimPose)
        {
            var primaryTouch = m_PointerInput.Default.PrimaryPointer.ReadValue<VisionOSSpatialPointerState>();
            var phase = primaryTouch.phase;
            var began = phase == VisionOSSpatialPointerPhase.Began;
            var active = began || phase == VisionOSSpatialPointerPhase.Moved;

            if (began)
            {
                aimPose = new Pose(primaryTouch.startRayOrigin, primaryTouch.startRayRotation);

                return true;
            }

            if (active)
            {
                aimPose = new Pose(primaryTouch.inputDevicePosition, primaryTouch.inputDeviceRotation);

                return true;
            }

            aimPose = Pose.identity;
            return false;
        }

        void OnEnable()
        {
            m_PointerInput ??= new PointerInput();
            m_PointerInput.Enable();
            m_IsActive = true;
        }

        void OnDisable()
        {
            m_PointerInput.Disable();
        }

#else
        public override bool TryGetPose(out Pose aimPose)
        {
            aimPose = Pose.identity;
            return false;
        }
#endif
    }
}