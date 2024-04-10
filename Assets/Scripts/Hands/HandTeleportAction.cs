using Project.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace Project.Hands
{
    public class HandTeleportAction : MonoBehaviour
    {
        public event Action<bool> OnStateChanged;

        public bool State
        {
            get => m_State;
            set
            {
                if (m_State == value)
                    return;

                m_State = value;
                OnStateChanged?.Invoke(m_State);
            }
        }

        [SerializeField] private HandPoseProvider m_HandPoseProvider;
        [SerializeField] private AdaptiveRayInteractor m_TargetRayInteractor;
        [SerializeField] private GameObject m_TeleportControlsVisual;
        [SerializeField, Tooltip("Threshold for the index finger pointing outwards")]
        private float m_IndexFingerDirectionThreshold = 0.65f;
        [SerializeField, Tooltip("Threshold for the rest of the fingers to point inwards")]
        private float m_OtherFingersDirectionThreshold = -0.25f;
        [SerializeField, Tooltip("Threshold for the thumb finger, passing this threshold activates the teleport")]
        private float m_ThumbFingerDirectionThreshold = -0.3f;
        [SerializeField] private float m_DirectionLerpSpeed = 0.25f;

        [SerializeField] private InputActionProperty m_ActivateTeleportAction;
        [SerializeField] private InputActionProperty m_InvokeTeleportAction;
        [SerializeField] private TMP_Text m_Debug;

        private XRInputButtonReader m_TargetRayActivateTeleportInput;
        private XRInputButtonReader m_TargetRayInvokeTeleportInput;
        private bool m_TeleportWasActiveLastFrame;
        private bool m_TeleportInvokedLastFrame;
        private bool m_State = true;
        private float m_IndexDirection;
        private float m_MiddleDirection;
        private float m_RingDirection;
        private float m_PinkyDirection;
        private float m_ThumbDirection;
        private LineRenderer m_TeleportControlsLine;



        private void OnEnable()
        {
            m_TargetRayActivateTeleportInput = m_TargetRayInteractor.selectInput;
            m_TargetRayInvokeTeleportInput = m_TargetRayInteractor.activateInput;
            m_TeleportControlsLine = m_TeleportControlsVisual.GetComponent<LineRenderer>();
        }

        private void Update()
        {
            var isFoundFingers = m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out var palmPose);

            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.IndexIntermediate, palmPose, out var indexDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.MiddleIntermediate, palmPose, out var middleDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.RingIntermediate, palmPose, out var ringDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.LittleIntermediate, palmPose, out var pinkyDirection);
            isFoundFingers &= m_HandPoseProvider.TryGetJoint(TrackedHandJoint.ThumbTip, out var thumbTipPose);
            isFoundFingers &= TryGetThumbDirection(out var thumbDirection);

            DebugStuff(indexDirection, middleDirection, ringDirection, pinkyDirection, thumbDirection);

            if (!isFoundFingers)
            {
                m_IndexDirection 
                    = m_MiddleDirection 
                    = m_RingDirection 
                    = m_PinkyDirection 
                    = m_ThumbDirection = 0f;
                return;
            }

            m_IndexDirection = Mathf.Lerp(m_IndexDirection, indexDirection, m_DirectionLerpSpeed);
            m_MiddleDirection = Mathf.Lerp(m_MiddleDirection, middleDirection, m_DirectionLerpSpeed);
            m_RingDirection = Mathf.Lerp(m_RingDirection, ringDirection, m_DirectionLerpSpeed);
            m_PinkyDirection = Mathf.Lerp(m_PinkyDirection, pinkyDirection, m_DirectionLerpSpeed);
            m_ThumbDirection = Mathf.Lerp(m_ThumbDirection, thumbDirection, m_DirectionLerpSpeed);

            var isTeleportActive = m_IndexDirection > m_IndexFingerDirectionThreshold
                && m_MiddleDirection < m_OtherFingersDirectionThreshold
                && m_RingDirection < m_OtherFingersDirectionThreshold
                && m_PinkyDirection < m_OtherFingersDirectionThreshold;

            // display the teleport ray visuals but not activate the teleport

            var wasTeleportActive = isTeleportActive;

            if(isTeleportActive)
            {
                m_TargetRayInteractor.gameObject.SetActive(true);
                m_TeleportControlsVisual.SetActive(true);
            }

            m_TeleportControlsLine.SetPosition(0, m_TeleportControlsVisual.transform.position);
            m_TeleportControlsLine.SetPosition(1, thumbTipPose.Position);

            isTeleportActive &= !(m_ThumbDirection <= (m_TeleportInvokedLastFrame ? m_ThumbFingerDirectionThreshold - 0.1f : m_ThumbFingerDirectionThreshold));

            if(wasTeleportActive)
            {
                // Only send data to teleport value reader if teleport is pre-activated
                m_TargetRayActivateTeleportInput.QueueManualState(isTeleportActive,
                    isTeleportActive ? 1.0f : 0.0f,
                    isTeleportActive && !m_TeleportWasActiveLastFrame,
                    !isTeleportActive && m_TeleportWasActiveLastFrame);
            }

            if (!m_TeleportWasActiveLastFrame)
            {
                m_TargetRayInteractor.gameObject.SetActive(false);
                m_TeleportControlsVisual.SetActive(false);
            }

            //if (isTeleportActive)
            //{
            //    var isInvoked = thumbDirection <= (m_TeleportInvokedLastFrame ? m_ThumbFingerDirectionThreshold - 0.1f : m_ThumbFingerDirectionThreshold);

            //    if (!(m_InvokeTeleportAction.action?.controls.Count > 0))
            //    {
            //        m_TargetRayInvokeTeleportInput.QueueManualState(isInvoked,
            //            1.0f,
            //            isInvoked && !m_TeleportInvokedLastFrame,
            //            !isInvoked && m_TeleportInvokedLastFrame);
            //    }

            //    m_TeleportInvokedLastFrame = isInvoked;
            //}
            //else
            //{
            //    if (!(m_InvokeTeleportAction.action?.controls.Count > 0))
            //    {
            //        m_TargetRayInvokeTeleportInput.QueueManualState(false,
            //            0.0f,
            //            false && !m_TeleportInvokedLastFrame,
            //            !false && m_TeleportInvokedLastFrame);
            //    }

            //    m_TeleportInvokedLastFrame = false;
            //}

            m_TeleportWasActiveLastFrame = isTeleportActive;
        }

        private void DebugStuff(float indexDirection, float middleDirection, float ringDirection, float pinkyDirection, float thumbDirection)
        {
            if (m_Debug == null)
                return;

            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"Index: {indexDirection.ToString("0.##")}");
            sb.AppendLine($"Middle: {middleDirection.ToString("0.##")}");
            sb.AppendLine($"Ring: {ringDirection.ToString("0.##")}");
            sb.AppendLine($"Pinky: {pinkyDirection.ToString("0.##")}");
            sb.AppendLine($"Thumb: {thumbDirection.ToString("0.##")}");

            m_Debug.text = sb.ToString();
        }

        private bool TryGetFingerDirection(TrackedHandJoint finger, HandJointPose palmRef, out float direction)
        {
            bool gotData = m_HandPoseProvider.TryGetJoint(finger, out var fingerPose);

            if (gotData)
            {
                var cameraForward = palmRef.Forward;
                var fingerDirection = fingerPose.Forward;

                direction = Vector3.Dot(fingerDirection, cameraForward);

                return true;
            }

            direction = 0f;
            return false;
        }

        private bool TryGetThumbDirection(out float direction)
        {
            bool gotData = m_HandPoseProvider.TryGetJoint(TrackedHandJoint.ThumbProximal, out var fingerPose);
            gotData &= m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out var palmPose);

            if (gotData)
            {
                var palmDirection = palmPose.Up;
                var fingerDirection = fingerPose.Forward;

                direction = Vector3.Dot(fingerDirection, palmDirection);

                return true;
            }

            direction = 0f;
            return false;
        }
    }
}